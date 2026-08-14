using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Kanit zincirinin ozne, operasyon, izin ve calisma baglamini tasir.
// sistemdeki gorevi: Teshis girdisini HTTP ve checker DTO'larindan bagimsiz domain verisi yapar.
public sealed class PtnAccessTuple
{
    public Guid ConnectionId { get; set; }
    public Guid? SpecSnapshotId { get; set; }
    public string SubjectRef { get; set; } = string.Empty;
    public string OperationId { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int? StatusCode { get; set; }
    public List<string> RequiredPermissions { get; set; } = [];
    public Dictionary<string, string?> Context { get; set; } = new(StringComparer.Ordinal);
}
