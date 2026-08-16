namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: Sema snapshot'inda tasinan constraint turlerinin kararli kodlarini tanimlar.
// sistemdeki gorevi: PostgreSQL ve SQL Server katalog kodlarini ortak constraint turlerine cevirir; rapor/diff motoru vendor kodlarini bilmez.
public static class SchemaConstraintTypeCodes
{
    // Birincil anahtar constraint'i.
    public const string PrimaryKey = "PrimaryKey";

    // Unique constraint.
    public const string Unique = "Unique";

    // Foreign key constraint.
    public const string ForeignKey = "ForeignKey";

    // Check constraint.
    public const string Check = "Check";
}
