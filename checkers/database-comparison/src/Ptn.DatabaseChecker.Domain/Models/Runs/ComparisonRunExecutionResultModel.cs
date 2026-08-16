using System;
using System.Collections.Generic;
using Ptn.DatabaseChecker.Models.Comparison.Findings;

namespace Ptn.DatabaseChecker.Models.Runs;

// islevi: Bir tarifin background execution'ından cikan hesaplanmis sonucu tasir (snapshot FK + zaman + sayaclar + bulgular).
// sistemdeki gorevi: ComparisonRunExecutionManager bunu uretir; background job Mapperly ile sonucu mevcut Pending/Running entity'ye uygular ve terminal durumu kalici hale getirir.
public class ComparisonRunExecutionResultModel
{
    // Hangi tariften dogdu (calistir-ve-sakla her zaman bir tariften dogar).
    public Guid? ComparisonDefinitionId { get; set; }

    // O an fiilen kullanilan referans baglanti (snapshot).
    public Guid SourceConnectionId { get; set; }

    // O an fiilen kullanilan denetlenen baglanti (snapshot).
    public Guid TargetConnectionId { get; set; }

    // O an kullanilan karsilastirma modu lookup kimligi (snapshot).
    public Guid ComparisonTypeId { get; set; }

    // Motorun cozdugu yasam-dongusu durum lookup kimligi (basarili calistirmada Completed).
    public Guid StatusId { get; set; }

    // Motorun fiilen basladigi an.
    public DateTime? StartedAt { get; set; }

    // Isin bittigi an; CompletedAt - StartedAt = islem suresi.
    public DateTime? CompletedAt { get; set; }

    // Sema farki ozet sayaci (Findings.SchemaDifferences uzunlugu); liste ekrani bulgu okumasin diye denormalize.
    public int SchemaDifferenceCount { get; set; }

    // Veri farki ozet sayaci (Findings.DataDifferences uzunlugu).
    public int DataDifferenceCount { get; set; }

    // Migration farki ozet sayaci (Findings.MigrationDifferences uzunlugu).
    public int MigrationDifferenceCount { get; set; }

    // Run'in tum bulgulari (sema/migration/veri); entity'ye owned jsonb olarak yazilir, rapor bundan uretilir.
    public ComparisonFindings Findings { get; set; } = new();
}
