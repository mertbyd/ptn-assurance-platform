using Ptn.DatabaseChecker.Dtos.Correlation;

namespace Ptn.DatabaseChecker.Dtos.Capabilities;

// islevi: Capture kimligi, FK ile daraltilmis schema.table adaylari ve correlation bilgisini tasir.
// sistemdeki gorevi: Exact ve inferred yollarin ayni public request govdesini kullanmasini saglar.
public sealed class WriteSetCaptureRequestDto
{
    public Guid ConnectionId { get; set; }
    public Guid CaptureRef { get; set; }
    public List<string> CandidateTables { get; set; } = [];
    public CorrelationRefDto? Correlation { get; set; }
}
