namespace Ptn.DatabaseChecker.ExceptionCodes;

// islevi: Tum lookup CRUD akislarinin paylastigi hata ve validasyon kodlarini tek kaynakta tanimlar.
// sistemdeki gorevi: Lookup'lar generic tek dikeyden yonetildigi icin hata kodlari da tek merkezde toplanir; string literal yerine bu sabitler kullanilir (golden rule 3: hard-coded string yok).
public static class LookupExceptionCodes
{
    private const string Prefix = "DatabaseChecker.Lookup";

    // Ayni Code'a sahip ikinci bir lookup satiri eklenmeye/guncellenmeye calisildiginda firlatilir.
    public const string CodeAlreadyExists = $"{Prefix}:00001";

    // Girdi-format validasyon kodlari; FluentValidation mesajlari bu sabitleri kullanir.
    public static class Validation
    {
        // Code alani icin bos/uzunluk kurallari.
        public const string CodeRequired = $"{Prefix}:Validation:CodeRequired";
        public const string CodeMaxLength = $"{Prefix}:Validation:CodeMaxLength";

        // Name alani icin bos/uzunluk kurallari.
        public const string NameRequired = $"{Prefix}:Validation:NameRequired";
        public const string NameMaxLength = $"{Prefix}:Validation:NameMaxLength";

        // Description alani icin uzunluk kurali (opsiyonel alan, bos olabilir).
        public const string DescriptionMaxLength = $"{Prefix}:Validation:DescriptionMaxLength";
    }
}
