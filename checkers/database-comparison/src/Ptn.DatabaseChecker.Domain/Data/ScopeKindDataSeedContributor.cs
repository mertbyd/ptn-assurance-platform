using System;
using System.Collections.Generic;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Entities.Lookups;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Ptn.DatabaseChecker.Data;

// islevi: Kapsam kurali lookup satirlarini (Include/Exclude/Ignore/DataCompare) baslangicta idempotent olusturur.
// sistemdeki gorevi: Ortak akis LookupDataSeedContributor'dan gelir; burada yalnizca satirlar ve fabrika tanimli.
public class ScopeKindDataSeedContributor : LookupDataSeedContributor<ScopeKind>
{
    public ScopeKindDataSeedContributor(
        IRepository<ScopeKind, Guid> repository,
        IGuidGenerator guidGenerator)
        : base(repository, guidGenerator)
    {
    }

    protected override IReadOnlyDictionary<string, string> DesiredRows => new Dictionary<string, string>
    {
        [ScopeKindCodes.Include] = "Dahil Et",
        [ScopeKindCodes.Exclude] = "Haric Tut",
        [ScopeKindCodes.Ignore] = "Yok Say",
        [ScopeKindCodes.DataCompare] = "Veri Kiyasla"
    };

    protected override ScopeKind CreateEntity(Guid id, string code, string name)
        => new(id, code, name);
}
