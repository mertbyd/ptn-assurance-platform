using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: API assertion turetilebilirlik kontrolunun operasyon ve JSON pointer girdilerini tasir.
// sistemdeki gorevi: Yayin kapisinin serbest assertion kodu yerine tipli pointer listesiyle calismasini saglar.
public sealed class PtnDerivabilityRequest
{
    public Guid SnapshotId { get; set; }
    public string? OperationId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? StatusCode { get; set; }
    public string? MediaType { get; set; }
    public List<string> AssertionPaths { get; set; } = [];
}
