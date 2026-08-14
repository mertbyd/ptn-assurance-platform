using System.Text.Json;

namespace Ptn.TestModule.Dtos.Bridge.Api;

// islevi: Sozlesmeye karsi denetlenecek HTTP yanit gozlemini tasir.
// sistemdeki gorevi: Public response assertion girdisini tipli ve govde-sinirli tutar.
public sealed class ResponseObservationDto
{
    public Guid SnapshotId { get; set; }
    public string? OperationId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string? ContentType { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public JsonElement? Body { get; set; }
}
