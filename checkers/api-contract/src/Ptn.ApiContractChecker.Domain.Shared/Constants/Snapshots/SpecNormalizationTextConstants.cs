namespace Ptn.ApiContractChecker.Constants.Snapshots;

// islevi: Spec normalizasyonunda kullanilan desen, ayirac ve dokumantasyon hedef etiketlerini tek yerde sahiplenir.
// sistemdeki gorevi: Domain normalizer'i ile altyapi okuyucusunun kararli metin sozlesmelerini kod icindeki literallerden ayirir.
public static class SpecNormalizationTextConstants
{
    // islevi: Anlamsiz bicim farklarini eleyen regex ve ayiraclari gruplar.
    // sistemdeki gorevi: Okuyucu ile normalizer'in ayni kanonik metin sozlesmesini kullanmasini saglar.
    public static class Normalization
    {
        public const string WhitespacePattern = @"\s+";
        public const string PathParameterPattern = @"\{[^/{}]+\}";
        public const string TypeSeparatorPattern = @"[,|]";
        public const string SingleSpace = " ";
        public const string PathParameterMask = "{}";
        public const string TypeSeparator = "|";
        public const string NullType = "null";
    }

    // islevi: Yapisal modelden ayrilan dokumantasyon kayitlarinin hedef turlerini gruplar.
    // sistemdeki gorevi: Sonraki finding adiminda DocsOnly adreslerinin kararli kalmasini saglar.
    public static class DocumentationTargets
    {
        public const string Operation = "operation";
        public const string Parameter = "parameter";
        public const string RequestBody = "request-body";
        public const string Response = "response";
        public const string Header = "header";
        public const string Schema = "schema";
        public const string Property = "property";
    }
}
