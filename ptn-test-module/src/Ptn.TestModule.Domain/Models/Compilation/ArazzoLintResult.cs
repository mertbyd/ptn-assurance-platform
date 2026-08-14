namespace Ptn.TestModule.Models.Compilation;

// islevi: Pinli Redocly lint isleminin sema kararini ve butceli tanisini tasir.
// sistemdeki gorevi: Surec cikisini Domain Manager'a ham Process veya Docker ayrintisi sizdirmadan verir.
public sealed class ArazzoLintResult
{
    public bool IsValid { get; set; }
    public string Diagnostics { get; set; } = string.Empty;
}
