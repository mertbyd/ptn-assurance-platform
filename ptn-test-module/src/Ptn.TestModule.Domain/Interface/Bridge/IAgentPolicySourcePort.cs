using System.Threading;
using System.Threading.Tasks;

namespace Ptn.TestModule.Interface.Bridge;

// islevi: Ajan politikasi belgesinin kaynagini ham icerik olarak okuyan capability'yi tanimlar.
// sistemdeki gorevi: Politikayi assembly'ye gomulu kopya yerine ayarli tek kaynaktan adresler.
/// <summary>
/// Ajan politikasi kaynaginin ham icerigini dondiren sozlesmedir.
/// </summary>
public interface IAgentPolicySourcePort
{
    // Ayarli kokten ajan politikasi belgesini okur ve ham baytlarini getirir.
    /// <summary>Yapilandirilmis ajan politikasi belgesinin ham baytlarini getirir.</summary>
    Task<byte[]> ReadAsync(CancellationToken cancellationToken = default);
}
