using System;
using System.Collections.Generic;
using Ptn.DatabaseChecker.Models.Comparison;

namespace Ptn.DatabaseChecker.Models.Diagnosis;

// islevi: Tek hipotezin ihtiyac duydugu salt-okuma probe turu, katalog hedefi, anahtari ve ayar beklentisini tasir.
// sistemdeki gorevi: Kurallar ile probe implementasyonlarini serbest SQL/WHERE veya provider context'i paylastirmadan baglar.
public sealed class ProbeRequest
{
    public string ProbeKindCode { get; set; } = string.Empty;
    public string HypothesisKindCode { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public Dictionary<string, string?> KeyValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public TableDataStructureModel? Structure { get; set; }
    public string? SettingName { get; set; }
    public string? ExpectedSettingValue { get; set; }
}
