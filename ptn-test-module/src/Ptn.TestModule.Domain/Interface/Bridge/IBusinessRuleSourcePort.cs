using System.Threading;
using System.Threading.Tasks;

namespace Ptn.TestModule.Interface.Bridge;

// islevi: Test edilecek yazilimin is kurali kaynagini ham icerik olarak okuyan capability'yi tanimlar.
// sistemdeki gorevi: Ayar, dosya ve kok siniri ayrintisini tek sinir arkasinda toplar; Manager yalniz muhur uretir.
/// <summary>
/// Is kurali kaynaginin ham icerigini dondiren sozlesmedir.
/// </summary>
public interface IBusinessRuleSourcePort
{
    // Ayarli kokten is kurali belgesini okur ve ham baytlarini getirir.
    /// <summary>Yapilandirilmis is kurali belgesinin ham baytlarini getirir.</summary>
    Task<byte[]> ReadAsync(CancellationToken cancellationToken = default);

    // Ayarli koke ayni dosya adiyla yazar; okuma ve muhur ayni baytlari gorur.
    /// <summary>Is kurali belgesini yapilandirilmis kokteki sabit adina yazar.</summary>
    Task WriteAsync(byte[] content, CancellationToken cancellationToken = default);
}
