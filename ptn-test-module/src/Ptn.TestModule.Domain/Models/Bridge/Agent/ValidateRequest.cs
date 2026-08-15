using System;
using System.Collections.Generic;
using Ptn.TestModule.Models.Bridge.Database;
namespace Ptn.TestModule.Models.Bridge.Agent;
// islevi: Yayinlanabilirlik yoklamasinin snapshot, operasyon ve assertion referanslarini tasir.
// sistemdeki gorevi: Assertion pointer veya operasyon adresini public yuzeyde serbest metin olarak tasimaz.
public sealed class ValidateRequest
{
    public string ProfileKey { get; set; } = string.Empty;
    public Guid ConnectionId { get; set; }
    public Guid SpecSnapshotId { get; set; }
    public Guid OperationReferenceId { get; set; }
    public List<Guid> AssertionReferenceIds { get; set; } = [];
    public List<DatabaseDerivabilityAddress> DatabaseAssertions { get; set; } = [];
    public string ResponseFormat { get; set; } = string.Empty;
}
