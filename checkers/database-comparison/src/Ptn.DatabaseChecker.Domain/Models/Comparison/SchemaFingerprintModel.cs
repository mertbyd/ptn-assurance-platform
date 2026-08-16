using System;
using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Hedefin tek andaki yapisini sema fotografini saklamadan tek snapshot muhru ve dal muhurleriyle temsil eder.
// sistemdeki gorevi: Senaryo malzeme muhrunun (ADR-0020) DB tarafini uretir; koc basina kalici yazilan tek sey hash'lerdir, sema fotografi degil.
public class SchemaFingerprintModel
{
    // Muhur formulunun kararli kodu; formul degisirse tuketici eski muhru "kaydi" saymadan once bunu karsilastirir.
    public string AlgorithmCode { get; set; } = string.Empty;

    // Bilesen kumesi, sirasi veya normalizasyonu degistiginde artan formul surumu.
    public int AlgorithmVersion { get; set; }

    // Tum hedefin tek muhru; ADR-0020'nin db_schema_fingerprint alanina yazilan degerdir.
    public string SnapshotFingerprint { get; set; } = string.Empty;

    // Sema adi ve o semanin muhru; hangi semanin kaydigini tam diff yapmadan gosterir.
    public List<SchemaFingerprintEntryModel> Schemas { get; set; } = new();

    // schema.table adresi ve o tablonun muhru; hangi tablonun kaydigini tam diff yapmadan gosterir.
    public List<SchemaFingerprintEntryModel> Tables { get; set; } = new();

    // Muhrun hesaplandigi an (UTC); yalniz bilgi alanidir, hicbir muhrun icine girmez.
    public DateTime ComputedAt { get; set; }
}
