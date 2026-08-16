using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Models.Comparison;

namespace Ptn.DatabaseChecker.Interface.Comparison;

// islevi: Bir motora ozel "baglanabiliyor muyuz?" testinin sozlesmesi.
// sistemdeki gorevi: AppService kimligi Vault'tan cozup bu sozlesmeye verir; implementasyon dogru surucuyle (Npgsql/SqlClient) baglanir, sonucu (istisna degil) ConnectionTestResult olarak doner. EngineComponentResolver dogru olani EngineCode ile secer.
public interface IDatabaseConnectionTester : IEngineComponent
{
    // Verilen baglanti bilgisiyle hedefe baglanmayi dener; salt-okur, hedefe yazmaz. Sonucu (basari/hata) rapor eder.
    Task<ConnectionTestResult> TestAsync(DatabaseConnectionInfo info, CancellationToken cancellationToken = default);
}
