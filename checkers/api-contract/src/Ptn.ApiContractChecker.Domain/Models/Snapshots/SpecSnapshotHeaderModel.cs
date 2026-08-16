namespace Ptn.ApiContractChecker.Models.Snapshots;

// islevi: Bir dokumanin snapshot gecmisindeki tek satiri ham spec govdesi olmadan tasir.
// sistemdeki gorevi: Karsilastirma ekraninin iki snapshot'i secebilmesi icin gereken en kucuk kimlik ve olcu kumesidir.
public class SpecSnapshotHeaderModel
{
    public Guid Id { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime LastSeenAt { get; set; }
    public string FormatCode { get; set; } = default!;
    public string? ApiVersion { get; set; }
    public int ByteSize { get; set; }
    public string ShortCanonicalHash { get; set; } = default!;
}
