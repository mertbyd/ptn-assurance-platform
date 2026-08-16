namespace Ptn.DatabaseChecker.Constants;

// islevi: Database Checker ve ortak SaaS modullerinin kararli veritabani sema adlarini tanimlar.
// sistemdeki gorevi: Test Module ile ortak composition hostun ayni kalici sema sozlesmesini kullanmasini saglar.
public static class DatabaseCheckerDatabaseConstants
{
    public const string ConnectionStringName = "DatabaseChecker";
    public const string DefaultConnectionStringName = "Default";
    public const string MigrationsHistoryTableName = "__database_checker_migrations_history";
    public const string AbpSchema = "abp";
    public const string OpenIddictSchema = "openiddict";
    public const string LookupsSchema = "lookup";
    public const string ConnectionsSchema = "connection";
    public const string DefinitionsSchema = "definition";
    public const string RunsSchema = "run";
    public const string OperatorsSchema = "operator";
    public const string ComparisonSchema = "comparison";
    public const string EmailSchema = "email";
}
