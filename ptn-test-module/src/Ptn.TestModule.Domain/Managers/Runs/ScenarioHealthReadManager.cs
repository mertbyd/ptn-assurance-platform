using System;
using Ptn.TestModule.Constants.Catalog;
using Ptn.TestModule.Entities.Runs;
using Volo.Abp.Domain.Entities;

namespace Ptn.TestModule.Managers.Runs;

// islevi: Saglik okuma girdisinin kanoniklestirilmesini ve bulunamadi kararini sahiplenir.
// sistemdeki gorevi: Anahtarsiz view'in EnsureExists karsiligini Manager'da tutar; AppService duz orkestrasyon kalir.
/// <summary>
/// Senaryo saglik okumasinin anahtar normalizasyonunu ve varlik kapisini uygular.
/// </summary>
public class ScenarioHealthReadManager : TestModuleDomainService
{
    // Senaryo anahtarini katalogla ayni kucuk harfli kapali forma indirger.
    /// <summary>Saglik sorgusunun senaryo anahtarini kanonik bicime getirir.</summary>
    public static string NormalizeScenarioKey(string? scenarioKey)
    {
        var normalized = scenarioKey?.Trim().ToLowerInvariant();
        return !string.IsNullOrWhiteSpace(normalized) && normalized.Length <= TestScenarioConsts.MaxScenarioKeyLength
            ? normalized
            : throw new EntityNotFoundException(typeof(ScenarioHealth));
    }

    // Henuz kosmamis veya yenilenmemis anahtari ABP'nin standart bulunamadi davranisina cevirir.
    /// <summary>Saglik satirinin bulundugunu dogrular.</summary>
    public ScenarioHealth EnsureFound(ScenarioHealth? row, string scenarioKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scenarioKey);
        return row ?? throw new EntityNotFoundException(typeof(ScenarioHealth), scenarioKey);
    }
}
