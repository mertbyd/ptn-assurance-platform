using System.Text.Json;
using Ptn.ApiContractChecker.Constants.Snapshots;

namespace Ptn.ApiContractChecker.Models.Snapshots;

// islevi: Tek operasyonun request, basari response ve security yazarlik ozetini tasir.
// sistemdeki gorevi: Tam OpenAPI govdesini acmadan 2 KB altinda operation.find cevabi verir.
public class OperationSummaryResult
{
    public string OutcomeCode { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? OperationId { get; set; }
    public List<OperationParameterSummary> RequiredParameters { get; set; } = [];
    public List<string> RequestMediaTypes { get; set; } = [];
    public List<string> SuccessStatusCodes { get; set; } = [];
    public List<SchemaFieldSummary> ResponseFields { get; set; } = [];
    public List<string> SecurityRequirements { get; set; } = [];
    public bool IsTruncated { get; set; }
    public string? ResultRef { get; set; }

    // islevi: Alan listesini once verbosity sonra UTF-8 tavanina gore kararli sondan kirpar.
    public void TrimToBudget(int fieldLimit)
    {
        if (ResponseFields.Count > fieldLimit)
        {
            ResponseFields.RemoveRange(fieldLimit, ResponseFields.Count - fieldLimit);
            IsTruncated = true;
        }

        while (MeasureUtf8Bytes() > SnapshotAuthoringConstants.MaxSummaryBytes && ResponseFields.Count > 0)
        {
            ResponseFields.RemoveAt(ResponseFields.Count - 1);
            IsTruncated = true;
        }
    }

    // islevi: Operasyon ozetinin UTF-8 JSON boyutunu olcer.
    public int MeasureUtf8Bytes() => JsonSerializer.SerializeToUtf8Bytes(this).Length;
}
