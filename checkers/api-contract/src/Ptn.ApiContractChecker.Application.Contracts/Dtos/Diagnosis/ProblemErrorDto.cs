namespace Ptn.ApiContractChecker.Dtos.Diagnosis;

// islevi: Yapilandirilmis RFC 9457 veya ABP validation error adresi ve kodunu tasir.
// sistemdeki gorevi: Ham response govdesini API kontratina almadan problem extension olgusunu domaine verir.
public sealed class ProblemErrorDto
{
    public string? Pointer { get; set; }
    public string? Code { get; set; }
}
