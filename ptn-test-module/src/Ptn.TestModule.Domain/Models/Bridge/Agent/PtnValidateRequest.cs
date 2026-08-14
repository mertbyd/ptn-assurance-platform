using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge.Agent;

// islevi: Yayinlanabilirlik yoklamasinin snapshot, operasyon ve assertion referanslarini tasir.
// sistemdeki gorevi: Assertion pointer veya operasyon adresini public yuzeyde serbest metin olarak tasimaz.
public sealed class PtnValidateRequest
{
    public string ProfileKey { get; set; } = string.Empty;
    public Guid ConnectionId { get; set; }
    public Guid SpecSnapshotId { get; set; }
    public Guid OperationReferenceId { get; set; }
    public List<Guid> AssertionReferenceIds { get; set; } = [];
    public string ResponseFormat { get; set; } = string.Empty;
}
