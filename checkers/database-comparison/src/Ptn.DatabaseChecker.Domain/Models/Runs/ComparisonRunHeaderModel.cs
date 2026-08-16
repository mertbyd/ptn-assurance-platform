using System;

namespace Ptn.DatabaseChecker.Models.Runs;

// islevi: Run listesi/get-header okumasi icin hafif projeksiyon modeli; owned agir kolonlari (ScopeSnapshot/Findings/Reports) TASIMAZ.
// sistemdeki gorevi: Repository, run satirini yalnizca header kolonlari + companion adlarla projekte eder; boylece liste sorgusu jsonb payload'lari cekmez (perf). Detay icin tam entity ayrica okunur.
public class ComparisonRunHeaderModel
{
    // Run kimligi.
    public Guid Id { get; set; }

    // Run'in dogdugu opsiyonel tarif kimligi; null = ad-hoc.
    public Guid? ComparisonDefinitionId { get; set; }

    // Tarifin insan-okur adi (companion); ad-hoc run'da null.
    public string? ComparisonDefinitionName { get; set; }

    // Snapshot referans baglanti kimligi ve adi.
    public Guid SourceConnectionId { get; set; }
    public string SourceConnectionName { get; set; } = default!;

    // Snapshot hedef baglanti kimligi ve adi.
    public Guid TargetConnectionId { get; set; }
    public string TargetConnectionName { get; set; } = default!;

    // Snapshot karsilastirma modu lookup kimligi + kararli kod + insan-okur ad.
    public Guid ComparisonTypeId { get; set; }
    public string ComparisonTypeCode { get; set; } = default!;
    public string ComparisonTypeName { get; set; } = default!;

    // Durum lookup kimligi + kararli kod + insan-okur ad.
    public Guid StatusId { get; set; }
    public string StatusCode { get; set; } = default!;
    public string StatusName { get; set; } = default!;

    // Yasam dongusu zamanlari; Pending'de null olabilir.
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Basarisiz run icin hata ozeti.
    public string? ErrorMessage { get; set; }

    // Denormalize ozet sayaclar.
    public int SchemaDifferenceCount { get; set; }
    public int DataDifferenceCount { get; set; }
    public int MigrationDifferenceCount { get; set; }

    // Run kaydinin olusturulma zamani.
    public DateTime CreationTime { get; set; }
}
