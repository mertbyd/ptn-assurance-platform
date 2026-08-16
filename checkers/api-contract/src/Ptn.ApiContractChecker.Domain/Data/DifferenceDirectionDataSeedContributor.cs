using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Entities;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Ptn.ApiContractChecker.Data;

// islevi: DifferenceDirection lookup satirlarini kararli kodlarindan idempotent olarak uretir.
// sistemdeki gorevi: Request, response, endpoint ve dokumantasyon fark yonlerinin veri sozlugunu hazirlar.
public class DifferenceDirectionDataSeedContributor : LookupDataSeedContributor<DifferenceDirection>
{
    private static readonly IReadOnlyDictionary<string, string> Rows =
        new Dictionary<string, string>
        {
            [DifferenceDirectionCodes.Request] = DifferenceDirectionNames.Request,
            [DifferenceDirectionCodes.Response] = DifferenceDirectionNames.Response,
            [DifferenceDirectionCodes.Endpoint] = DifferenceDirectionNames.Endpoint,
            [DifferenceDirectionCodes.Documentation] = DifferenceDirectionNames.Documentation
        };

    protected override IReadOnlyDictionary<string, string> DesiredRows => Rows;

    public DifferenceDirectionDataSeedContributor(
        IRepository<DifferenceDirection, Guid> repository,
        IGuidGenerator guidGenerator,
        IDataFilter<IPassivable> passivableFilter)
        : base(repository, guidGenerator, passivableFilter)
    {
    }

    // Ortak lookup alanlariyla aktif fark yonu satiri kurar.
    protected override DifferenceDirection CreateEntity(Guid id, string code, string name)
    {
        return new DifferenceDirection(id, code, name);
    }
}
