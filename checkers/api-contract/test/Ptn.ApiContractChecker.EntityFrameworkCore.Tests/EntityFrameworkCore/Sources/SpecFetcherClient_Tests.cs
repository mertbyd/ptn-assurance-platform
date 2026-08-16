using System.Collections;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Polly.Timeout;
using Ptn.ApiContractChecker.Constants.Sources;
using Ptn.ApiContractChecker.ExceptionCodes;
using Ptn.ApiContractChecker.ExceptionCodes.Sources;
using Ptn.ApiContractChecker.Interface.Sources;
using Ptn.ApiContractChecker.Models.Secrets;
using Ptn.ApiContractChecker.Models.Sources;
using Shouldly;
using Volo.Abp;
using Xunit;
using Ptn.ApiContractChecker.EntityFrameworkCore.Adapters.Sources;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Sources;

// islevi: Spec fetch adapterinin basari, guard, hata cevirisi, DI ve secret sizintisi davranislarini gercek ABP konteynerinde dogrular.
// sistemdeki gorevi: KBP-608 cekme hattinin soketsiz EF integration kabul kapisini olusturur.
[Collection(EfCoreIntegrationCollection.Name)]
public class SpecFetcherClient_Tests : ApiContractCheckerEntityFrameworkCoreTestBase
{
    private readonly ISpecFetcherClient _fetcher;
    private readonly SpecSourceHttpClientFactory _httpClientFactory;
    private readonly InMemorySpecSourceSecretProvider _secretProvider;

    public SpecFetcherClient_Tests()
    {
        _fetcher = GetRequiredService<ISpecFetcherClient>();
        _httpClientFactory = (SpecSourceHttpClientFactory)GetRequiredService<IHttpClientFactory>();
        _secretProvider = (InMemorySpecSourceSecretProvider)GetRequiredService<Ptn.ApiContractChecker.Interface.Secrets.ISecretProvider>();
    }

    // JSON ve YAML govdelerinin ham icerik, medya tipi ve gercek bayt boyutuyla donduruldugunu kanitlar.
    [Theory]
    [InlineData("{\"openapi\":\"3.1.0\"}", "application/json")]
    [InlineData("openapi: 3.1.0\ninfo: {}", "application/yaml")]
    public async Task Valid_Content_Should_Return_Bytes_Media_Type_And_Size(string body, string mediaType)
    {
        var bytes = Encoding.UTF8.GetBytes(body);
        _httpClientFactory.ConfigureResponse(HttpStatusCode.OK, bytes, mediaType);

        var result = await _fetcher.FetchAsync(BuildRequest());

        Encoding.UTF8.GetString(result.Content).ShouldBe(body);
        result.MediaType.ShouldBe(mediaType);
        result.ByteSize.ShouldBe(bytes.Length);
        _httpClientFactory.ObservedClientName.ShouldBe(SpecSourceConsts.HttpClientName);
    }

    // HTTP 401 ve 404 yanitlarinin icerik guard'larindan once ayni kararli durum hatasina donustugunu kanitlar.
    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.NotFound)]
    public async Task Failed_Status_Should_Throw_Status_Error(HttpStatusCode statusCode)
    {
        _httpClientFactory.ConfigureResponse(statusCode, "<html/>"u8.ToArray(), "text/html");

        var exception = await Should.ThrowAsync<BusinessException>(() => _fetcher.FetchAsync(BuildRequest()));

        exception.Code.ShouldBe(SpecSourceExceptionCodes.FetchHttpStatusRejected);
    }

    // Basarili HTML yanitinin spec medya tipi olarak kabul edilmedigini kanitlar.
    [Fact]
    public async Task Html_Content_Should_Throw_Content_Type_Error()
    {
        _httpClientFactory.ConfigureResponse(HttpStatusCode.OK, "<html/>"u8.ToArray(), "text/html");

        var exception = await Should.ThrowAsync<BusinessException>(() => _fetcher.FetchAsync(BuildRequest()));

        exception.Code.ShouldBe(SpecSourceExceptionCodes.FetchContentTypeRejected);
    }

    // Content-Length siniri bastan asiyorsa govdenin hic okunmadan reddedildigini kanitlar.
    [Fact]
    public async Task Oversized_Declared_Content_Should_Be_Rejected_Before_Read()
    {
        _httpClientFactory.ConfigureResponse(
            HttpStatusCode.OK,
            new byte[65],
            "application/json");

        var exception = await Should.ThrowAsync<BusinessException>(() => _fetcher.FetchAsync(BuildRequest()));

        exception.Code.ShouldBe(SpecSourceExceptionCodes.FetchContentTooLarge);
        _httpClientFactory.ContentBytesRead.ShouldBe(0);
    }

    // Content-Length yokken sinir ustu govdenin max + 1 baytta kesildigini kanitlar.
    [Fact]
    public async Task Oversized_Chunked_Content_Should_Stop_At_Overflow_Probe()
    {
        _httpClientFactory.ConfigureResponse(
            HttpStatusCode.OK,
            new byte[256],
            "application/json",
            includeContentLength: false);

        var exception = await Should.ThrowAsync<BusinessException>(() => _fetcher.FetchAsync(BuildRequest()));

        exception.Code.ShouldBe(SpecSourceExceptionCodes.FetchContentTooLarge);
        _httpClientFactory.ContentBytesRead.ShouldBe(65);
    }

    // Yalan dusuk Content-Length degerinin sinirli stream okumasini atlatamadigini kanitlar.
    [Fact]
    public async Task Misleading_Content_Length_Should_Stop_At_Overflow_Probe()
    {
        _httpClientFactory.ConfigureResponse(
            HttpStatusCode.OK,
            new byte[256],
            "application/json",
            declaredContentLength: 1);

        var exception = await Should.ThrowAsync<BusinessException>(() => _fetcher.FetchAsync(BuildRequest()));

        exception.Code.ShouldBe(SpecSourceExceptionCodes.FetchContentTooLarge);
        _httpClientFactory.ContentBytesRead.ShouldBe(65);
    }

    // Sifir baytlik basarili yanitin bos icerik hata koduyla reddedildigini kanitlar.
    [Fact]
    public async Task Empty_Content_Should_Throw_Empty_Content_Error()
    {
        _httpClientFactory.ConfigureResponse(HttpStatusCode.OK, [], "application/json");

        var exception = await Should.ThrowAsync<BusinessException>(() => _fetcher.FetchAsync(BuildRequest()));

        exception.Code.ShouldBe(SpecSourceExceptionCodes.FetchContentEmpty);
    }

    // Ag hatasinin sarmalanmadan yukari ciktigini kanitlar: sinir ABP exception middleware'idir,
    // arka plan isi yolunda da isin yeniden denenmesini bu davranis saglar.
    [Fact]
    public async Task Network_Failure_Should_Propagate_Unwrapped()
    {
        _httpClientFactory.ConfigureException(new HttpRequestException("provider failure"));

        await Should.ThrowAsync<HttpRequestException>(() => _fetcher.FetchAsync(BuildRequest()));
    }

    // Polly ve standart timeout exception'larinin da sarmalanmadan yukari ciktigini kanitlar.
    [Theory]
    [MemberData(nameof(TimeoutExceptions))]
    public async Task Timeout_Should_Propagate_Unwrapped(Exception timeout)
    {
        _httpClientFactory.ConfigureException(timeout);

        var thrown = await Should.ThrowAsync<Exception>(() => _fetcher.FetchAsync(BuildRequest()));

        thrown.ShouldBeOfType(timeout.GetType());
    }

    // Cekim yolunda karsilasilabilen iki farkli timeout tipini test verisine tasir.
    public static TheoryData<Exception> TimeoutExceptions =>
    [
        new TimeoutRejectedException("provider timeout"),
        new TimeoutException("provider timeout")
    ];

    // Credential'in request'e eklendigini ama basari sonucuna ya da hata mesaj/Data alanina sizmadigini kanitlar.
    [Fact]
    public async Task Credential_Should_Exist_Only_On_Request()
    {
        const string secretPath = "tenant/source";
        const string secretValue = "Bearer fetch-secret";
        await _secretProvider.SetAsync(
            secretPath,
            new ApiCredentialModel
            {
                HeaderName = "Authorization",
                HeaderValue = secretValue
            });
        _httpClientFactory.ConfigureResponse(HttpStatusCode.OK, "{}"u8.ToArray(), "application/json");

        var result = await _fetcher.FetchAsync(BuildRequest(secretPath));

        _httpClientFactory.ObservedHeaderName.ShouldBe("Authorization");
        _httpClientFactory.ObservedHeaderValue.ShouldBe(secretValue);
        JsonSerializer.Serialize(result).ShouldNotContain(secretValue);

        _httpClientFactory.ConfigureResponse(HttpStatusCode.Unauthorized, [], "application/json");
        var exception = await Should.ThrowAsync<BusinessException>(() => _fetcher.FetchAsync(BuildRequest(secretPath)));
        exception.Message.ShouldNotContain(secretValue);
        DataContains(exception.Data, secretValue).ShouldBeFalse();
    }

    // Secret deposu hatasinin kaynak erisim hatasi gibi maskelenmeden kendi koduyla yukari ciktigini kanitlar.
    [Fact]
    public async Task Secret_Store_Failure_Should_Not_Be_Reported_As_Fetch_Failure()
    {
        _httpClientFactory.ConfigureResponse(HttpStatusCode.OK, "{}"u8.ToArray(), "application/json");

        var exception = await Should.ThrowAsync<BusinessException>(
            () => _fetcher.FetchAsync(BuildRequest("tenant/missing-secret")));

        exception.Code.ShouldBe(SecretExceptionCodes.NotFound);
        _httpClientFactory.ObservedRequestUri.ShouldBeNull();
    }

    // Conventional DI kapali oldugu icin yalniz typed client kaydinin cozuldugunu kanitlar.
    [Fact]
    public void Fetcher_Should_Have_Exactly_One_Typed_Registration()
    {
        var registrations = GetRequiredService<IServiceProvider>().GetServices<ISpecFetcherClient>().ToList();

        registrations.Count.ShouldBe(1);
        registrations.Single().ShouldBeOfType<SpecFetcherClient>();
    }

    // Exception Data koleksiyonunda credential degerinin hicbir bicimde bulunmadigini denetler.
    private static bool DataContains(IDictionary data, string value)
        => data.Values.Cast<object?>().Any(item => item?.ToString()?.Contains(value, StringComparison.Ordinal) == true);

    // Test dokumani icin kararli URL ve opsiyonel secret yolunu kurar.
    private static SpecFetchRequestModel BuildRequest(string? secretPath = null)
        => new("https://orders.test", "/openapi/v1.json", secretPath);
}
