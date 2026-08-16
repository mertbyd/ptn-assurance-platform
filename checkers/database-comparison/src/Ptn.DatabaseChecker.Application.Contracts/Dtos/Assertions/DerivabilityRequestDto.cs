namespace Ptn.DatabaseChecker.Dtos.Assertions;

// islevi: Bir baglanti icin toplu DB assertion turetilebilirlik girdisini tasir.
// sistemdeki gorevi: Her assertion adresini yayim oncesi ayni canli katalog kapisindan gecirir.
public sealed class DerivabilityRequestDto
{
    public Guid ConnectionId { get; set; }
    public List<DerivabilityAddressDto> Assertions { get; set; } = [];
}
