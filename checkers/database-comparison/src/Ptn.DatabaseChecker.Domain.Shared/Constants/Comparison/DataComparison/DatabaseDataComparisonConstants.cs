namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: Veri/migration karsilastirmasinin paylastigi EF migration history metadata adlarini toplar.
// sistemdeki gorevi: PostgreSQL ve SQL Server katalog konfigurasyonlari (__EFMigrationsHistory eslemesi) ile provider varlik-kontrol LINQ'leri ayni tablo/sema/kolon adlarini tek kaynaktan alir; bu adlar birden cok yerden referanslandigi icin sabit olarak yasar.
public static class DatabaseDataComparisonConstants
{
    public static class EntityFrameworkMigrationsHistory
    {
        // EF Core'un varsayilan migration history tablo adi; iki provider okuyucusu da ayni defteri arar.
        public const string TableName = "__EFMigrationsHistory";

        // Migration kimligini tasiyan kolon adi.
        public const string MigrationIdColumn = "MigrationId";

        // Migration'i uygulayan EF Core surumunu tasiyan kolon adi.
        public const string ProductVersionColumn = "ProductVersion";

        // PostgreSQL icin ilk T7 kapsaminda desteklenen varsayilan EF history semasi.
        public const string PostgreSqlDefaultSchema = "public";

        // SQL Server icin ilk T7 kapsaminda desteklenen varsayilan EF history semasi.
        public const string SqlServerDefaultSchema = "dbo";
    }
}
