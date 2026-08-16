using Ptn.DatabaseChecker.Models.Correlation;

namespace Ptn.DatabaseChecker.Models.Capabilities;

// islevi: Bir capture penceresinin kimligini, FK ile daraltilmis aday tablolarini ve correlation bilgisini tasir.
// sistemdeki gorevi: AppService girdisini provider tiplerinden bagimsiz Manager strateji secimine iletir.
public sealed class WriteSetCaptureRequest
{
    public Guid CaptureRef { get; set; }
    public List<string> CandidateTables { get; set; } = [];
    public CorrelationRef? Correlation { get; set; }
}
