using System;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Entities.Lookups;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Ptn.DatabaseChecker.Entities.Connections;

// islevi: Karsilastirmalarda kullanilacak kayitli veritabani baglantilarini (adres defteri) tutar; sifre burada degil Vault'tadir.
// sistemdeki gorevi: ComparisonDefinition ve ComparisonRun kaynak/hedef olarak buraya FK verir; VaultSecretPath sifrenin kasadaki adresini tasir.
public class DatabaseConnection : AuditedAggregateRoot<Guid>, IPassivable, IMultiTenant
{
    // Veritabani motoru lookup'inin kimligi (Guid lookup Id); sistem bu degere gore dogru sema okuyucusunu secer.
    public Guid EngineId { get; set; }

    // EngineId'nin navigation'i; Include ile motor kodu/adi okunur.
    public DatabaseEngine Engine { get; set; } = default!;

    // Insan-okur takma ad ("Canli-PG"); kiraci icinde unique.
    public string Name { get; set; } = default!;

    // Sunucunun adresi (host adi veya IP).
    public string Host { get; set; } = default!;

    // Sunucunun kapi numarasi (5432, 1433...).
    public int Port { get; set; }

    // Sunucudaki hedef veritabaninin adi.
    public string DatabaseName { get; set; } = default!;

    // Sifrenin kendisi degil, Vault'taki secret'in adresi; sifre asla DB'ye yazilmaz.
    public string VaultSecretPath { get; set; } = default!;

    // TLS kullaniminin kararli politika kodu; yeni kayitlar guvenli varsayilan olarak TLS zorunlu baslar.
    public string TlsModeCode { get; set; } = TlsModeCodes.Require;

    // true yalnizca self-signed hedef icin acikca secildiginde sertifika zinciri dogrulamasini atlar.
    public bool TrustServerCertificate { get; set; }

    // Emekli edilen baglanti silinmez pasife cekilir; gecmis Run'lar FK ile isaret etmeye devam eder.
    public bool IsActive { get; set; }

    // Kiraci izolasyonu; ABP sorgulari otomatik filtreler.
    public Guid? TenantId { get; protected set; }

    // EF Core materializasyonu icin parametresiz ctor; disaridan cagrilmaz.
    protected DatabaseConnection()
    {
    }

    // Kimligi disaridan alan ctor; alanlar Mapperly ile dogrulanmis modelden kopyalanir.
    public DatabaseConnection(Guid id)
        : base(id)
    {
    }
}
