using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ptn.TestModule.Interface.Bridge;

// islevi: Profil paketi kokundeki yazma ve anahtar listeleme capability'sini tanimlar.
// sistemdeki gorevi: Dosya sistemi erisimini Domain disinda tutar; Manager yalniz ad, butce ve kok kurallarini verir.
/// <summary>
/// Profil paketi kaynagina yazan ve yuklu anahtarlari listeleyen sozlesmedir.
/// </summary>
public interface IProfilePackSourcePort
{
    // Profil paketini ayarli kokte <key>.yaml adiyla olusturur veya degistirir.
    /// <summary>Profil paketi icerigini yapilandirilmis kokteki anahtar dosyasina yazar.</summary>
    Task WriteAsync(string profileKey, byte[] content, CancellationToken cancellationToken = default);

    // Ayarli kokteki yuklu profil paketi anahtarlarini kararli sirada getirir.
    /// <summary>Yapilandirilmis kokte bulunan profil paketi anahtarlarini getirir.</summary>
    Task<IReadOnlyList<string>> ListKeysAsync(CancellationToken cancellationToken = default);
}
