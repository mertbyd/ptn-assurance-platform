using System;
using System.Collections.Generic;
using System.Linq;
using Ptn.TestModule.Constants.Catalog.Lookups;
using Ptn.TestModule.Models.Catalog;
using Ptn.TestModule.Models.Compilation;

namespace Ptn.TestModule.Managers.Catalog;

// islevi: Senaryo yayininin bes zorunlu makine kapisini sabit sirada degerlendirir.
// sistemdeki gorevi: Bir kapinin uyarida kaybolmasini engeller ve Published gecisine kodlu karar verir.
public class ScenarioPublicationGateManager : TestModuleDomainService
{
    // Sema, turetilebilirlik, assertion, malzeme ve sourceDescriptions kapilarini sirasiyla calistirir.
    public TestScenarioPublishDecision Evaluate(
        ScenarioPublicationCandidate candidate,
        ScenarioCompilationEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(evidence);

        var failedGateCodes = new List<string>();
        AddFailureIf(!evidence.IsSchemaValid, ScenarioGateCodes.SchemaValidity, failedGateCodes);
        AddFailureIf(!evidence.AreAssertionsDerivable, ScenarioGateCodes.Derivability, failedGateCodes);
        AddFailureIf(evidence.AssertionCount <= 0, ScenarioGateCodes.AssertionCount, failedGateCodes);
        AddFailureIf(!IsMaterialSealComplete(candidate), ScenarioGateCodes.MaterialIntegrity, failedGateCodes);
        AddFailureIf(!SourceDescriptionsMatch(candidate, evidence), ScenarioGateCodes.SourceDescriptionConsistency, failedGateCodes);

        return new TestScenarioPublishDecision
        {
            IsPublishable = failedGateCodes.Count == 0,
            FailedGateCodes = failedGateCodes,
            Warnings = evidence.SchemaLintWarnings
                .Select(warning => warning.WarningCode)
                .Distinct(StringComparer.Ordinal)
                .ToList()
        };
    }

    // Kosula gore ilgili kapinin kodunu kararli sirada sonuca ekler.
    private static void AddFailureIf(bool condition, string gateCode, ICollection<string> failedGateCodes)
    {
        if (condition)
        {
            failedGateCodes.Add(gateCode);
        }
    }

    // Aggregate'in dort malzeme kimligi ile fingerprint baglarinin eksiksiz oldugunu dogrular.
    private static bool IsMaterialSealComplete(ScenarioPublicationCandidate candidate)
    {
        return !string.IsNullOrWhiteSpace(candidate.RulesFingerprint) &&
               candidate.SpecSnapshotId.HasValue && candidate.SpecSnapshotId.Value != Guid.Empty &&
               !string.IsNullOrWhiteSpace(candidate.SpecFingerprint) &&
               candidate.DbConnectionId.HasValue && candidate.DbConnectionId.Value != Guid.Empty &&
               !string.IsNullOrWhiteSpace(candidate.DbSchemaFingerprint) &&
               !string.IsNullOrWhiteSpace(candidate.ProfileFingerprint);
    }

    // Derlenmis belgeden cozulmus tum API kaynaklarinin satirdaki snapshot kimligine bagli oldugunu dogrular.
    private static bool SourceDescriptionsMatch(
        ScenarioPublicationCandidate candidate,
        ScenarioCompilationEvidence evidence)
    {
        return candidate.SpecSnapshotId.HasValue &&
               evidence.SourceDescriptionSpecSnapshotIds.Count > 0 &&
               evidence.SourceDescriptionSpecSnapshotIds.All(id => id == candidate.SpecSnapshotId.Value);
    }
}
