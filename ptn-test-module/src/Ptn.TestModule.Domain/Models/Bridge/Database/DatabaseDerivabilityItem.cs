namespace Ptn.TestModule.Models.Bridge.Database;

// islevi: Checker'dan gelen tek DB assertion turetilebilirlik olgusunu tasir.
// sistemdeki gorevi: Kaynak outcome kodunun Bridge sozlugunde normalize edilecegi veri kabugudur.
public sealed class DatabaseDerivabilityItem
{
    public string TableRef { get; set; } = string.Empty;
    public string ColumnRef { get; set; } = string.Empty;
    public string OutcomeCode { get; set; } = string.Empty;
}
