using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Runs.Lookups;

// islevi: Dis runner kontrollerine verilebilecek severity degerlerini kapali kodlarla tanimlar.
// sistemdeki gorevi: Akis kontrolu ile kayit sahibi hukmu arasindaki ayrimi kod seviyesinde tutar (ADR-0015 §E).
/// <summary>
/// Redocly Respect severity bayraginin kabul ettigi kararli degerleri tasir.
/// </summary>
public static class RespectSeverityCodes
{
    /// <summary>Kontrol basarisiz olursa kosumu dusuren akis seviyesidir.</summary>
    public const string Error = "error";

    /// <summary>Kontrolu kaydeden ama hukmu bizim checker'imiza birakan seviyedir.</summary>
    public const string Warn = "warn";

    /// <summary>Kontrolu tamamen kapatan seviyedir.</summary>
    public const string Off = "off";

    /// <summary>Severity bayraginda kabul edilen tum degerlerdir.</summary>
    public static IReadOnlyCollection<string> All { get; } = [Error, Warn, Off];
}
