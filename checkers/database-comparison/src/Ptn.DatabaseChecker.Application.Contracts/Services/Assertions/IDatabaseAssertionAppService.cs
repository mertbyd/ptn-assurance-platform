using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Dtos.Assertions;
using Volo.Abp.Application.Services;

namespace Ptn.DatabaseChecker.Services.Assertions;

// islevi: Test Module icin row, count, absence ve batch assertion use-case'lerini tanimlar.
// sistemdeki gorevi: Tum uclar ayni RowAssertionManager cekirdegine gider; batch tek HTTP/MCP round-trip'te coklu sonuc dondurur.
public interface IDatabaseAssertionAppService : IApplicationService
{
    // islevi: Anahtarla secilen tek satirin kolon beklentilerini dogrular.
    Task<RowAssertionResultDto> AssertRowAsync(
        RowAssertionRequestDto input,
        CancellationToken cancellationToken = default);

    // islevi: Anahtarla secilen satir kumesinin cardinality beklentisini dogrular.
    Task<RowAssertionResultDto> AssertCountAsync(
        RowAssertionRequestDto input,
        CancellationToken cancellationToken = default);

    // islevi: Anahtarla secilen satirin bulunmadigini ayni cardinality cekirdeginde dogrular.
    Task<RowAssertionResultDto> AssertAbsentAsync(
        RowAssertionRequestDto input,
        CancellationToken cancellationToken = default);

    // islevi: Birden cok assertion'i tek uygulama cagrisi icinde bagimsiz sonuclariyla calistirir.
    Task<List<RowAssertionResultDto>> AssertBatchAsync(
        List<RowAssertionRequestDto> input,
        CancellationToken cancellationToken = default);
}
