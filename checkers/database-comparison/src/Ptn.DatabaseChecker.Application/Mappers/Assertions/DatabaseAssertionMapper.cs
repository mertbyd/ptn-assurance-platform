using System.Collections.Generic;
using Ptn.DatabaseChecker.Dtos.Assertions;
using Ptn.DatabaseChecker.Models.Assertions;
using Riok.Mapperly.Abstractions;

namespace Ptn.DatabaseChecker.Application.Mappers.Assertions;

// islevi: Assertion request ve result modellerinin DTO donusumlerini Mapperly ile uretir.
// sistemdeki gorevi: AppService'in katmanlar arasi alanlari elle kopyalamadan tek domain cekirdegini cagirmasini saglar.
[Mapper]
public partial class DatabaseAssertionMapper
{
    // islevi: Tek public assertion DTO'sunu persist edilmeyen domain request'ine tasir.
    public partial RowAssertionRequest MapToRequest(RowAssertionRequestDto input);

    // islevi: Assertion DTO listesini domain request listesine tasir.
    public partial List<RowAssertionRequest> MapToRequests(List<RowAssertionRequestDto> input);

    // islevi: Tek domain assertion sonucunu API cevap DTO'suna tasir.
    public partial RowAssertionResultDto MapToResultDto(RowAssertionResult result);

    // islevi: Domain assertion sonuc listesini API cevap DTO listesine tasir.
    public partial List<RowAssertionResultDto> MapToResultDtos(List<RowAssertionResult> results);
}
