using System.Net;
using System.Text.Json;
using Ptn.ApiContractChecker.Dtos.Sources;
using Ptn.ApiContractChecker.Entities.Sources;
using Ptn.ApiContractChecker.ExceptionCodes.Sources;
using Ptn.ApiContractChecker.Interface.Sources;
using Ptn.ApiContractChecker.Managers.Sources;
using Ptn.ApiContractChecker.Secrets;
using Ptn.ApiContractChecker.Services.Sources;
using Shouldly;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Sources;

// islevi: SpecSource repository gorunurlugu, Vault create sirasi, reachability ve pasiflestirme davranislarini gercek EF saglayicisiyla dogrular.
// sistemdeki gorevi: Tenant sizintisi, secret response sizintisi ve passivate-sonrasi 404 regresyonlarini tek entegrasyon kapisinda engeller.
[Collection(EfCoreIntegrationCollection.Name)]
public class SpecSourceRepository_Tests : ApiContractCheckerEntityFrameworkCoreTestBase
{
    private readonly ISpecSourceAppService _appService;
    private readonly ISpecSourceRepository _repository;
    private readonly SpecSourceManager _manager;
    private readonly ICurrentTenant _currentTenant;
    private readonly InMemorySpecSourceSecretProvider _secretProvider;
    private readonly SpecSourceHttpClientFactory _httpClientFactory;

    public SpecSourceRepository_Tests()
    {
        _appService = GetRequiredService<ISpecSourceAppService>();
        _repository = GetRequiredService<ISpecSourceRepository>();
        _manager = GetRequiredService<SpecSourceManager>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
        _secretProvider = (InMemorySpecSourceSecretProvider)GetRequiredService<Ptn.ApiContractChecker.Interface.Secrets.ISecretProvider>();
        _httpClientFactory = (SpecSourceHttpClientFactory)GetRequiredService<IHttpClientFactory>();
    }

    // Tenant A kaynaginin tenant B AppService sorgusunda NotFound oldugunu kanitlar.
    [Fact]
    public async Task Other_Tenant_Source_Should_Be_NotFound()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var sourceId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantA))
            {
                var source = new SpecSource(sourceId, "orders", "https://orders.test", null, tenantA);
                _manager.AddDocument(source, Guid.NewGuid(), "v1", "/openapi/v1.json");
                await _repository.InsertAsync(source, autoSave: true);
            }
        });

        using (_currentTenant.Change(tenantB))
        {
            await Should.ThrowAsync<EntityNotFoundException>(() => _appService.GetAsync(sourceId));
        }
    }

    // Create'in credential'i kaynak kimligine bagli Vault yoluna yazdigini ve response'ta sizdirmadigini kanitlar.
    [Fact]
    public async Task Create_Should_Write_Credential_And_Return_A_Secretless_Response()
    {
        var tenantId = Guid.NewGuid();
        var input = BuildCreateInput();

        SpecSourceDto dto;
        using (_currentTenant.Change(tenantId))
        {
            dto = await _appService.CreateAsync(input);
        }

        var expectedPath = SecretPathBuilder.Build(tenantId, dto.Id);
        _secretProvider.Credentials.ContainsKey(expectedPath).ShouldBeTrue();
        var json = JsonSerializer.Serialize(dto);
        json.ShouldNotContain(input.HeaderValue!);
        json.ShouldNotContain(expectedPath);
        dto.Documents.Count.ShouldBe(1);
    }

    // Reachability testinin Vault header'ini request bellekte kullanip secretsiz basari sonucu dondurdugunu kanitlar.
    [Fact]
    public async Task Reachability_Should_Use_Vault_Credential_Without_Returning_It()
    {
        var tenantId = Guid.NewGuid();
        var input = BuildCreateInput();
        SpecSourceDto source;
        using (_currentTenant.Change(tenantId))
        {
            source = await _appService.CreateAsync(input);
            var result = await _appService.TestReachabilityAsync(source.Id);
            result.IsReachable.ShouldBeTrue();
            result.TestedDocumentCount.ShouldBe(1);
            JsonSerializer.Serialize(result).ShouldNotContain(input.HeaderValue!);
        }

        _httpClientFactory.ObservedHeaderName.ShouldBe(input.HeaderName);
        _httpClientFactory.ObservedHeaderValue.ShouldBe(input.HeaderValue);
        _httpClientFactory.ObservedClientName.ShouldBe(Ptn.ApiContractChecker.Constants.Sources.SpecSourceConsts.HttpClientName);
    }

    // Ag hatasinin yanita ham exception metni degil cekim hattiyla ortak kararli kod olarak dondugunu kanitlar.
    [Fact]
    public async Task Unreachable_Source_Should_Report_Stable_Code_Without_Transport_Detail()
    {
        const string transportDetail = "No such host is known (orders.test:443)";
        var tenantId = Guid.NewGuid();
        var input = BuildCreateInput();

        using (_currentTenant.Change(tenantId))
        {
            var source = await _appService.CreateAsync(input);
            _httpClientFactory.ConfigureException(new HttpRequestException(transportDetail));

            var result = await _appService.TestReachabilityAsync(source.Id);

            result.IsReachable.ShouldBeFalse();
            result.ErrorMessage.ShouldBe(SpecSourceExceptionCodes.FetchNetworkFailed);
            JsonSerializer.Serialize(result).ShouldNotContain(transportDetail);
        }
    }

    // Basarisiz HTTP durumunun ReasonPhrase yerine kararli durum hata koduyla raporlandigini kanitlar.
    [Fact]
    public async Task Failing_Status_Should_Report_Stable_Status_Code()
    {
        var tenantId = Guid.NewGuid();
        var input = BuildCreateInput();

        using (_currentTenant.Change(tenantId))
        {
            var source = await _appService.CreateAsync(input);
            _httpClientFactory.ConfigureResponse(HttpStatusCode.NotFound, [], "application/json");

            var result = await _appService.TestReachabilityAsync(source.Id);

            result.IsReachable.ShouldBeFalse();
            result.StatusCode.ShouldBe((int)HttpStatusCode.NotFound);
            result.ErrorMessage.ShouldBe(SpecSourceExceptionCodes.FetchHttpStatusRejected);
        }
    }

    // Credential alani verilmeyen update'in mevcut Vault secret'ini koruyup aggregate dokumanini guncelledigini kanitlar.
    [Fact]
    public async Task Update_Without_Credential_Should_Preserve_Existing_Secret()
    {
        var tenantId = Guid.NewGuid();
        var createInput = BuildCreateInput();
        using (_currentTenant.Change(tenantId))
        {
            var source = await _appService.CreateAsync(createInput);
            var secretPath = SecretPathBuilder.Build(tenantId, source.Id);
            var existingCredential = _secretProvider.Credentials[secretPath];
            var document = source.Documents.Single();
            var updateInput = new UpdateSpecSourceDto
            {
                Name = source.Name + "-updated",
                BaseUrl = source.BaseUrl,
                Documents =
                [
                    new SpecDocumentDto
                    {
                        Id = document.Id,
                        DocumentName = document.DocumentName,
                        Path = "/openapi/v1-updated.json",
                        IsActive = true
                    }
                ]
            };

            var updated = await _appService.UpdateAsync(source.Id, updateInput);

            updated.Name.ShouldBe(updateInput.Name);
            updated.Documents.Single().Path.ShouldBe(updateInput.Documents.Single().Path);
            _secretProvider.Credentials[secretPath].ShouldBeSameAs(existingCredential);
        }
    }

    // Pasiflestirme cevabinin yeniden okumadan dondugunu ve sonrasinda global filtreden gizlendigini kanitlar.
    [Fact]
    public async Task Passivate_Should_Return_Loaded_Source_Then_Hide_It()
    {
        var tenantId = Guid.NewGuid();
        SpecSourceDto source;
        using (_currentTenant.Change(tenantId))
        {
            source = await _appService.CreateAsync(BuildOpenCreateInput());
            var passivated = await _appService.PassivateAsync(source.Id);
            passivated.IsActive.ShouldBeFalse();
            (await _repository.FindWithDetailsAsync(source.Id)).ShouldBeNull();
        }
    }

    // Credential'li gecerli kaynak create girdisini tek yerde kurar.
    private static CreateSpecSourceDto BuildCreateInput()
    {
        var input = BuildOpenCreateInput();
        input.HeaderName = "Authorization";
        input.HeaderValue = "Bearer integration-secret";
        return input;
    }

    // Secretsiz gecerli kaynak create girdisini tek yerde kurar.
    private static CreateSpecSourceDto BuildOpenCreateInput()
    {
        return new CreateSpecSourceDto
        {
            Name = $"orders-{Guid.NewGuid():N}",
            BaseUrl = "https://orders.test",
            Documents =
            [
                new SpecDocumentDto
                {
                    DocumentName = "v1",
                    Path = "/openapi/v1.json"
                }
            ]
        };
    }
}
