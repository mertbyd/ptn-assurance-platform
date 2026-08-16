using System.Text.Json;
using Ptn.ApiContractChecker.Models.Correlation;

namespace Ptn.ApiContractChecker.Models.Conformance;

// islevi: Request ve response assertion girdilerinin ortak operasyon ve payload yuzeyini tanimlar.
// sistemdeki gorevi: Ayni yedi kural metodunun iki yon tarafindan yeniden kullanilmasini saglar.
internal interface IConformanceObservation
{
    string? OperationId { get; }
    string Method { get; }
    string Path { get; }
    string? ContentType { get; }
    IReadOnlyDictionary<string, string> Headers { get; }
    JsonElement? Body { get; }
    string ProfileCode { get; }
    CorrelationRef? Correlation { get; }
}
