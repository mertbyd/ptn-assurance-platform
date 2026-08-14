namespace Ptn.TestModule.Models.Bridge.Api;

// islevi: API problem details icindeki tek yapisal hata adresini ve kodunu tasir.
// sistemdeki gorevi: API diagnosis request modelinin checker DTO'suyla tipli navigation eslemesini tamamlar.
public sealed class PtnProblemError
{
    public string? Pointer { get; set; }
    public string? Code { get; set; }
}
