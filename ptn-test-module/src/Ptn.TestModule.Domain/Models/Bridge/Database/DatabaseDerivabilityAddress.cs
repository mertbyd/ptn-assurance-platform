using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge.Database;

// islevi: DB assertion'in katalog, anahtar, beklenti ve matcher adresini domain sinirinda tasir.
// sistemdeki gorevi: Checker wire DTO'sunu yayin kapisi kararindan ayirir.
public sealed class DatabaseDerivabilityAddress
{
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public List<string> KeyColumns { get; set; } = [];
    public List<string> ExpectedColumns { get; set; } = [];
    public string MatcherCode { get; set; } = string.Empty;
    public string CardinalityKindCode { get; set; } = string.Empty;
}
