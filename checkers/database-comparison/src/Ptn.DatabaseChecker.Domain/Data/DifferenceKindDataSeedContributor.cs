using System;
using System.Collections.Generic;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Entities.Lookups;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Ptn.DatabaseChecker.Data;

// islevi: Fark yonu lookup satirlarini (OnlyInSource/OnlyInTarget/Modified) baslangicta idempotent olusturur.
// sistemdeki gorevi: Ortak akis LookupDataSeedContributor'dan gelir; burada yalnizca satirlar ve fabrika tanimli.
public class DifferenceKindDataSeedContributor : LookupDataSeedContributor<DifferenceKind>
{
    public DifferenceKindDataSeedContributor(
        IRepository<DifferenceKind, Guid> repository,
        IGuidGenerator guidGenerator)
        : base(repository, guidGenerator)
    {
    }

    protected override IReadOnlyDictionary<string, string> DesiredRows => new Dictionary<string, string>
    {
        [DifferenceKindCodes.OnlyInSource] = "Sadece Kaynakta",
        [DifferenceKindCodes.OnlyInTarget] = "Sadece Hedefte",
        [DifferenceKindCodes.Modified] = "Degismis"
    };

    protected override DifferenceKind CreateEntity(Guid id, string code, string name)
        => new(id, code, name);
}
