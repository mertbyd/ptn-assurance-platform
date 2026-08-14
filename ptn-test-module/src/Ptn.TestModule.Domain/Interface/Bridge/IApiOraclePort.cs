using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Models.Bridge;

namespace Ptn.TestModule.Interface.Bridge;

// islevi: API checker yazarlik ve uygunluk yeteneklerini domain-native modellerle tanimlar.
// sistemdeki gorevi: Manager ve AppService katmanlarinin checker DTO'larini dogrudan cagirmasini engeller.
public interface IApiOraclePort
{
    // Operasyon baglama adaylarini checker'dan normalize edilmis sonuc olarak ister.
    Task<PtnOperationBinding> SuggestOperationBindingsAsync(
        PtnOperationQuery query,
        CancellationToken cancellationToken);

    // Secili operasyon icin minimal ve placeholder-isaretli request ornegini ister.
    Task<PtnRequestExample> BuildRequestExampleAsync(
        PtnOperationQuery query,
        CancellationToken cancellationToken);

    // Assertion pointer'larinin sozlesmeden turetilebilirligini checker'a sorar.
    Task<PtnDerivabilityResult> ValidateScenarioAssertionsAsync(
        PtnDerivabilityRequest request,
        CancellationToken cancellationToken);

    // Gozlenen HTTP yanitini API sozlesmesine karsi checker'a hukmettirir.
    Task<PtnConformanceResult> AssertResponseAsync(
        PtnResponseObservation observation,
        CancellationToken cancellationToken);
}
