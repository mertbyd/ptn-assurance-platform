namespace Ptn.TestModule.Dtos.Authoring;

// islevi: Yazarlik malzemesinin yukleme sonrasi ad, boyut ve muhur sonucunu tasir.
// sistemdeki gorevi: MCP Resource'un okudugu baytlarla ayni muhru cagirana geri vererek kaynak tekligini kanitlar.
public sealed class AuthoringSourceDto
{
    /// <summary>
    /// Kaynagin ayarli kokteki sabit dosya adini belirtir.
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    /// <summary>
    /// Yazilan icerigin bayt cinsinden uzunlugunu belirtir.
    /// </summary>
    public int ByteCount { get; set; }
    /// <summary>
    /// Icerigin lowercase sha256: sozlesmesindeki muhrunu belirtir.
    /// </summary>
    public string Fingerprint { get; set; } = string.Empty;
}
