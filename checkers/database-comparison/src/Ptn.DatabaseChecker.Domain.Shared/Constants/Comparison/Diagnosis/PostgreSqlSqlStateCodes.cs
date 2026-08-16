namespace Ptn.DatabaseChecker.Constants.Diagnosis;

// islevi: PostgreSQL SQLSTATE siniflari ile yapilandirilmis provider alan ve setting adlarini tek yerde tanimlar.
// sistemdeki gorevi: Kimlik cikaricinin mesaj parse etmeden sinif-23 guvenini ve katalog nesne referanslarini cikarmasini saglar.
public static class PostgreSqlSqlStateCodes
{
    public const string IntegrityConstraintClassPrefix = "23";
    public const string ForeignKeyViolation = "23503";
    public const string UniqueViolation = "23505";
    public const string GeneratedAlways = "428C9";

    // islevi: Npgsql'in mesajdan bagimsiz yapilandirilmis nesne alan adlarini gruplar.
    // sistemdeki gorevi: Extractor'in yalniz izinli sema, tablo, kolon ve constraint alanlarini okumasini saglar.
    public static class ProviderFields
    {
        public const string SchemaName = "schema_name";
        public const string TableName = "table_name";
        public const string ColumnName = "column_name";
        public const string ConstraintName = "constraint_name";
    }

    // islevi: Diagnosis tarafindan okunmasina izin verilen PostgreSQL setting adlarini gruplar.
    // sistemdeki gorevi: ServerSettingProbe'a kullanici kontrollu serbest setting adi girmesini engeller.
    public static class SettingNames
    {
        public const string SearchPath = "search_path";
        public const string Collation = "lc_collate";
    }
}
