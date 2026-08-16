namespace Ptn.DatabaseChecker;

// islevi: Database Checker ve ortak entegrasyon yuzeylerinin ortam bazli sema adlarini tutar.
// sistemdeki gorevi: EF configuration ile Test Module ve ortak SaaS hostun ayni kalici sema sozlesmesini kullanmasini saglar.
public static class DatabaseCheckerDbProperties
{
    public static string DbTablePrefix { get; set; } = "";

    public static string LookupsSchema { get; set; } = Constants.DatabaseCheckerDatabaseConstants.LookupsSchema;
    public static string ConnectionsSchema { get; set; } = Constants.DatabaseCheckerDatabaseConstants.ConnectionsSchema;
    public static string DefinitionsSchema { get; set; } = Constants.DatabaseCheckerDatabaseConstants.DefinitionsSchema;
    public static string RunsSchema { get; set; } = Constants.DatabaseCheckerDatabaseConstants.RunsSchema;
    public static string OperatorsSchema { get; set; } = Constants.DatabaseCheckerDatabaseConstants.OperatorsSchema;
    public static string ComparisonSchema { get; set; } = Constants.DatabaseCheckerDatabaseConstants.ComparisonSchema;
    public static string EmailSchema { get; set; } = Constants.DatabaseCheckerDatabaseConstants.EmailSchema;

    public const string ConnectionStringName = Constants.DatabaseCheckerDatabaseConstants.ConnectionStringName;
}
