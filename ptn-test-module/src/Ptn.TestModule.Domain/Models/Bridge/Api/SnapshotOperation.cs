using System;

namespace Ptn.TestModule.Models.Bridge.Api;

// islevi: Checker snapshot envanterindeki tek gercek operasyon satirini tasir.
// sistemdeki gorevi: Checker DTO'sunu sizdirmadan kapali secim referansi ve API adresini bir arada tutar.
public sealed class SnapshotOperation
{
    public Guid ReferenceId { get; set; }
    public string? OperationId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? RequestSchemaRef { get; set; }
    public string? ResponseSchemaRef { get; set; }
}
