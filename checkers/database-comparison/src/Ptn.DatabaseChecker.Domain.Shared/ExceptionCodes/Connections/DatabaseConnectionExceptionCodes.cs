namespace Ptn.DatabaseChecker.ExceptionCodes;

// islevi: DatabaseConnection modulunun hata ve validasyon kodlarini tanimlar.
// sistemdeki gorevi: Manager ve validator string literal yerine bu sabitleri kullanir (golden rule 3: hard-coded string yok).
public static class DatabaseConnectionExceptionCodes
{
    private const string Prefix = "DatabaseChecker.DatabaseConnection";

    // Ayni kiraci icinde ayni takma ada sahip ikinci baglanti eklenmeye calisildiginda firlatilir.
    public const string NameAlreadyExists = $"{Prefix}:00001";

    // Pasif bir baglanti test, kesif veya run execution icin kullanildiginda firlatilir.
    public const string Inactive = $"{Prefix}:00002";

    // Baglanti entity'sinde desteklenmeyen bir TLS politika kodu kullanildiginda firlatilir.
    public const string InvalidTlsMode = $"{Prefix}:00003";

    // Baglanti testi hedef kimligin gerekenden fazla yazma veya yonetici yetkisi tasidigini bulgu olarak raporlar; throw edilmez.
    public const string ExcessivePrivilege = $"{Prefix}:00004";

    // Girdi-format validasyon kodlari; FluentValidation mesajlari bu sabitleri kullanir.
    public static class Validation
    {
        public const string EngineIdRequired = $"{Prefix}:Validation:EngineIdRequired";
        public const string NameRequired = $"{Prefix}:Validation:NameRequired";
        public const string NameMaxLength = $"{Prefix}:Validation:NameMaxLength";
        public const string HostRequired = $"{Prefix}:Validation:HostRequired";
        public const string HostMaxLength = $"{Prefix}:Validation:HostMaxLength";
        public const string PortOutOfRange = $"{Prefix}:Validation:PortOutOfRange";
        public const string DatabaseNameRequired = $"{Prefix}:Validation:DatabaseNameRequired";
        public const string DatabaseNameMaxLength = $"{Prefix}:Validation:DatabaseNameMaxLength";

        // Kimlik bilgileri (create'te zorunlu, update'te parola verilince kullanici adi zorunlu); sifre DB'ye degil Vault'a yazilir.
        public const string UsernameRequired = $"{Prefix}:Validation:UsernameRequired";
        public const string UsernameMaxLength = $"{Prefix}:Validation:UsernameMaxLength";
        public const string PasswordRequired = $"{Prefix}:Validation:PasswordRequired";
        public const string PasswordMaxLength = $"{Prefix}:Validation:PasswordMaxLength";

        // Update'te kullanici adi tek basina gonderildiginde firlatilir; secret ikili yazildigi icin parolasiz username sessizce kaybolurdu.
        public const string PasswordRequiredForUsernameChange = $"{Prefix}:Validation:PasswordRequiredForUsernameChange";
    }
}
