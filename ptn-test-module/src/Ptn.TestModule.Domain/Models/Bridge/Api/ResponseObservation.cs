using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Ptn.TestModule.Models.Bridge;

// islevi: API checker'a gonderilecek gozlenen HTTP yanitini tipli ve govde-sinirli olarak tasir.
// sistemdeki gorevi: Response uygunluk portunu checker DTO'sundan ayirirken ham token tasinmasini engeller.
public sealed class ResponseObservation
{
    public Guid SnapshotId { get; set; }
    public string? OperationId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string? ContentType { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public JsonElement? Body { get; set; }
    public CorrelationRef? Correlation { get; set; }
}
