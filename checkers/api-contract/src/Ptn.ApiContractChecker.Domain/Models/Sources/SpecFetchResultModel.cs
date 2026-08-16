namespace Ptn.ApiContractChecker.Models.Sources;

// islevi: Guard'lardan gecen ham spec baytlarini medya tipi ve olculmus boyutuyla tasir.
// sistemdeki gorevi: KBP-609 ayristirma ve snapshot hattina HTTP tipi ya da credential sizdirmayan ic cikis saglar.
public class SpecFetchResultModel
{
    // Hash ve ayristirma icin kodlama kaybi olmadan korunan ham response govdesi.
    public byte[] Content { get; }

    // Content-Type basligindan dogrulanip normalize edilen medya tipi.
    public string MediaType { get; }

    // Content-Length'e degil gercekten okunan ham baytlara dayanan boyut.
    public int ByteSize { get; }

    // Dogrulanmis ham govdeyi olculmus boyutuyla tek seferde kurar.
    public SpecFetchResultModel(byte[] content, string mediaType)
    {
        Content = content;
        MediaType = mediaType;
        ByteSize = content.Length;
    }
}
