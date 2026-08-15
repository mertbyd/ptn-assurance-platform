using System;

namespace Ptn.TestModule.Models.Bridge.Api;

// islevi: API checker operasyon secim isteginin tamamlanmis kaynak modelini tasir.
// sistemdeki gorevi: Manager'in sectigi verbosity kodunu Mapperly ile checker DTO'suna tasir.
public sealed class ApiOperationRequest
{
    public Guid SnapshotId { get; set; }
    public string? OperationId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string VerbosityCode { get; set; } = string.Empty;
}
