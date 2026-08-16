using System.Text.Json;

namespace Ptn.ApiContractChecker.Models.Runs;

// islevi: Toplam sayi, etkili sayfa boyutu ve acik kirpilma bilgisini bulgu satirlariyla tasir.
// sistemdeki gorevi: ABP PagedResult DTO'suna map edilmeden once 32 KB butcesini olculebilir kilar.
public class FindingPageResultModel
{
    public long TotalCount { get; init; }
    public List<FindingReadModel> Items { get; } = [];
    public int RequestedMaxResultCount { get; init; }
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

    // islevi: Mevcut sonuc modelinin UTF-8 JSON boyutunu olcer.
    private int MeasureUtf8Bytes() => JsonSerializer.SerializeToUtf8Bytes(this).Length;
}
