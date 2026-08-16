using Ptn.ApiContractChecker.Constants.Snapshots;

namespace Ptn.ApiContractChecker.Configuration;

// islevi: Canli spec cevabinin izinli medya tiplerini ve azami ham bayt boyutunu typed options olarak tasir.
// sistemdeki gorevi: HTTP guard esiklerini appsettings ile tek sozlesmede bulusturur; esik dogrulamasi SpecFetchOptionsValidator'in isidir.
public class SpecFetchOptions
{
    public const string SectionName = "SpecFetch";
    public const int DefaultMaxContentSizeBytes = 10_485_760;

    // Ham icerik, SpecContent.ByteSize ile uyumlu int sinirinda tutulur.
    public int MaxContentSizeBytes { get; set; } = DefaultMaxContentSizeBytes;

    // Kabul edilen spec medya tipleri; sozlesmenin sahibi SpecMediaTypes'tir.
    public string[] AllowedMediaTypes { get; set; } = [.. SpecMediaTypes.All];
}
