using System;
using Ptn.TestModule.Constants.Catalog;
using Ptn.TestModule.Entities.Catalog;
using Ptn.TestModule.ExceptionCodes.Runs;
using Volo.Abp;

namespace Ptn.TestModule.Managers.Runs;

// islevi: Karantinaya alma, sureyi dogrulama ve suresi dolani otomatik cikarma kurallarini isletir.
// sistemdeki gorevi: Suresiz karantinayi imkansiz kilar; suresiz karantina gercek hatayi gizler (PLAN-0003 TM-28 §2.5).
/// <summary>
/// Senaryo karantinasinin sinirli sure politikasini uygular.
/// </summary>
public class ScenarioQuarantineManager : TestModuleDomainService
{
    // Senaryoyu yalniz gelecege donuk ve azami sureyi asmayan bir bitisle karantinaya alir.
    /// <summary>Senaryoyu son kullanma tarihi zorunlu olacak sekilde karantinaya alir.</summary>
    public void Quarantine(TestScenario scenario, DateTime? until, string? reason, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        EnsureExpiryIsPresent(until);
        EnsureWindowIsValid(until!.Value, now);
        scenario.QuarantineUntil = until;
        scenario.QuarantineReason = reason;
    }

    // Suresi dolan senaryoyu karantinadan otomatik cikarir ve degisip degismedigini bildirir.
    /// <summary>Suresi dolmus karantinayi temizler; temizlendiyse true doner.</summary>
    public bool ReleaseExpired(TestScenario scenario, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        if (scenario.QuarantineUntil is null || IsQuarantined(scenario, now))
        {
            return false;
        }

        scenario.QuarantineUntil = null;
        scenario.QuarantineReason = null;
        return true;
    }

    // Senaryonun verilen anda hala karantinada olup olmadigini bildirir.
    /// <summary>Senaryonun verilen anda karantinada olup olmadigini bildirir.</summary>
    public static bool IsQuarantined(TestScenario scenario, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        return scenario.QuarantineUntil is not null && scenario.QuarantineUntil.Value > now;
    }

    // Son kullanma tarihi verilmeyen karantina istegini kararli kodla reddeder.
    /// <summary>Karantina bitisinin verildigini dogrular.</summary>
    private static void EnsureExpiryIsPresent(DateTime? until)
    {
        if (until is null)
        {
            throw new BusinessException(TestModuleRunErrorCodes.QuarantineRequiresExpiry);
        }
    }

    // Gecmise donuk veya azami pencereyi asan karantinayi reddeder.
    /// <summary>Karantina penceresinin gelecege donuk ve sinir icinde oldugunu dogrular.</summary>
    private static void EnsureWindowIsValid(DateTime until, DateTime now)
    {
        if (until <= now || until > now.AddDays(TestScenarioConsts.MaxQuarantineDays))
        {
            throw new BusinessException(TestModuleRunErrorCodes.QuarantineWindowInvalid)
                .WithData(nameof(until), until);
        }
    }
}
