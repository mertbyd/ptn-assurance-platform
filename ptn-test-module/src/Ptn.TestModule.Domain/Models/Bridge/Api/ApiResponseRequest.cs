using System;
using System.Collections.Generic;
using System.Text.Json;
using Ptn.TestModule.Models.Bridge;

namespace Ptn.TestModule.Models.Bridge.Api;

// islevi: API checker response uygunluk isteginin tamamlanmis kaynak modelini tasir.
// sistemdeki gorevi: Manager'in sectigi profil kodunu Mapperly ile checker DTO'suna tasir.
public sealed class ApiResponseRequest
{
    public Guid SnapshotId { get; set; }
    public string? OperationId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string? ContentType { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public JsonElement? Body { get; set; }
    public string ProfileCode { get; set; } = string.Empty;
    public CorrelationRef? Correlation { get; set; }
}
