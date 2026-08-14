using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Runs.Lookups;

// islevi: Bir kosunun nasil baslatildigini kararli kodlarla tanimlar.
// sistemdeki gorevi: test_trigger_kinds lookup'inin kapali sozlugudur; "kim baslatti" CreatorId'dir, "nasil" ayri lookup'tir (ADR-0016 §E, §F).
public static class TestTriggerKindCodes
{
    // Kullanici arayuzunden elle baslatildi.
    public const string Manual = "Manual";

    // Zamanlanmis is tarafindan baslatildi; oturum acmis kullanici yoktur.
    public const string Scheduled = "Scheduled";

    // Dis API cagrisiyla baslatildi.
    public const string Api = "Api";

    // Gelen webhook ile baslatildi.
    public const string Webhook = "Webhook";

    // Sozlesme degisti; etkilenen senaryolar kosuyor.
    public const string ContractChange = "ContractChange";

    public static IReadOnlyCollection<string> All { get; } =
        [Manual, Scheduled, Api, Webhook, ContractChange];
}
