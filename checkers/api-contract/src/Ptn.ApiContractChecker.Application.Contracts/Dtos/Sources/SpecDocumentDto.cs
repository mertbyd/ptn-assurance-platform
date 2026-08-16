using Volo.Abp.Application.Dtos;

namespace Ptn.ApiContractChecker.Dtos.Sources;

// islevi: SpecSource aggregate'ine ait dokuman tanimini API sinirinda tasir.
// sistemdeki gorevi: Ayri CRUD acmadan dokumanlari kaynak detayinda ve kaynak mutasyon girdisinde temsil eder.
public class SpecDocumentDto : EntityDto<Guid>
{
    // Kaynak icindeki benzersiz dokuman adi.
    public string DocumentName { get; set; } = default!;

    // Kaynak taban adresine goreli spec yolu.
    public string Path { get; set; } = default!;

    // Dokumanin yeni cekimlerde secilebilirligini belirtir.
    public bool IsActive { get; set; } = true;
}
