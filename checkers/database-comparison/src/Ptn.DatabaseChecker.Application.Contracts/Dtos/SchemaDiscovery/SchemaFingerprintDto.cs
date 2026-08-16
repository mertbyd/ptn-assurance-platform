using System;
using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Dtos.SchemaDiscovery;

// islevi: Hedefin tek andaki yapisini sema fotografi dondurmeden tek snapshot muhru ve dal muhurleriyle tasir.
// sistemdeki gorevi: Senaryo malzeme muhrunun DB tarafini (db_schema_fingerprint) yayinlar; kanoniklik sozlesmesi PACKAGE-README'de public API kadar baglayicidir.
public class SchemaFingerprintDto
{
    /// <summary>
    /// Tum hedefin tek muhru; senaryo malzeme muhrundeki db_schema_fingerprint degeridir.
    /// </summary>
    public string SnapshotFingerprint { get; set; } = default!;

    /// <summary>
    /// Muhur formulunun kararli kodu; farkli kod tasiyan iki muhur karsilastirilamaz.
    /// </summary>
    public string AlgorithmCode { get; set; } = default!;

    /// <summary>
    /// Formul surumu; bilesen kumesi, sirasi veya normalizasyonu degistiginde artar.
    /// </summary>
    public int AlgorithmVersion { get; set; }

    /// <summary>
    /// Sema adi ve o semanin muhru.
    /// </summary>
    public List<SchemaFingerprintEntryDto> Schemas { get; set; } = new();

    /// <summary>
    /// schema.table adresi ve o tablonun muhru.
    /// </summary>
    public List<SchemaFingerprintEntryDto> Tables { get; set; } = new();

    /// <summary>
    /// Muhrun hesaplandigi an (UTC); yalniz bilgi alanidir, hicbir muhrun icine girmez.
    /// </summary>
    public DateTime ComputedAt { get; set; }
}
