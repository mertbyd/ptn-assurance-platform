using Ptn.ApiContractChecker.Entities.Sources;
using Ptn.ApiContractChecker.Models.Sources;

namespace Ptn.ApiContractChecker.Interface.Sources;

// islevi: Bir SpecSource'un aktif dokumanlarini dis sistemde yoklayan provider sinirini tanimlar.
// sistemdeki gorevi: Domain manager'i HTTP istemcisi ve Vault cozumleme ayrintilarindan bagimsiz tutar.
public interface ISpecSourceReachabilityTester
{
    // Aktif dokumanlari kaynak kimligiyle yoklayip secretsiz sonucu dondurur.
    Task<SpecSourceReachabilityModel> TestAsync(
        SpecSource source,
        IReadOnlyCollection<SpecDocument> documents);
}
