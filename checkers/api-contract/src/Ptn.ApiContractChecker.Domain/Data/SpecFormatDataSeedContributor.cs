using Ptn.ApiContractChecker.Constants.Formats.Lookups;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Entities;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Ptn.ApiContractChecker.Data;

// islevi: SpecFormat lookup satirlarini kararli kodlarindan idempotent olarak uretir.
// sistemdeki gorevi: Swagger/OAS okuyucu seciminin veri sozlugunu hazirlar; OAS 3.2 kanitlanana kadar pasif kalir.
public class SpecFormatDataSeedContributor : LookupDataSeedContributor<SpecFormat>
{
    private static readonly IReadOnlyDictionary<string, string> Rows =
        new Dictionary<string, string>
        {
            [SpecFormatCodes.Swagger20] = SpecFormatNames.Swagger20,
            [SpecFormatCodes.OpenApi30] = SpecFormatNames.OpenApi30,
            [SpecFormatCodes.OpenApi31] = SpecFormatNames.OpenApi31,
            [SpecFormatCodes.OpenApi32] = SpecFormatNames.OpenApi32
        };

    protected override IReadOnlyDictionary<string, string> DesiredRows => Rows;

    public SpecFormatDataSeedContributor(
        IRepository<SpecFormat, Guid> repository,
        IGuidGenerator guidGenerator,
        IDataFilter<IPassivable> passivableFilter)
        : base(repository, guidGenerator, passivableFilter)
    {
    }

    // Rezerve OAS 3.2 satirini KBP-603 destegi kanitlanana kadar pasif kurar.
    protected override SpecFormat CreateEntity(Guid id, string code, string name)
    {
        return new SpecFormat(id, code, name, isActive: code != SpecFormatCodes.OpenApi32);
    }
}
