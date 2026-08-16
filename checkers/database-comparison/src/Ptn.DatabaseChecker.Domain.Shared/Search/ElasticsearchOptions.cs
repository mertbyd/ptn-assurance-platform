namespace Ptn.DatabaseChecker.Search;

// islevi: Elasticsearch baglanti ve arama ayarlarini typed options olarak tasir.
// sistemdeki gorevi: Url, fuzziness, timeout ve reindex batch degerleri kodda dagilmaz; appsettings "Elasticsearch" bolumu tek kaynaktir.
public class ElasticsearchOptions
{
    // appsettings bolum adi; binding tum katmanlarda bu sabitten yapilir.
    public const string SectionName = "Elasticsearch";

    // Metin aramalarinda kullanilan varsayilan fuzziness degeri.
    public const string DefaultFuzziness = "AUTO";

    // Reindex bulk istegi icin varsayilan dokuman sayisi.
    public const int DefaultReindexBatchSize = 500;

    // Elasticsearch istegi icin varsayilan timeout suresi.
    public const int DefaultRequestTimeoutSeconds = 30;

    // Elasticsearch endpoint adresi.
    public string? Url { get; set; }

    // Metin aramalarinda uygulanacak fuzziness seviyesi.
    public string Fuzziness { get; set; } = DefaultFuzziness;

    // Reindex sirasinda tek bulk istegine yazilacak dokuman sayisi.
    public int ReindexBatchSize { get; set; } = DefaultReindexBatchSize;

    // Elasticsearch client istek timeout suresi.
    public int RequestTimeoutSeconds { get; set; } = DefaultRequestTimeoutSeconds;
}
