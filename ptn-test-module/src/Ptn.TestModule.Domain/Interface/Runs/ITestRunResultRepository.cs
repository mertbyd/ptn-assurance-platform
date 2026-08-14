using System;
using System.Threading;
using System.Threading.Tasks;
using Nexum.Abp.Foundation.Repositories;
using Ptn.TestModule.Entities.Runs;

namespace Ptn.TestModule.Interface.Runs;

// islevi: TestRunResult aggregate'inin deneme ve bulgulu okuma sorgularini tanimlar.
// sistemdeki gorevi: Attempt uretimi ile tek sorguluk terminal rapor okumalarini provider katmaninda tutar.
/// <summary>
/// Test kosum sonucu aggregate'i icin gereken ozel repository sorgularini tanimlar.
/// </summary>
public interface ITestRunResultRepository : IBaseRepository<TestRunResult, Guid>
{
    // Attempt null ise son denemeyi, sayi ise tam denemeyi getirir.
    /// <summary>Bir kosumun istenen veya en son terminal denemesini getirir.</summary>
    Task<TestRunResult?> FindByAttemptAsync(
        Guid testRunId,
        int? attempt = null,
        CancellationToken cancellationToken = default);

    // Aggregate'i tum finding cocuklariyla tek sorguda getirir.
    /// <summary>Terminal sonucu bulgulariyla birlikte tek sorguda getirir.</summary>
    Task<TestRunResult?> GetWithFindingsAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
