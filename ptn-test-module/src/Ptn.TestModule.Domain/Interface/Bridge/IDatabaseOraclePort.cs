using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Models.Bridge;

namespace Ptn.TestModule.Interface.Bridge;

// islevi: Database checker assertion ve salt-okunur projeksiyon yeteneklerini domain-native modellerle tanimlar.
// sistemdeki gorevi: Serbest SQL'i ve checker DTO'larini domain zincirinin disinda tutar.
public interface IDatabaseOraclePort
{
    // Anahtarla secilen satirin kolon beklentilerini checker'a hukmettirir.
    Task<PtnAssertionResult> AssertRowAsync(PtnAssertionRequest request, CancellationToken cancellationToken);

    // Anahtarla secilen satir kumesinin cardinality beklentisini checker'a hukmettirir.
    Task<PtnAssertionResult> AssertCountAsync(PtnAssertionRequest request, CancellationToken cancellationToken);

    // Anahtarla secilen satirin bulunmadigini checker'a hukmettirir.
    Task<PtnAssertionResult> AssertAbsentAsync(PtnAssertionRequest request, CancellationToken cancellationToken);

    // Birden cok assertion'i tek checker cagrisi icinde bagimsiz sonuclariyla calistirir.
    Task<IReadOnlyList<PtnAssertionResult>> AssertBatchAsync(
        IReadOnlyList<PtnAssertionRequest> requests,
        CancellationToken cancellationToken);

    // Serbest SQL icermeyen salt-okunur projeksiyon yuzeyinden redaksiyonlu kanit ister.
    Task<PtnProjectionResult> ProjectAsync(PtnProjectionRequest request, CancellationToken cancellationToken);
}
