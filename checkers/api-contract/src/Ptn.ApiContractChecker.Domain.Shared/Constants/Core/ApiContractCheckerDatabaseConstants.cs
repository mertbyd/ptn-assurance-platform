namespace Ptn.ApiContractChecker.Constants;

// islevi: Uygulama ve bagli ABP modullerinin kararli veritabani adlarini ve kolon tiplerini tanimlar.
// sistemdeki gorevi: Domain.Shared disinda sema, baglanti, kolon ve provider tipi literal'i kalmasini engeller.
public static class ApiContractCheckerDatabaseConstants
{
    public const string EmptyTablePrefix = "";
    public const string ConnectionStringName = "ApiContractChecker";
    public const string DefaultConnectionStringName = "Default";
    public const string MigrationsHistoryTableName = "__api_contract_checker_migrations_history";
    public const string AbpSchema = "abp";
    public const string OpenIddictSchema = "openiddict";
    public const string OperatorsSchema = "operator";
    public const string CheckerSchema = "checker";
    public const string EmailSchema = "email";
    public const string SqliteProviderName = "Microsoft.EntityFrameworkCore.Sqlite";
}
