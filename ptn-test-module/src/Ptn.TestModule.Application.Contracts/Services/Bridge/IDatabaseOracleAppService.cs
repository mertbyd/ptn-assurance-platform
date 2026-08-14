using Ptn.TestModule.Dtos.Bridge.Database;
using Volo.Abp.Application.Services;

namespace Ptn.TestModule.Services.Bridge;

// islevi: Database assertion ve salt-okunur projeksiyon kullanim senaryolarini tanimlar.
// sistemdeki gorevi: Checker DTO'larini public Test Module sozlesmesinin disinda tutar.
public interface IDatabaseOracleAppService : IApplicationService
{
    // Tek satirin kolon beklentilerini denetler.
    Task<AssertionResultDto> AssertRowAsync(DatabaseAssertionRequestDto input, CancellationToken cancellationToken);

    // Anahtarla secilen satir kumesinin kardinalitesini denetler.
    Task<AssertionResultDto> AssertCountAsync(DatabaseAssertionRequestDto input, CancellationToken cancellationToken);

    // Anahtarla secilen satirin bulunmadigini denetler.
    Task<AssertionResultDto> AssertAbsentAsync(DatabaseAssertionRequestDto input, CancellationToken cancellationToken);

    // Birden cok assertion'i tek sirali sonuc listesi olarak denetler.
    Task<IReadOnlyList<AssertionResultDto>> AssertBatchAsync(DatabaseAssertionBatchRequestDto input, CancellationToken cancellationToken);

    // Izinli kolonlar icin redaksiyonlu projeksiyon ister.
    Task<ProjectionResultDto> ProjectAsync(ProjectionRequestDto input, CancellationToken cancellationToken);
}
