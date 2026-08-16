using Ptn.ApiContractChecker.Constants.Runs.Lookups;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Entities;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Ptn.ApiContractChecker.Data;

// islevi: CheckRunStatus lookup satirlarini kararli kodlarindan idempotent olarak uretir.
// sistemdeki gorevi: Pending durumundan terminal durumlara kadar run yasam dongusunun veri sozlugunu hazirlar.
public class CheckRunStatusDataSeedContributor : LookupDataSeedContributor<CheckRunStatus>
{
    private static readonly IReadOnlyDictionary<string, string> Rows =
        new Dictionary<string, string>
        {
            [CheckRunStatusCodes.Pending] = CheckRunStatusNames.Pending,
            [CheckRunStatusCodes.Running] = CheckRunStatusNames.Running,
            [CheckRunStatusCodes.Completed] = CheckRunStatusNames.Completed,
            [CheckRunStatusCodes.Failed] = CheckRunStatusNames.Failed,
            [CheckRunStatusCodes.Partial] = CheckRunStatusNames.Partial
        };

    protected override IReadOnlyDictionary<string, string> DesiredRows => Rows;

    public CheckRunStatusDataSeedContributor(
        IRepository<CheckRunStatus, Guid> repository,
        IGuidGenerator guidGenerator,
        IDataFilter<IPassivable> passivableFilter)
        : base(repository, guidGenerator, passivableFilter)
    {
    }

    // Ortak lookup alanlariyla aktif run durum satiri kurar.
    protected override CheckRunStatus CreateEntity(Guid id, string code, string name)
    {
        return new CheckRunStatus(id, code, name);
    }
}
