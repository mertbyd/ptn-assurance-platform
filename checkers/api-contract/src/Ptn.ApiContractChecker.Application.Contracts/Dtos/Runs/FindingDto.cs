namespace Ptn.ApiContractChecker.Dtos.Runs;

// islevi: Tek contract farkinin tur, severity, direction, adres ve degerlerini API cevabinda tasir.
// sistemdeki gorevi: Lookup kodlariyla kendi kendine yeterli bulgu detayini istemciye sunar.
public class FindingDto
{
    public string KindCode { get; set; } = default!;
    public string SeverityCode { get; set; } = default!;
    public string DirectionCode { get; set; } = default!;
    public string? Fingerprint { get; set; }
    public string ChangeStateCode { get; set; } = default!;
    public FindingAddressDto Address { get; set; } = new();
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}
