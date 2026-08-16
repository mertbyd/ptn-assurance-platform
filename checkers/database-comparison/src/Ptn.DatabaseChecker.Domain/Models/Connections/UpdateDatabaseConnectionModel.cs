using System;
using Ptn.DatabaseChecker.Constants.Comparison;

namespace Ptn.DatabaseChecker.Models.Connections;

// islevi: Baglanti guncellemesinde degistirilebilir alanlari tasir.
// sistemdeki gorevi: DatabaseConnectionManager ad benzersizligi (mevcut haric) ve motor varligi kurallarini bu model uzerinden isletir.
public class UpdateDatabaseConnectionModel
{
    // Motor lookup kimligi (Guid lookup Id); varligi manager dogrular.
    public Guid EngineId { get; set; }

    // Kiraci icinde benzersiz olmasi gereken takma ad.
    public string Name { get; set; } = default!;

    // Sunucu adresi.
    public string Host { get; set; } = default!;

    // Sunucu portu.
    public int Port { get; set; }

    // Hedef veritabani adi.
    public string DatabaseName { get; set; } = default!;

    // Sifrenin Vault'taki adresi.
    public string VaultSecretPath { get; set; } = default!;

    // Hedef baglantinin kararli TLS politika kodu.
    public string TlsModeCode { get; set; } = TlsModeCodes.Require;

    // Sertifika zinciri dogrulamasinin acikca atlanip atlanmayacagi.
    public bool TrustServerCertificate { get; set; }

    // Aktiflik durumu (pasife cekme / geri alma).
    public bool IsActive { get; set; }
}
