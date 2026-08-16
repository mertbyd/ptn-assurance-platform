namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Merkle zincirinin bir dalini adresi ve muhruyle tasir (sema adi veya schema.table adresi).
// sistemdeki gorevi: Kayan dalin tam diff yapilmadan adiyla isaretlenmesini saglar; Schemas ve Tables listeleri ayni sekli paylasir.
public class SchemaFingerprintEntryModel
{
    // Dalin kararli adresi; sema seviyesinde sema adi, tablo seviyesinde schema.table adres grameridir.
    public string Name { get; set; } = string.Empty;

    // Dalin buyuk harfli onaltilik SHA-256 muhru.
    public string Fingerprint { get; set; } = string.Empty;
}
