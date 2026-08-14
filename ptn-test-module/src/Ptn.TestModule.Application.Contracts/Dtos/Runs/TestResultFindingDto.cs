using System;
using Volo.Abp.Application.Dtos;

namespace Ptn.TestModule.Dtos.Runs;

// islevi: Kalici terminal bulgusunun public okunabilir gorunumunu tasir.
// sistemdeki gorevi: Entity ve tenant filtre alanlarini bulgu raporu sozlesmesine cevirir.
/// <summary>Bir terminal sonuc bulgusunun public gorunumudur.</summary>
public sealed class TestResultFindingDto : CreationAuditedEntityDto<Guid>
{
    /// <summary>Bulguyu sahiplenen sonuc aggregate kimligidir.</summary>
    public Guid TestRunResultId { get; set; }

    /// <summary>Rapor icindeki kararli bir tabanli sira numarasidir.</summary>
    public int Ordinal { get; set; }

    /// <summary>Bulguyu ureten checker veya runner kodudur.</summary>
    public string SourceCheckerCode { get; set; } = string.Empty;

    /// <summary>Kaynak checker'in acik uclu karsilastirma turu kodudur.</summary>
    public string ComparisonKindCode { get; set; } = string.Empty;

    /// <summary>Dogrulanan is kurali referansidir.</summary>
    public string? RuleRef { get; set; }

    /// <summary>Farkin makine-okur tam konumudur.</summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>Kullaniciya gosterilecek hedef adidir.</summary>
    public string? TargetDisplayName { get; set; }

    /// <summary>Bulgunun kisa SARIF uyumlu mesajidir.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Beklenen degerin guvenli metin temsilidir.</summary>
    public string? ExpectedValue { get; set; }

    /// <summary>Gozlenen degerin guvenli metin temsilidir.</summary>
    public string? ObservedValue { get; set; }

    /// <summary>Buyuk kanit govdesi yerine saklanan kisa ozettir.</summary>
    public string? EvidenceSummary { get; set; }

    /// <summary>Asenkron gozlemin kosum baslangicina gore milisaniye konumudur.</summary>
    public int? ObservedAtMs { get; set; }

    /// <summary>Polling sirasinda yapilan deneme sayisidir.</summary>
    public int? AttemptCount { get; set; }

    /// <summary>Kaydi sahiplenen tenant kimligidir.</summary>
    public Guid? TenantId { get; set; }
}
