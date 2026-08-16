namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Motor-ozel yetki sorgusunun yazma ve yonetici bulgularini provider-bagimsiz tasir.
// sistemdeki gorevi: EF probe'u ile baglanti tester'i arasinda ham SQL/provider tipi sizdirmayan kucuk sonuc sozlesmesidir.
public sealed class EnginePrivilegeProbeResult
{
    public bool CanWrite { get; init; }
    public bool IsSuperUser { get; init; }
    public string? WarningCode { get; init; }
}
