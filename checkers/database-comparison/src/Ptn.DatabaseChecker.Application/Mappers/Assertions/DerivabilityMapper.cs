using Ptn.DatabaseChecker.Dtos.Assertions;
using Ptn.DatabaseChecker.Models.Assertions;
using Riok.Mapperly.Abstractions;

namespace Ptn.DatabaseChecker.Application.Mappers.Assertions;

// islevi: Derivability request/result DTO ve domain modelleri arasindaki donusumleri Mapperly ile uretir.
// sistemdeki gorevi: AppService'i adres veya outcome alanlarini elle kopyalamaktan uzak tutar.
[Mapper]
public partial class DerivabilityMapper
{
    // islevi: Public derivability girdisini toplu domain request'ine tasir.
    public partial DerivabilityRequest MapToRequest(DerivabilityRequestDto input);

    // islevi: Domain derivability sonucunu adres+outcome public cevabina tasir.
    public partial DerivabilityResultDto MapToResultDto(DerivabilityResult result);

    // islevi: Public assertion adresini domain request item'ina tasir.
    private partial DerivabilityAddress MapAddress(DerivabilityAddressDto input);

    // islevi: Domain outcome item'ini yalniz tablo/kolon referansi ve outcome DTO'suna tasir.
    private partial DerivabilityItemDto MapItem(DerivabilityItem input);
}
