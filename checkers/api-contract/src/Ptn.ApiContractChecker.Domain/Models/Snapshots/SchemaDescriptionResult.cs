using System.Text.Json;
using Ptn.ApiContractChecker.Constants.Snapshots;

namespace Ptn.ApiContractChecker.Models.Snapshots;

// islevi: Tek component semasinin bir seviyelik alan, enum ve ref ozetini tasir.
// sistemdeki gorevi: schema.describe cevabini tam spec govdesinden ayirip 2 KB ile sinirlar.
public class SchemaDescriptionResult
{
    public string OutcomeCode { get; set; } = string.Empty;
    public string SchemaRef { get; set; } = string.Empty;
    public string? Type { get; set; }
    public bool Nullable { get; set; }
    public List<string> EnumValues { get; set; } = [];
    public List<SchemaFieldSummary> Fields { get; set; } = [];
    public bool IsTruncated { get; set; }
    public string? ResultRef { get; set; }

    // islevi: Bir seviyelik alan listesini verbosity ve byte tavanina indirger.
    public void TrimToBudget(int fieldLimit)
    {
        if (Fields.Count > fieldLimit)
        {
            Fields.RemoveRange(fieldLimit, Fields.Count - fieldLimit);
            IsTruncated = true;
        }

        while (MeasureUtf8Bytes() > SnapshotAuthoringConstants.MaxSummaryBytes && Fields.Count > 0)
        {
            Fields.RemoveAt(Fields.Count - 1);
            IsTruncated = true;
        }
    }

    // islevi: Sema ozetinin UTF-8 JSON boyutunu olcer.
    public int MeasureUtf8Bytes() => JsonSerializer.SerializeToUtf8Bytes(this).Length;
}
