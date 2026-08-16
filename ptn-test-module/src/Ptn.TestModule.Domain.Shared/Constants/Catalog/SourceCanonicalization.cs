namespace Ptn.TestModule.Constants.Catalog;

// islevi: Test senaryosu kaynak belgelerinin (Arazzo vb.) imza ve mühür hesaplaması öncesi kanonikleştirilmesini tanımlar.
// sistemdeki gorevi: Ajanın ürettiği ile kullanıcının yüklediği veya düzenlediği aynı içerikli belgelerin sunucuda tek bir hash (SourceHash) altında birleşmesini garanti eder.
public static class SourceCanonicalization
{
    // Sunucu tarafı tekil satır sonu biçimidir.
    public const string LineEnding = "\n";
    
    // Hash hesaplamasında kullanılan geçerli kanonikleştirme kuralının izleme kodudur.
    // Açıklama: UTF-8 (BOM yok), satır sonları '\n', satır sonu boşluklar kırpılır, belge sonu boş satırlar kırpılır.
    public const string RuleName = "ptn-source-canonical-v1";
}
