namespace Ptn.DatabaseChecker.Dtos.SchemaDiscovery;

// islevi: Merkle zincirinin bir dalini adresi ve muhruyle API cevabina tasir.
// sistemdeki gorevi: Tuketici hangi semanin veya tablonun kaydigini tam karsilastirma calistirmadan bu listeden okur.
public class SchemaFingerprintEntryDto
{
    /// <summary>
    /// Dalin kararli adresi; sema seviyesinde sema adi, tablo seviyesinde schema.table adresidir.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Dalin buyuk harfli onaltilik SHA-256 muhru.
    /// </summary>
    public string Fingerprint { get; set; } = default!;
}
