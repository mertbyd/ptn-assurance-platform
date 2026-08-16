using System;
using Volo.Abp.Domain.Entities;

namespace Ptn.DatabaseChecker.Entities.Lookups;

// islevi: Tum lookup (referans/master veri) entity'lerinin ortak kolonlarini ve kimligini tek yerde tanimlar.
// sistemdeki gorevi: Her yeni lookup icin Code/Name/Description/IsActive tekrarini engeller; concrete lookup'lar yalnizca kendi ctor'unu verir (golden rule 1: is bir kez yapilir).
public abstract class LookupEntity : Entity<Guid>, IPassivable
{
    // Kararli kod; okuyucu secimi, seed idempotansi ve benzersizlik kontrolu bu alandan yurur, ilgili *Codes sabitleriyle eslesir.
    public string Code { get; set; } = default!;

    // Insan-okur ad; UI listelerinde ve rapor basliklarinda gosterilir.
    public string Name { get; set; } = default!;

    // Opsiyonel aciklama; satirin kapsami/notu icin, UI detayinda gosterilebilir.
    public string? Description { get; set; }

    // Kaydin operasyonel olarak secilebilir olup olmadigi (IPassivable); pasif satir listede gizlenir ama gecmis kayitlar korunur.
    public bool IsActive { get; set; }

    // EF Core materializasyonu icin parametresiz ctor; concrete tipler tarafindan zincirlenir.
    protected LookupEntity()
    {
    }

    // Ortak alanlari tek noktada garanti altina alan ctor; concrete lookup ctor'lari bunu cagirir.
    protected LookupEntity(Guid id, string code, string name, string? description = null, bool isActive = true)
        : base(id)
    {
        Code = code;
        Name = name;
        Description = description;
        IsActive = isActive;
    }
}
