using Ptn.ApiContractChecker.Constants.Formats.Lookups;
using Ptn.ApiContractChecker.Entities;
using Ptn.ApiContractChecker.Entities.Lookups;
using Shouldly;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.DataModel;

// islevi: Bes lookup seed katalogunun tamligini ve pasif satir idempotansini dogrular.
// sistemdeki gorevi: Kod sabitleriyle veritabani satirlarinin eksik veya cogaltilmis kalmasini engeller.
[Collection(EfCoreIntegrationCollection.Name)]
public class LookupSeed_Tests : ApiContractCheckerEntityFrameworkCoreTestBase
{
    private readonly IRepository<SpecFormat, Guid> _specFormatRepository;
    private readonly IRepository<CheckRunStatus, Guid> _statusRepository;
    private readonly IRepository<DifferenceSeverity, Guid> _severityRepository;
    private readonly IRepository<DifferenceDirection, Guid> _directionRepository;
    private readonly IRepository<DifferenceKind, Guid> _kindRepository;
    private readonly IDataFilter<IPassivable> _passivableFilter;
    private readonly IDataSeeder _dataSeeder;

    public LookupSeed_Tests()
    {
        _specFormatRepository = GetRequiredService<IRepository<SpecFormat, Guid>>();
        _statusRepository = GetRequiredService<IRepository<CheckRunStatus, Guid>>();
        _severityRepository = GetRequiredService<IRepository<DifferenceSeverity, Guid>>();
        _directionRepository = GetRequiredService<IRepository<DifferenceDirection, Guid>>();
        _kindRepository = GetRequiredService<IRepository<DifferenceKind, Guid>>();
        _passivableFilter = GetRequiredService<IDataFilter<IPassivable>>();
        _dataSeeder = GetRequiredService<IDataSeeder>();
    }

    // Seed ikinci kez calistiginda pasif OAS 3.2 dahil hicbir lookup satirinin cogalmadigini dogrular.
    [Fact]
    public async Task Lookup_Seeds_Should_Be_Complete_And_Idempotent()
    {
        await WithUnitOfWorkAsync(() => _dataSeeder.SeedAsync());

        await WithUnitOfWorkAsync(async () =>
        {
            using (_passivableFilter.Disable())
            {
                (await _specFormatRepository.GetCountAsync()).ShouldBe(4);
                (await _statusRepository.GetCountAsync()).ShouldBe(5);
                (await _severityRepository.GetCountAsync()).ShouldBe(3);
                (await _directionRepository.GetCountAsync()).ShouldBe(4);
                (await _kindRepository.GetCountAsync()).ShouldBe(16);

                var openApi32 = await _specFormatRepository.SingleAsync(
                    format => format.Code == SpecFormatCodes.OpenApi32);
                openApi32.IsActive.ShouldBeFalse();
            }
        });
    }
}
