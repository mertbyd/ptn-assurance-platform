using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Npgsql;
using Ptn.DatabaseChecker.Connections;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Connections;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Comparison;

// islevi: Provider connection string'lerinin emniyet profilindeki TLS, timeout, GUC ve application-name kararlarini tasidigini dogrular.
// sistemdeki gorevi: Eski hard-coded timeout/TrustServerCertificate davranisinin geri gelmesini engelleyen provider siniri regresyon testidir.
public class DatabaseConnectionStringFactory_Tests
{
    [Fact]
    public void PostgreSql_Should_Carry_Startup_Gucs_And_Safety_Profile()
    {
        var info = CreateInfo(TlsModeCodes.Require, trustServerCertificate: false);

        var builder = new NpgsqlConnectionStringBuilder(DatabaseConnectionStringFactory.BuildPostgreSql(info));

        builder.Timeout.ShouldBe(11);
        builder.CommandTimeout.ShouldBe(31);
        builder.ApplicationName.ShouldBe("CheckNexus.DatabaseComparison/1.2.3");
        builder.SslMode.ShouldBe(SslMode.Require);
        builder.Options.ShouldNotBeNull();
        builder.Options!.ShouldContain("-c statement_timeout=31s");
        builder.Options.ShouldContain("-c lock_timeout=7s");
        builder.Options.ShouldContain("-c default_transaction_read_only=on");
    }

    [Fact]
    public void SqlServer_Should_Carry_Encryption_Trust_And_Timeout_Profile()
    {
        var info = CreateInfo(TlsModeCodes.Require, trustServerCertificate: true);

        var builder = new SqlConnectionStringBuilder(DatabaseConnectionStringFactory.BuildSqlServer(info));

        builder.ConnectTimeout.ShouldBe(11);
        builder.CommandTimeout.ShouldBe(31);
        builder.ApplicationName.ShouldBe("CheckNexus.DatabaseComparison/1.2.3");
        builder.Encrypt.ShouldBe(SqlConnectionEncryptOption.Mandatory);
        builder.TrustServerCertificate.ShouldBeTrue();
    }

    [Fact]
    public async Task SqlServer_Interceptor_Should_Apply_Lock_Timeout_Once_When_Connection_Opens()
    {
        var profile = CreateProfile(TlsModeCodes.Require, trustServerCertificate: false);
        var connection = new RecordingDbConnection();
        var interceptor = new SqlServerSessionInterceptor(profile);

        await interceptor.ConnectionOpenedAsync(connection, null!);

        connection.ExecutionCount.ShouldBe(1);
        connection.LastCommandText.ShouldBe("SET LOCK_TIMEOUT 7000;");
        connection.LastCommandTimeout.ShouldBe(31);
    }

    // islevi: Provider testlerine secret-loglamadan sabit runtime baglanti modeli kurar.
    private static DatabaseConnectionInfo CreateInfo(string tlsModeCode, bool trustServerCertificate)
        => new()
        {
            Host = "database.example",
            Port = 5432,
            DatabaseName = "sample",
            Username = "reader",
            Password = string.Empty,
            SafetyProfile = CreateProfile(tlsModeCode, trustServerCertificate)
        };

    // islevi: Connection string ve session testleri icin ortak emniyet profili kurar.
    private static ConnectionSafetyProfile CreateProfile(string tlsModeCode, bool trustServerCertificate)
        => new(
            connectTimeoutSeconds: 11,
            statementTimeoutSeconds: 31,
            lockTimeoutSeconds: 7,
            readOnlyTransaction: true,
            applicationName: "CheckNexus.DatabaseComparison/1.2.3",
            tlsModeCode,
            trustServerCertificate);

    // islevi: Interceptor testinde gercek SQL Server acmadan calistirilan session komutunu kaydeder.
    // sistemdeki gorevi: ConnectionOpenedAsync basina tek komut calistirildigini davranis seviyesinde kanitlar.
    private sealed class RecordingDbConnection : DbConnection
    {
        public int ExecutionCount { get; private set; }
        public string? LastCommandText { get; private set; }
        public int LastCommandTimeout { get; private set; }

        [AllowNull]
        public override string ConnectionString { get; set; } = string.Empty;
        public override string Database => "recording";
        public override string DataSource => "recording";
        public override string ServerVersion => "1";
        public override ConnectionState State => ConnectionState.Open;

        // islevi: Test baglantisinin veritabani degistirme gerektirmeyen no-op sozlesmesini saglar.
        public override void ChangeDatabase(string databaseName)
        {
        }

        // islevi: Test baglantisinin kapatma gerektirmeyen no-op sozlesmesini saglar.
        public override void Close()
        {
        }

        // islevi: Test baglantisini her zaman acik kabul eden no-op sozlesmesini saglar.
        public override void Open()
        {
        }

        // islevi: Session interceptor testinde transaction acilmadigini acikca korur.
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
            => throw new NotSupportedException();

        // islevi: Session initializer'in calistiracagi kaydedici komutu uretir.
        protected override DbCommand CreateDbCommand()
            => new RecordingDbCommand(this);

        // islevi: Calistirilan session komutunun sayisini ve profil alanlarini test icin kaydeder.
        private void Record(string commandText, int commandTimeout)
        {
            ExecutionCount++;
            LastCommandText = commandText;
            LastCommandTimeout = commandTimeout;
        }

        // islevi: Session komutunun parametre ve sonuc seti gerektirmeyen kaydedici DbCommand uygulamasidir.
        // sistemdeki gorevi: DbConnectionInterceptor testini dis SQL Server bagimliligi olmadan calistirir.
        private sealed class RecordingDbCommand : DbCommand
        {
            private readonly RecordingDbConnection _connection;
            private readonly SqlCommand _parameterSource = new();

            public RecordingDbCommand(RecordingDbConnection connection)
            {
                _connection = connection;
            }

            [AllowNull]
            public override string CommandText { get; set; } = string.Empty;
            public override int CommandTimeout { get; set; }
            public override CommandType CommandType { get; set; }
            public override UpdateRowSource UpdatedRowSource { get; set; }
            public override bool DesignTimeVisible { get; set; }
            [AllowNull]
            protected override DbConnection DbConnection { get; set; } = null!;
            protected override DbParameterCollection DbParameterCollection => _parameterSource.Parameters;
            protected override DbTransaction? DbTransaction { get; set; }

            // islevi: Session komutunun iptal desteigi gerektirmeyen test no-op sozlesmesini saglar.
            public override void Cancel()
            {
            }

            // islevi: Session komutu calistirmasini baglanti kaydedicisine tek kez aktarir.
            public override int ExecuteNonQuery()
            {
                _connection.Record(CommandText, CommandTimeout);
                return 0;
            }

            // islevi: Bu testte kullanilmayan scalar yolunu acikca reddeder.
            public override object? ExecuteScalar()
                => throw new NotSupportedException();

            // islevi: Parametre gerektirmeyen session komutu icin uyumlu SqlParameter uretir.
            protected override DbParameter CreateDbParameter()
                => new SqlParameter();

            // islevi: Bu testte kullanilmayan reader yolunu acikca reddeder.
            protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
                => throw new NotSupportedException();

            // islevi: Session komutunun hazirlik gerektirmeyen test no-op sozlesmesini saglar.
            public override void Prepare()
            {
            }
        }
    }
}
