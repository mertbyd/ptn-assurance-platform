namespace Ptn.ApiContractChecker.Models.Diagnosis;

// islevi: RFC 9457 veya ABP validation error uzantisinin kararli kod ve adresini tasir.
// sistemdeki gorevi: Yapilandirilmamis govdeyi domaine almadan hata konumunu kimlik cikarimina verir.
public sealed class ProblemErrorSignal
{
    public string? Pointer { get; set; }
    public string? Code { get; set; }
}
