using System;
using System.Collections.Generic;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Entities.Lookups;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Ptn.DatabaseChecker.Data;

// islevi: Karsilastirma modu lookup satirlarini (SchemaOnly/DataOnly/Both) baslangicta idempotent olusturur.
// sistemdeki gorevi: Ortak akis LookupDataSeedContributor'dan gelir; burada yalnizca satirlar ve fabrika tanimli.
public class ComparisonTypeDataSeedContributor : LookupDataSeedContributor<ComparisonType>
{
    public ComparisonTypeDataSeedContributor(
        IRepository<ComparisonType, Guid> repository,
        IGuidGenerator guidGenerator)
        : base(repository, guidGenerator)
    {
    }

    // Tanimli modlar: kod (kararli kimlik) -> insan-okur ad.
    protected override IReadOnlyDictionary<string, string> DesiredRows => new Dictionary<string, string>
    {
        [ComparisonTypeCodes.SchemaOnly] = "Sadece Sema",
        [ComparisonTypeCodes.DataOnly] = "Sadece Veri",
        [ComparisonTypeCodes.Both] = "Sema + Veri"
    };

    // Kod + ad ile ComparisonType satiri kurar; ortak alanlar LookupEntity ctor'una devredilir.
    protected override ComparisonType CreateEntity(Guid id, string code, string name)
        => new(id, code, name);
}
