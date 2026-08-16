using System.Collections.Generic;
using Volo.Abp.Domain.Values;

namespace Ptn.DatabaseChecker.Models.Connections;

// islevi: Hedef baglantinin timeout, read-only, uygulama kimligi ve TLS kararlarini degismez bir deger nesnesinde toplar.
// sistemdeki gorevi: Tek runtime DatabaseConnectionInfo modeli uzerinden tester ve katalog context'lerine ayni emniyet kararlarini tasir.
public sealed class ConnectionSafetyProfile : ValueObject
{
    public int ConnectTimeoutSeconds { get; }
    public int StatementTimeoutSeconds { get; }
    public int LockTimeoutSeconds { get; }
    public bool ReadOnlyTransaction { get; }
    public string ApplicationName { get; }
    public string TlsModeCode { get; }
    public bool TrustServerCertificate { get; }

    // islevi: Cozulmus baglanti emniyeti kararlarini degismez profil olarak kurar.
    public ConnectionSafetyProfile(
        int connectTimeoutSeconds,
        int statementTimeoutSeconds,
        int lockTimeoutSeconds,
        bool readOnlyTransaction,
        string applicationName,
        string tlsModeCode,
        bool trustServerCertificate)
    {
        ConnectTimeoutSeconds = connectTimeoutSeconds;
        StatementTimeoutSeconds = statementTimeoutSeconds;
        LockTimeoutSeconds = lockTimeoutSeconds;
        ReadOnlyTransaction = readOnlyTransaction;
        ApplicationName = applicationName;
        TlsModeCode = tlsModeCode;
        TrustServerCertificate = trustServerCertificate;
    }

    // islevi: ABP value-object esitligi icin profil kararlarini kararli sirada verir.
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return ConnectTimeoutSeconds;
        yield return StatementTimeoutSeconds;
        yield return LockTimeoutSeconds;
        yield return ReadOnlyTransaction;
        yield return ApplicationName;
        yield return TlsModeCode;
        yield return TrustServerCertificate;
    }
}
