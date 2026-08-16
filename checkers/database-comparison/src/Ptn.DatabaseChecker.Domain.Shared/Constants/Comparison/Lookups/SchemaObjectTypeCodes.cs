namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: Sema nesne turu lookup satirlarinin kararli Code degerlerini sabitler.
// sistemdeki gorevi: Seed, FK cozumleme ve rapor gruplama mantigi ayni string'e baglanir; enum'un derleme-zamani kimlik rolunu bu sabitler devralir.
public static class SchemaObjectTypeCodes
{
    public const string Database = "Database";
    public const string Table = "Table";
    public const string View = "View";
    public const string Trigger = "Trigger";
    public const string Procedure = "Procedure";
    public const string Function = "Function";
    public const string Column = "Column";
    public const string Index = "Index";
    public const string PrimaryKey = "PrimaryKey";
    public const string ForeignKey = "ForeignKey";
    public const string Unique = "Unique";
    public const string Check = "Check";
    public const string Sequence = "Sequence";
    public const string Type = "Type";
    public const string Extension = "Extension";
    /// <summary>EF migration defteri bulgularinin ortak okuma turu.</summary>
    public const string Migration = "Migration";

    /// <summary>Kodun bulgu sorgusunda desteklenen kapali nesne turu katalogunda olup olmadigini bildirir.</summary>
    public static bool IsDefined(string? code)
        => code is Database or Table or View or Trigger or Procedure or Function or Column or Index or
            PrimaryKey or ForeignKey or Unique or Check or Sequence or Type or Extension or Migration;
}
