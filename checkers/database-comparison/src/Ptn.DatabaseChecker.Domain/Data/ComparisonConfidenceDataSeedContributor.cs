using System;
using System.Collections.Generic;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Entities.Lookups;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Ptn.DatabaseChecker.Data;

// islevi: Fark guveni lookup satirlarini (Exact/Canonical/Approximate/Incomparable) baslangicta idempotent olusturur.
// sistemdeki gorevi: Ortak akis LookupDataSeedContributor'dan gelir; burada yalnizca satirlar ve fabrika tanimli.
public class ComparisonConfidenceDataSeedContributor : LookupDataSeedContributor<ComparisonConfidence>
{
    public ComparisonConfidenceDataSeedContributor(
        IRepository<ComparisonConfidence, Guid> repository,
        IGuidGenerator guidGenerator)
        : base(repository, guidGenerator)
    {
    }

    protected override IReadOnlyDictionary<string, string> DesiredRows => new Dictionary<string, string>
    {
        [ComparisonConfidenceCodes.Exact] = "Kesin",
        [ComparisonConfidenceCodes.Canonical] = "Kanonik",
        [ComparisonConfidenceCodes.Approximate] = "Yaklasik",
        [ComparisonConfidenceCodes.Incomparable] = "Kiyaslanamaz"
    };

    protected override ComparisonConfidence CreateEntity(Guid id, string code, string name)
        => new(id, code, name);
}
