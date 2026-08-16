using System;
using System.Collections.Generic;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Entities.Lookups;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Ptn.DatabaseChecker.Data;

// islevi: Calistirma durumu lookup satirlarini (Pending/Running/Completed/Failed) baslangicta idempotent olusturur.
// sistemdeki gorevi: Ortak akis LookupDataSeedContributor'dan gelir; burada yalnizca satirlar ve fabrika tanimli.
public class ComparisonRunStatusDataSeedContributor : LookupDataSeedContributor<ComparisonRunStatus>
{
    public ComparisonRunStatusDataSeedContributor(
        IRepository<ComparisonRunStatus, Guid> repository,
        IGuidGenerator guidGenerator)
        : base(repository, guidGenerator)
    {
    }

    protected override IReadOnlyDictionary<string, string> DesiredRows => new Dictionary<string, string>
    {
        [ComparisonRunStatusCodes.Pending] = "Kuyrukta",
        [ComparisonRunStatusCodes.Running] = "Calisiyor",
        [ComparisonRunStatusCodes.Completed] = "Tamamlandi",
        [ComparisonRunStatusCodes.Failed] = "Basarisiz"
    };

    protected override ComparisonRunStatus CreateEntity(Guid id, string code, string name)
        => new(id, code, name);
}
