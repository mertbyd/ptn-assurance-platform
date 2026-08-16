namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: FK ON UPDATE/ON DELETE davranislarini motor-bagimsiz kodlara cevirir.
// sistemdeki gorevi: PostgreSQL karakter kodlari ve SQL Server sayisal kodlari rapor/diff katmanina sizmadan ortak dilde tasinir.
public static class SchemaReferentialActionCodes
{
    // NO ACTION davranisi.
    public const string NoAction = "NoAction";

    // RESTRICT davranisi.
    public const string Restrict = "Restrict";

    // CASCADE davranisi.
    public const string Cascade = "Cascade";

    // SET NULL davranisi.
    public const string SetNull = "SetNull";

    // SET DEFAULT davranisi.
    public const string SetDefault = "SetDefault";

    // Provider'in beklenmeyen/haritalanmamis davranis kodu.
    public const string Unknown = "Unknown";
}
