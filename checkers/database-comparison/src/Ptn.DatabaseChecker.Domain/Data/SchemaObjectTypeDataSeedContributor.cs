using System;
using System.Collections.Generic;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Entities.Lookups;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Ptn.DatabaseChecker.Data;

// islevi: Sema nesne turu lookup satirlarini (Table/View/Column/...) baslangicta idempotent olusturur.
// sistemdeki gorevi: Ortak akis LookupDataSeedContributor'dan gelir; burada yalnizca satirlar ve fabrika tanimli.
public class SchemaObjectTypeDataSeedContributor : LookupDataSeedContributor<SchemaObjectType>
{
    public SchemaObjectTypeDataSeedContributor(
        IRepository<SchemaObjectType, Guid> repository,
        IGuidGenerator guidGenerator)
        : base(repository, guidGenerator)
    {
    }

    protected override IReadOnlyDictionary<string, string> DesiredRows => new Dictionary<string, string>
    {
        [SchemaObjectTypeCodes.Database] = "Veritabani",
        [SchemaObjectTypeCodes.Table] = "Tablo",
        [SchemaObjectTypeCodes.View] = "View",
        [SchemaObjectTypeCodes.Trigger] = "Trigger",
        [SchemaObjectTypeCodes.Procedure] = "Procedure",
        [SchemaObjectTypeCodes.Function] = "Function",
        [SchemaObjectTypeCodes.Column] = "Kolon",
        [SchemaObjectTypeCodes.Index] = "Index",
        [SchemaObjectTypeCodes.PrimaryKey] = "Birincil Anahtar",
        [SchemaObjectTypeCodes.ForeignKey] = "Yabanci Anahtar",
        [SchemaObjectTypeCodes.Unique] = "Unique Kisit",
        [SchemaObjectTypeCodes.Check] = "Check Kisit",
        [SchemaObjectTypeCodes.Sequence] = "Sequence",
        [SchemaObjectTypeCodes.Type] = "Tip/Enum",
        [SchemaObjectTypeCodes.Extension] = "Extension"
    };

    protected override SchemaObjectType CreateEntity(Guid id, string code, string name)
        => new(id, code, name);
}
