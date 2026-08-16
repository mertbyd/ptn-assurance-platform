using System;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Entities.Lookups;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Ptn.DatabaseChecker.Entities.Definitions;

// islevi: "SUNU sununla, su modda karsilastir" cumlesinin kaydi; tekrar kullanilabilir is sablonu (tarif).
// sistemdeki gorevi: Run'lar bu tariften dogar; tarif degisebilir ama gecmis Run'lar kendi snapshot'ini tasidigi icin tarih bozulmaz.
public class ComparisonDefinition : AuditedAggregateRoot<Guid>, IPassivable, IMultiTenant
{
    // Isin insan diliyle adi ("Test->Canli Gunluk Kontrol"); kiraci icinde unique.
    public string Name { get; set; } = default!;

    // Karsilastirmanin source tarafindaki baglanti; is rolu SourceRoleCode ile belirlenir.
    public Guid SourceConnectionId { get; set; }

    // SourceConnectionId'nin navigation'i; Include ile baglanti adi okunur.
    public DatabaseConnection SourceConnection { get; set; } = default!;

    // Karsilastirmanin target tarafindaki baglanti; source tarafinin karsi rolunu alir.
    public Guid TargetConnectionId { get; set; }

    // TargetConnectionId'nin navigation'i; Include ile baglanti adi okunur.
    public DatabaseConnection TargetConnection { get; set; } = default!;

    /// <summary>
    /// SourceConnection tarafinin Reference veya Audited rolu; hedef taraf karsi rolu alir.
    /// </summary>
    public string SourceRoleCode { get; set; } = ComparisonSideRoleCodes.Reference;

    // Karsilastirma modu lookup'inin kimligi (Guid lookup Id): SchemaOnly / DataOnly / Both.
    public Guid ComparisonTypeId { get; set; }

    // ComparisonTypeId'nin navigation'i; Include ile mod kodu/adi okunur.
    public ComparisonType ComparisonType { get; set; } = default!;

    // Tarifin baglamini anlatan serbest metin ("Her sabah deploy sonrasi kosulur").
    public string? Description { get; set; }

    // Emekli edilen tarif silinmez pasife cekilir; gecmis Run'lar FK ile isaret etmeye devam eder.
    public bool IsActive { get; set; }

    // Kiraci izolasyonu; ABP sorgulari otomatik filtreler.
    public Guid? TenantId { get; protected set; }

    // EF Core materializasyonu icin parametresiz ctor; disaridan cagrilmaz.
    protected ComparisonDefinition()
    {
    }

    // Kimligi disaridan alan ctor; alanlar Mapperly ile dogrulanmis modelden kopyalanir.
    public ComparisonDefinition(Guid id)
        : base(id)
    {
    }
}
