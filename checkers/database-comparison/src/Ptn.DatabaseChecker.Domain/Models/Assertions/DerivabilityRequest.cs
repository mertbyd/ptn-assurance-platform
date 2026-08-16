namespace Ptn.DatabaseChecker.Models.Assertions;

// islevi: Bir baglanti icin birlikte katalog kapisindan gecirilecek assertion adreslerini tasir.
// sistemdeki gorevi: AppService ile AssertionDerivabilityManager arasindaki motor-bagimsiz toplu girdidir.
public sealed class DerivabilityRequest
{
    public Guid ConnectionId { get; set; }
    public List<DerivabilityAddress> Assertions { get; set; } = [];
}
