using System;
using System.Collections.Generic;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Entities.Catalog;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Models.Runs;
using Volo.Abp;

namespace Ptn.TestModule.Managers.Runs;

// islevi: Senaryo ile kosum anindaki dort malzeme muhrunu sabit sirada karsilastirir.
// sistemdeki gorevi: Malzeme kaymasini Failed yerine Inconclusive ve Technical terminal kanitina cevirir.
/// <summary>
/// Kosum ani malzeme kaymasini degerlendirip terminal modeline donusturur.
/// </summary>
public class RunMaterialDriftManager : TestModuleDomainService
{
    // Dort guncel fingerprint'i senaryonun yayin anindaki muhurleriyle karsilastirir.
    /// <summary>Kurallar, API, veritabani ve profil muhurlerindeki kaymayi raporlar.</summary>
    public TestRunMaterialDrift Evaluate(
        TestScenario scenario,
        string rulesFingerprint,
        string specFingerprint,
        string dbSchemaFingerprint,
        string profileFingerprint)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        var driftedMaterialCodes = new List<string>();
        AddIfDrifted(scenario.RulesFingerprint, rulesFingerprint, TestRunConsts.RulesMaterialCode, driftedMaterialCodes);
        AddIfDrifted(scenario.SpecFingerprint, specFingerprint, TestRunConsts.ApiSpecificationMaterialCode, driftedMaterialCodes);
        AddIfDrifted(scenario.DbSchemaFingerprint, dbSchemaFingerprint, TestRunConsts.DatabaseSchemaMaterialCode, driftedMaterialCodes);
        AddIfDrifted(scenario.ProfileFingerprint, profileFingerprint, TestRunConsts.ProfileMaterialCode, driftedMaterialCodes);

        return new TestRunMaterialDrift
        {
            HasDrift = driftedMaterialCodes.Count > 0,
            DriftedMaterialCodes = driftedMaterialCodes
        };
    }

    // Drift kanitini sessiz kapsam kaybi veya yanlis alarm yaratmayan terminal hukme cevirir.
    /// <summary>Kaymis malzemeleri adiyla tasiyan Inconclusive ve Technical terminal modeli kurar.</summary>
    public TestRunTerminalModel CreateInconclusiveTerminalModel(TestRunMaterialDrift drift)
    {
        ArgumentNullException.ThrowIfNull(drift);
        if (!drift.HasDrift)
        {
            throw new BusinessException(TestModuleRunErrorCodes.MaterialDriftRequired);
        }

        return new TestRunTerminalModel
        {
            OutcomeCode = TestOutcomeStatusCodes.Inconclusive,
            FailureCategoryCode = TestFailureCategoryCodes.Technical,
            Detail = string.Join(", ", drift.DriftedMaterialCodes)
        };
    }

    // Beklenen ve guncel fingerprint farkliysa malzeme kodunu kararli sirada ekler.
    /// <summary>Tek bir malzeme muhrundeki kaymayi sonuc kodu listesine ekler.</summary>
    private static void AddIfDrifted(
        string? expected,
        string current,
        string materialCode,
        ICollection<string> driftedMaterialCodes)
    {
        if (!string.Equals(Normalize(expected), Normalize(current), StringComparison.Ordinal))
        {
            driftedMaterialCodes.Add(materialCode);
        }
    }

    // Checker veya profil kaynakli sha256: on ekini kaldirip digest'i kucuk harfe cevirir.
    /// <summary>Fingerprint degerini karsilastirma icin ortak lowercase bicime getirir.</summary>
    private static string? Normalize(string? fingerprint)
    {
        if (string.IsNullOrWhiteSpace(fingerprint))
        {
            return null;
        }

        var normalized = fingerprint.Trim();
        if (normalized.StartsWith(PtnBridgeSettingNames.FingerprintPrefix, StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[PtnBridgeSettingNames.FingerprintPrefix.Length..];
        }

        return normalized.ToLowerInvariant();
    }
}
