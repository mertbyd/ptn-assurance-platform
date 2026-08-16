using System.Text.Json;

namespace Ptn.ApiContractChecker.Models.Snapshots;

// islevi: Snapshot operasyon envanterinin toplam sayisini, hafif satirlarini ve acik sayfa/byte butcesini tasir.
// sistemdeki gorevi: Kapsam raporunun paydasini olculebilir kilarken sessiz sayfa kirpmayi engeller.
public class SnapshotOperationInventoryResult
{
    public string OutcomeCode { get; set; } = string.Empty;
    public long TotalCount { get; set; }
    public List<SpecOperationRow> Items { get; } = [];
    public int RequestedMaxResultCount { get; set; }
    public int EffectiveMaxResultCount { get; set; }
    public bool IsTruncated { get; set; }
    public int ResponseBytes { get; set; }

    // islevi: Son satirlari kararli sirayla cikararak serilestirilmis sonucu byte tavanina indirger.
    public void TrimToBudget(int maxResponseBytes)
    {
        ResponseBytes = MeasureUtf8Bytes();
        while (ResponseBytes > maxResponseBytes && Items.Count > 0)
        {
            Items.RemoveAt(Items.Count - 1);
            IsTruncated = true;
            EffectiveMaxResultCount = Items.Count;
            ResponseBytes = MeasureUtf8Bytes();
        }
    }

    // islevi: Mevcut envanter sonucunun UTF-8 JSON boyutunu olcer.
    private int MeasureUtf8Bytes() => JsonSerializer.SerializeToUtf8Bytes(this).Length;
}
