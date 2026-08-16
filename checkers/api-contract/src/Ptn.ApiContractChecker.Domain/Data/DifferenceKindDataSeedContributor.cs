using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Entities;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Ptn.ApiContractChecker.Data;

// islevi: DifferenceKind kapali katalogunu kararli kodlarindan idempotent olarak uretir.
// sistemdeki gorevi: Engine'in uretebildigi bulgu turlerini veritabani sozluguyle birebir hizalar.
public class DifferenceKindDataSeedContributor : LookupDataSeedContributor<DifferenceKind>
{
    private static readonly IReadOnlyDictionary<string, string> Rows =
        new Dictionary<string, string>
        {
            [DifferenceKindCodes.NewRequiredRequestProperty] = DifferenceKindNames.NewRequiredRequestProperty,
            [DifferenceKindCodes.RequestPropertyBecameRequired] = DifferenceKindNames.RequestPropertyBecameRequired,
            [DifferenceKindCodes.RequestPropertyTypeChanged] = DifferenceKindNames.RequestPropertyTypeChanged,
            [DifferenceKindCodes.RequestParameterEnumValueRemoved] = DifferenceKindNames.RequestParameterEnumValueRemoved,
            [DifferenceKindCodes.RequestBodyBecameRequired] = DifferenceKindNames.RequestBodyBecameRequired,
            [DifferenceKindCodes.ResponsePropertyBecameOptional] = DifferenceKindNames.ResponsePropertyBecameOptional,
            [DifferenceKindCodes.ResponsePropertyBecameNullable] = DifferenceKindNames.ResponsePropertyBecameNullable,
            [DifferenceKindCodes.ResponseSuccessStatusRemoved] = DifferenceKindNames.ResponseSuccessStatusRemoved,
            [DifferenceKindCodes.ResponseMediaTypeRemoved] = DifferenceKindNames.ResponseMediaTypeRemoved,
            [DifferenceKindCodes.RequiredResponseHeaderRemoved] = DifferenceKindNames.RequiredResponseHeaderRemoved,
            [DifferenceKindCodes.EndpointAdded] = DifferenceKindNames.EndpointAdded,
            [DifferenceKindCodes.EndpointRemoved] = DifferenceKindNames.EndpointRemoved,
            [DifferenceKindCodes.SchemaAdded] = DifferenceKindNames.SchemaAdded,
            [DifferenceKindCodes.SchemaRemoved] = DifferenceKindNames.SchemaRemoved,
            [DifferenceKindCodes.SchemaRenamed] = DifferenceKindNames.SchemaRenamed,
            [DifferenceKindCodes.DescriptionChanged] = DifferenceKindNames.DescriptionChanged
        };

    protected override IReadOnlyDictionary<string, string> DesiredRows => Rows;

    public DifferenceKindDataSeedContributor(
        IRepository<DifferenceKind, Guid> repository,
        IGuidGenerator guidGenerator,
        IDataFilter<IPassivable> passivableFilter)
        : base(repository, guidGenerator, passivableFilter)
    {
    }

    // Ortak lookup alanlariyla aktif fark turu satiri kurar.
    protected override DifferenceKind CreateEntity(Guid id, string code, string name)
    {
        return new DifferenceKind(id, code, name);
    }
}
