using System;
using System.Collections.Generic;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Entities.Definitions;
using Ptn.DatabaseChecker.Entities.Lookups;
using Ptn.DatabaseChecker.Models.Comparison.Findings;
using Ptn.DatabaseChecker.Models.Comparison.Reports;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Ptn.DatabaseChecker.Entities.Runs;

// islevi: Dugmeye her basildiginda dogan infaz kaydi; snapshot + durum + ozet sayaclar. Yazildiktan sonra tarih kaydidir, elle duzeltilmez.
// sistemdeki gorevi: Kayit dunyasinin kalbi; tum bulgu tablolari ve rapor ComparisonRunId FK'siyle bu kayda baglanir.
public class ComparisonRun : AuditedAggregateRoot<Guid>, IMultiTenant
{
    // Hangi tariften dogdu; null = ad-hoc calistirma (tarif kaydetmeden tek seferlik kosturma).
    public Guid? ComparisonDefinitionId { get; set; }

    // ComparisonDefinitionId'nin navigation'i; Include ile tarif adi okunur.
    public ComparisonDefinition? Definition { get; set; }

    // O an fiilen kullanilan referans baglanti (snapshot); tarif sonradan degisse de tarih dogru kalir.
    public Guid SourceConnectionId { get; set; }

    // SourceConnectionId'nin navigation'i; Include ile baglanti adi okunur.
    public DatabaseConnection SourceConnection { get; set; } = default!;

    // O an fiilen kullanilan denetlenen baglanti (snapshot).
    public Guid TargetConnectionId { get; set; }

    // TargetConnectionId'nin navigation'i; Include ile baglanti adi okunur.
    public DatabaseConnection TargetConnection { get; set; } = default!;

    // O an kullanilan karsilastirma modu lookup'inin kimligi (Guid lookup Id, snapshot).
    public Guid ComparisonTypeId { get; set; }

    // ComparisonTypeId'nin navigation'i; Include ile mod kodu/adi okunur.
    public ComparisonType ComparisonType { get; set; } = default!;

    // Yasam dongusu durumu lookup'inin kimligi (Guid lookup Id): Pending / Running / Completed / Failed.
    public Guid StatusId { get; set; }

    // StatusId'nin navigation'i; Include ile durum kodu/adi okunur.
    public ComparisonRunStatus Status { get; set; } = default!;

    // Islemenin fiilen basladigi an; CreationTime ile farki kuyrukta bekleme suresidir. Pending'de null.
    public DateTime? StartedAt { get; set; }

    // Isin bittigi an; CompletedAt - StartedAt = islem suresi (ayrica Duration kolonu tutulmaz, turetilebilir).
    public DateTime? CompletedAt { get; set; }

    // Failed ise kullanicinin gorecegi sebep; basarili run'da null.
    public string? ErrorMessage { get; set; }

    // Sema farki ozet sayaci; liste ekrani bulgu tablolarini COUNT etmesin diye run biterken bir kez yazilir (bilincli denormalizasyon).
    public int SchemaDifferenceCount { get; set; }

    // Veri farki ozet sayaci (DataDifference satir sayisi).
    public int DataDifferenceCount { get; set; }

    // Migration farki ozet sayaci.
    public int MigrationDifferenceCount { get; set; }

    // Run'in tum bulgulari (owned jsonb; eski Schema/Migration/Data* bulgu tablolarinin yerini alir).
    // Rapor bu modelden uretilir, ozet sayaclar (Schema/Data/MigrationDifferenceCount) bunun listelerinden hesaplanir.
    public ComparisonFindings Findings { get; set; } = new();

    // Run icin uretilmis rapor icerikleri (owned jsonb; eski ComparisonReport tablosunun yerini alir): format basina bir eleman (Html/Markdown).
    // DIKKAT: agir alan - Run liste sorgulari bu owned kolonu projekte etmemeli (yalnizca "raporu ac" akisinda okunur).
    public List<ComparisonReportContent> Reports { get; set; } = new();

    // Kiraci izolasyonu; ABP sorgulari otomatik filtreler.
    public Guid? TenantId { get; protected set; }

    // EF Core materializasyonu icin parametresiz ctor; disaridan cagrilmaz.
    protected ComparisonRun()
    {
    }

    // Kimligi disaridan alan ctor; alanlar Mapperly ile dogrulanmis modelden kopyalanir.
    public ComparisonRun(Guid id)
        : base(id)
    {
    }
}
