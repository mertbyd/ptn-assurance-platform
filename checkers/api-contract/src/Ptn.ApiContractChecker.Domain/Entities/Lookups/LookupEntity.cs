using System;
using Volo.Abp.Domain.Entities;

namespace Ptn.ApiContractChecker.Entities.Lookups;

// islevi: Tum lookup (referans/master veri) entity'lerinin ortak kolonlarini ve kimligini tek yerde tanimlar.
// sistemdeki gorevi: Her yeni lookup icin Code/Name/Description/IsActive tekrarini engeller; concrete lookup'lar yalnizca kendi ctor'unu verir (golden rule 1: is bir kez yapilir).
public abstract class LookupEntity : Entity<Guid>, IPassivable
{
    // Kararli kod; okuyucu secimi, seed idempotansi ve benzersizlik kontrolu bu alandan yurur, ilgili *Codes sabitleriyle eslesir.
    public string Code { get; internal set; } = default!;

    // Insan-okur ad; UI listelerinde ve rapor basliklarinda gosterilir.
    public string Name { get; internal set; } = default!;

    // Opsiyonel aciklama; satirin kapsami/notu icin, UI detayinda gosterilebilir.
    public string? Description { get; internal set; }

    // Kaydin operasyonel olarak secilebilir olup olmadigi (IPassivable); pasif satir listede gizlenir ama gecmis kayitlar korunur.
    public bool IsActive { get; internal set; }

    // EF Core materializasyonu icin parametresiz ctor; concrete tipler tarafindan zincirlenir.
    protected LookupEntity()
    {
    }

    // Ortak alanlari davranis uygulamadan atar; kanoniklestirme ve yasam dongusu manager'a aittir.
    protected LookupEntity(Guid id, string code, string name, string? description = null, bool isActive = true)
        : base(id)
    {
        Code = code;
        Name = name;
        Description = description;
        IsActive = isActive;
    }
}
