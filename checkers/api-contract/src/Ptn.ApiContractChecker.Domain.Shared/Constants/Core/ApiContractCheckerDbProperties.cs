namespace Ptn.ApiContractChecker;

// islevi: Uygulamanin kendi tablolarinin yasadigi veritabani semalarini ve baglanti dizesi adini tutar.
// sistemdeki gorevi: EF configuration dosyalari sema adini buradan okur; ortam bazli override EFCore modulunun ConfigureSchemas metodunda yapilir.
public static class ApiContractCheckerDbProperties
{
    public static string DbTablePrefix { get; set; } = Constants.ApiContractCheckerDatabaseConstants.EmptyTablePrefix;

    // Ortak Authenticator kullanicisinin checker tarafindaki opsiyonel operator projeksiyonu icin ayrilan sema.
    public static string OperatorsSchema { get; set; } = Constants.ApiContractCheckerDatabaseConstants.OperatorsSchema;

    // Is-alani tablolari: spec kaynaklari, dokumanlar, icerik, anlik goruntuler ve calistirmalar.
    public static string CheckerSchema { get; set; } = Constants.ApiContractCheckerDatabaseConstants.CheckerSchema;

    // Ortak Notifications/Emailing modulunun composition sirasinda kullanabilecegi sema sozlesmesi.
    public static string EmailSchema { get; set; } = Constants.ApiContractCheckerDatabaseConstants.EmailSchema;

    public const string ConnectionStringName = Constants.ApiContractCheckerDatabaseConstants.ConnectionStringName;
}
