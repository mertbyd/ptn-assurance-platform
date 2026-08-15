using System;
using Ptn.TestModule.Models.Runs;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Ptn.TestModule.Entities.Runs;

// islevi: Bir terminal farkin tam konum, kaynak ve deger alanlarini tasir.
// sistemdeki gorevi: TestRunResult aggregate'iyle birlikte yazilan tenant-aware cocuk veri kabugudur.
/// <summary>
/// Bir test kosum sonucu icindeki konumlanmis kalici bulgudur.
/// </summary>
public class TestResultFinding : CreationAuditedEntity<Guid>, IMultiTenant
{
    /// <summary>Bulguyu sahiplenen TestRunResult aggregate kimligidir.</summary>
    public Guid TestRunResultId { get; internal set; }

    /// <summary>Rapor icindeki kararli bir tabanli sira numarasidir.</summary>
    public int Ordinal { get; internal set; }

    /// <summary>Bulgunun kaynak, tur, kural ve konumundan turetilmis kararli parmak izidir.</summary>
    public string Fingerprint { get; internal set; } = string.Empty;

    /// <summary>Bulguyu ureten checker veya runner kodudur.</summary>
    public string SourceCheckerCode { get; internal set; } = string.Empty;

    /// <summary>Kaynak checker'in acik uclu karsilastirma turu kodudur.</summary>
    public string ComparisonKindCode { get; internal set; } = string.Empty;

    /// <summary>Dogrulanan is kurali referansidir.</summary>
    public string? RuleRef { get; internal set; }

    /// <summary>Farkin makine-okur tam konumudur.</summary>
    public string Location { get; internal set; } = string.Empty;

    /// <summary>Kullaniciya gosterilecek hedef adidir.</summary>
    public string? TargetDisplayName { get; internal set; }

    /// <summary>Bulgunun kisa SARIF uyumlu mesajidir.</summary>
    public string Message { get; internal set; } = string.Empty;

    /// <summary>Beklenen degerin guvenli metin temsilidir.</summary>
    public string? ExpectedValue { get; internal set; }

    /// <summary>Gozlenen degerin guvenli metin temsilidir.</summary>
    public string? ObservedValue { get; internal set; }

    /// <summary>Buyuk kanit govdesi yerine satir icinde tutulan ozettir.</summary>
    public string? EvidenceSummary { get; internal set; }

    /// <summary>Asenkron gozlemin kosum baslangicina gore milisaniye konumudur.</summary>
    public int? ObservedAtMs { get; internal set; }

    /// <summary>Polling sirasinda yapilan deneme sayisidir.</summary>
    public int? AttemptCount { get; internal set; }

    /// <summary>ABP veri filtresinin dogrudan cocuk sorgularinda kullandigi tenant kimligidir.</summary>
    public Guid? TenantId { get; internal set; }

    /// <summary>EF Core materializasyonu icin ayrilmis kurucudur.</summary>
    protected TestResultFinding()
    {
    }

    // Manager'in siraladigi bulgu alanlarini davranis calistirmadan atar.
    /// <summary>Dogrulanmis bulgu modelini aggregate cocuk veri kabuguna atar.</summary>
    public TestResultFinding(
        Guid id,
        Guid testRunResultId,
        int ordinal,
        Guid? tenantId,
        string fingerprint,
        TestResultFindingModel model)
        : base(id)
    {
        TestRunResultId = testRunResultId;
        Ordinal = ordinal;
        Fingerprint = fingerprint;
        SourceCheckerCode = model.SourceCheckerCode;
        ComparisonKindCode = model.ComparisonKindCode;
        RuleRef = model.RuleRef;
        Location = model.Location;
        TargetDisplayName = model.TargetDisplayName;
        Message = model.Message;
        ExpectedValue = model.ExpectedValue;
        ObservedValue = model.ObservedValue;
        EvidenceSummary = model.EvidenceSummary;
        ObservedAtMs = model.ObservedAtMs;
        AttemptCount = model.AttemptCount;
        TenantId = tenantId;
    }
}
