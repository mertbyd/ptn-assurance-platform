namespace Ptn.TestModule.Models.Bridge;

// islevi: Yazilan yazarlik malzemesinin dosya adi, boyutu ve icerik muhrunu tasir.
// sistemdeki gorevi: Yukleme sonucunu AppService'te elle kurmak yerine muhur sahibinden alinan tek modele baglar.
public sealed class AuthoringSourceSeal
{
    public string FileName { get; set; } = string.Empty;
    public int ByteCount { get; set; }
    public string Fingerprint { get; set; } = string.Empty;
}
