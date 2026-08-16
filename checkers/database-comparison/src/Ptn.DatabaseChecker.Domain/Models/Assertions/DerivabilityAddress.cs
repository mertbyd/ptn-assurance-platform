namespace Ptn.DatabaseChecker.Models.Assertions;

// islevi: Turetilebilirligi sinanacak tek veritabani assertion adresini ve matcher niyetini tasir.
// sistemdeki gorevi: Request girdisini sonuc item seklinden ayirarak output'un yalniz referans ve outcome yayimlamasini saglar.
public sealed class DerivabilityAddress
{
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public List<string> KeyColumns { get; set; } = [];
    public List<string> ExpectedColumns { get; set; } = [];
    public string MatcherCode { get; set; } = string.Empty;
    public string CardinalityKindCode { get; set; } = string.Empty;
}
