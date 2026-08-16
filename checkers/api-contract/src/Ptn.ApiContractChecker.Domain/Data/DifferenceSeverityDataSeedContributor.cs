using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Entities;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Ptn.ApiContractChecker.Data;

// islevi: DifferenceSeverity lookup satirlarini kararli kodlarindan idempotent olarak uretir.
// sistemdeki gorevi: Run sayaclari ve bulgu raporlamasinin siddet sozlugunu hazirlar.
public class DifferenceSeverityDataSeedContributor : LookupDataSeedContributor<DifferenceSeverity>
{
    private static readonly IReadOnlyDictionary<string, string> Rows =
        new Dictionary<string, string>
        {
            [DifferenceSeverityCodes.Breaking] = DifferenceSeverityNames.Breaking,
            [DifferenceSeverityCodes.NonBreaking] = DifferenceSeverityNames.NonBreaking,
            [DifferenceSeverityCodes.DocsOnly] = DifferenceSeverityNames.DocsOnly
        };

    protected override IReadOnlyDictionary<string, string> DesiredRows => Rows;

    public DifferenceSeverityDataSeedContributor(
        IRepository<DifferenceSeverity, Guid> repository,
        IGuidGenerator guidGenerator,
        IDataFilter<IPassivable> passivableFilter)
        : base(repository, guidGenerator, passivableFilter)
    {
    }

    // Ortak lookup alanlariyla aktif fark siddeti satiri kurar.
    protected override DifferenceSeverity CreateEntity(Guid id, string code, string name)
    {
        return new DifferenceSeverity(id, code, name);
    }
}
