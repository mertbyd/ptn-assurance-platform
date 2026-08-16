using System;
using System.Collections.Generic;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Entities.Lookups;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Ptn.DatabaseChecker.Data;

// islevi: Rapor formati lookup satirlarini (Html/Markdown) baslangicta idempotent olusturur.
// sistemdeki gorevi: Ortak akis LookupDataSeedContributor'dan gelir; burada yalnizca satirlar ve fabrika tanimli.
public class ReportFormatDataSeedContributor : LookupDataSeedContributor<ReportFormat>
{
    public ReportFormatDataSeedContributor(
        IRepository<ReportFormat, Guid> repository,
        IGuidGenerator guidGenerator)
        : base(repository, guidGenerator)
    {
    }

    protected override IReadOnlyDictionary<string, string> DesiredRows => new Dictionary<string, string>
    {
        [ReportFormatCodes.Html] = "HTML",
        [ReportFormatCodes.Markdown] = "Markdown"
    };

    protected override ReportFormat CreateEntity(Guid id, string code, string name)
        => new(id, code, name);
}
