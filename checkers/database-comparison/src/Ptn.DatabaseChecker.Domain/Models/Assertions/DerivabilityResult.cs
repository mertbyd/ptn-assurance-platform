namespace Ptn.DatabaseChecker.Models.Assertions;

// islevi: Tum assertion adreslerinin girdi sirasini koruyan turetilebilirlik sonuclarini tasir.
// sistemdeki gorevi: Her ogenin bagimsiz tek outcome tasidigi DB yayim kapisi cevabidir.
public sealed class DerivabilityResult
{
    public List<DerivabilityItem> Assertions { get; set; } = [];
}
