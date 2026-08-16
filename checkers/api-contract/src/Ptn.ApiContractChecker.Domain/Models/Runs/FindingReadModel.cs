namespace Ptn.ApiContractChecker.Models.Runs;

// islevi: Owned JSON bulgusunu repository projeksiyonundan katman disina duz alanlarla tasir.
// sistemdeki gorevi: EF owned tipini Application'a sizdirmeden filtrelenmis sayfa ve degisim siniflandirmasi kurar.
public class FindingReadModel
{
    public string KindCode { get; set; } = default!;
    public string SeverityCode { get; set; } = default!;
    public string DirectionCode { get; set; } = default!;
    public string? Fingerprint { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string ChangeStateCode { get; set; } = default!;
    public FindingAddressReadModel Address { get; set; } = new();
}
