using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Entities.Catalog;
using Ptn.TestModule.Models.Compilation;

namespace Ptn.TestModule.Interface.Compilation;

// islevi: Muhurlu senaryo malzemesinden yayin kapisinin okudugu makine kanitini ureten capability'yi tanimlar.
// sistemdeki gorevi: Profil cozumu, derleme ve iki turetilebilirlik cagrisini yayin kararindan ayirir (ADR-0015 §C).
/// <summary>
/// Senaryoyu derleyip yayin kapisinin okudugu makine kanitini dondiren sozlesmedir.
/// </summary>
public interface IScenarioCompilationPort
{
    // Profili cozer, belgeyi derler ve iki checker yuzeyine turetilebilirligi sorar; karar vermez.
    /// <summary>Senaryonun muhurlu malzemesinden tam derleme kanitini getirir.</summary>
    Task<ScenarioCompilationEvidence> CompileAsync(
        TestScenario scenario,
        CancellationToken cancellationToken = default);
}
