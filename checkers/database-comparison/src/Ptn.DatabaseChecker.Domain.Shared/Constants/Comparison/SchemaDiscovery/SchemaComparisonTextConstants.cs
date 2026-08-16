namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: Sema kesif, normalizasyon ve karsilastirma motorunun paylastigi kararli katalog token, normalizasyon parcasi ve finding etiketlerini toplar.
// sistemdeki gorevi: Provider schema-discovery repository'leri (DefinitionTokens/Normalization) ve SchemaDefinitionNormalizer (NameListSeparator/Normalization) ayni katalog kelime ve regex'lerini; SchemaComparisonManager ise definition alan adlari (DefinitionFields) ve change-summary etiketlerini (ChangeLabels) tekrar hard-code etmeden buradan paylasir. Yalnizca motorun ic format ayiraclari (definition field/key-value/object-key/summary ayiraci) tek kullanicisi oldugu icin SchemaComparisonManager icinde private const kalir.
public static class SchemaComparisonTextConstants
{
    // Kolon/isim listelerinin kararli ayiraci.
    public const string NameListSeparator = ",";

    public static class DefinitionTokens
    {
        // Sequence taniminda kullanilan type kelimesi.
        public const string BigInt = "bigint";

        // Boolean evet token'i.
        public const string Yes = "YES";

        // Boolean hayir token'i.
        public const string No = "NO";

        // PostgreSQL normal view token'i.
        public const string View = "view";

        // PostgreSQL materialized view token'i.
        public const string MaterializedView = "materialized view";

        // PostgreSQL extension versiyon prefix'i.
        public const string ExtensionVersion = "extension version";

        // PostgreSQL enum type prefix'i.
        public const string Enum = "enum";

        // PostgreSQL domain type prefix'i.
        public const string DomainAs = "domain as";

        // SQL Server alias type prefix'i.
        public const string AliasAs = "alias as";

        // SQL Server table type tanimi.
        public const string TableType = "table type";

        // Sequence tanimi token'i.
        public const string Sequence = "sequence";

        // Generic "as" token'i.
        public const string As = "as";

        // Sequence start token'i.
        public const string Start = "start";

        // Sequence minimum token'i.
        public const string Minimum = "min";

        // Sequence maksimum token'i.
        public const string Maximum = "max";

        // Sequence increment token'i.
        public const string Increment = "increment";

        // Sequence cycle token'i.
        public const string Cycle = "cycle";

        // Sequence cache token'i.
        public const string Cache = "cache";
    }

    public static class Normalization
    {
        // Ardisik whitespace regex pattern'i.
        public const string WhitespacePattern = @"\s+";

        // Noktalama/operator cevresi whitespace regex pattern'i.
        public const string PunctuationSpacingPattern = @"\s*([(),;=+\-*/<>])\s*";

        // Regex replacement icin yakalanan operator token'i.
        public const string CapturedTokenReplacement = "$1";

        // Normalize edilen metinde kullanilan tek bosluk.
        public const string SingleSpace = " ";

        // Windows satir sonu.
        public const string WindowsNewLine = "\r\n";

        // Kanonik satir sonu.
        public const string NewLine = "\n";

        // Tek carriage-return karakteri.
        public const char CarriageReturn = '\r';

        // Tek line-feed karakteri.
        public const char LineFeed = '\n';

        // SQL statement sonlandiricisi.
        public const char StatementTerminator = ';';

        // SQL Server identifier quote acilis karakteri.
        public const char SqlServerIdentifierOpen = '[';

        // SQL Server identifier quote kapanis karakteri.
        public const char SqlServerIdentifierClose = ']';

        // ANSI/PostgreSQL identifier quote karakteri.
        public const char AnsiIdentifierQuote = '"';
    }

    // islevi: Provider index DDL'inden nesne adini ayirirken kullanilan kararli parse token ve placeholder'larini tanimlar.
    public static class IndexDefinitionParsing
    {
        // CREATE INDEX taniminda index adinin baslangicini gosteren token.
        public const string IndexKeyword = "INDEX ";

        // CREATE INDEX taniminda index adinin bittigi ve tablo bolumunun basladigi token.
        public const string OnKeyword = " ON ";

        // Semantik karsilastirmada gercek index adinin yerine konan kararli deger.
        public const string NamePlaceholder = "<index>";
    }

    // islevi: Kanonik definition string'indeki alan adlari; karsilastirma motorunun urettigi definition metnini olusturur.
    public static class DefinitionFields
    {
        public const string Type = "type";
        public const string CanonicalType = "canonicalType";
        public const string Nullable = "nullable";
        public const string MaxLength = "max";
        public const string NumericPrecision = "precision";
        public const string NumericScale = "scale";
        public const string Identity = "identity";
        public const string Default = "default";
        public const string Unique = "unique";
        public const string PrimaryKey = "primary";
        public const string Columns = "columns";
        public const string IncludedColumns = "included";
        public const string Filter = "filter";
        public const string Definition = "definition";
        public const string ReferencedTable = "referencedTable";
        public const string ReferencedColumns = "referencedColumns";
        public const string DeleteAction = "delete";
        public const string UpdateAction = "update";
        public const string Collation = "collation";
        public const string Generated = "generated";
        public const string GenerationExpression = "generationExpression";
        public const string Persisted = "persisted";
        public const string IdentitySeed = "identitySeed";
        public const string IdentityIncrement = "identityIncrement";
        public const string Comment = "comment";
        public const string Validated = "validated";
        public const string Enabled = "enabled";
        public const string Deferrable = "deferrable";
        public const string InitiallyDeferred = "initiallyDeferred";
        public const string DatabaseCollation = "databaseCollation";
        public const string CollationProvider = "collationProvider";
    }

    // islevi: ChangeSummary'de gorunen degisim etiketleri; modified bulgularinda hangi alanin degistigini isaretler.
    public static class ChangeLabels
    {
        public const string DataType = "DataType";
        public const string Ordinal = "Ordinal";
        public const string Nullable = "Nullable";
        public const string MaxLength = "MaxLength";
        public const string NumericPrecision = "NumericPrecision";
        public const string NumericScale = "NumericScale";
        public const string Identity = "Identity";
        public const string Default = "Default";
        public const string Unique = "Unique";
        public const string PrimaryKey = "PrimaryKey";
        public const string Columns = "Columns";
        public const string IncludedColumns = "IncludedColumns";
        public const string Filter = "Filter";
        public const string Definition = "Definition";
        public const string Type = "Type";
        public const string ReferencedTable = "ReferencedTable";
        public const string ReferencedColumns = "ReferencedColumns";
        public const string DeleteAction = "DeleteAction";
        public const string UpdateAction = "UpdateAction";
        public const string Collation = "Collation";
        public const string Generated = "Generated";
        public const string GenerationExpression = "GenerationExpression";
        public const string Persisted = "Persisted";
        public const string IdentitySeed = "IdentitySeed";
        public const string IdentityIncrement = "IdentityIncrement";
        public const string Comment = "Comment";
        public const string Validated = "Validated";
        public const string Enabled = "Enabled";
        public const string Deferrable = "Deferrable";
        public const string InitiallyDeferred = "InitiallyDeferred";
        public const string DatabaseCollation = "DatabaseCollation";
        public const string CollationProvider = "CollationProvider";
    }
}
