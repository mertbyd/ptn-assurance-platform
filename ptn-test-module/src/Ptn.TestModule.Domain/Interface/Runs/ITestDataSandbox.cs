using System.Threading;
using System.Threading.Tasks;

namespace Ptn.TestModule.Interface.Runs;

// islevi: SUT test verisini checker kimliginden ayri, yazma yetkili bir baglantiyla sifirlayan capability'yi tanimlar.
// sistemdeki gorevi: Checker'in salt-okunur invariant'ini korurken kosum onkosullarini bilinen bir veri tabanina getirir (ADR-0007).
/// <summary>
/// Mantiksal test ortaminin yazma yetkili sandbox verisini kosumdan once sifirlar.
/// </summary>
public interface ITestDataSandbox
{
    // Ortama ozel ayri baglantiyi secip yapilandirilan reset stratejisini uygular.
    /// <summary>Verilen mantiksal ortamin sandbox verisini bilinen bos duruma getirir.</summary>
    Task ResetAsync(
        string environmentKey,
        CancellationToken cancellationToken = default);
}
