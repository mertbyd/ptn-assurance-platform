namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Bir baglantidaki DB assertion adreslerini toplu turetilebilirlik kapisina tasir.
// sistemdeki gorevi: Public Bridge dogrulama yolunu checker kontratindan ayirir.
public sealed class DatabaseDerivabilityRequestDto
{
    /// <summary>
    /// Checker isleminin calisacagi baglantinin kimligini belirtir.
    /// </summary>
    public Guid ConnectionId { get; set; }
    /// <summary>
    /// Dogrulama veya assertion girdilerini kararli sirada listeler.
    /// </summary>
    public List<DatabaseDerivabilityAddressDto> Assertions { get; set; } = [];
}
