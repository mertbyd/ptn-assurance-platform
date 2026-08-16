namespace Ptn.DatabaseChecker.Dtos.Assertions;

// islevi: Turetilebilirligi sinanacak tek DB assertion tablo/kolon/anahtar/matcher adresini tasir.
// sistemdeki gorevi: Output item seklini genisletmeden toplu derivability request'inin typed ogesidir.
public sealed class DerivabilityAddressDto
{
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public List<string> KeyColumns { get; set; } = [];
    public List<string> ExpectedColumns { get; set; } = [];
    public string MatcherCode { get; set; } = string.Empty;
    public string CardinalityKindCode { get; set; } = string.Empty;
}
