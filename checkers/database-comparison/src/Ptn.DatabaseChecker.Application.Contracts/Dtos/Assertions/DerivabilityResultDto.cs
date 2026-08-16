namespace Ptn.DatabaseChecker.Dtos.Assertions;

// islevi: Girdi sirasini koruyan DB assertion derivability item listesini HTTP cevabinda tasir.
// sistemdeki gorevi: Her assertion icin kismi gecis olmadan tek outcome yayimlar.
public sealed class DerivabilityResultDto
{
    public List<DerivabilityItemDto> Assertions { get; set; } = [];
}
