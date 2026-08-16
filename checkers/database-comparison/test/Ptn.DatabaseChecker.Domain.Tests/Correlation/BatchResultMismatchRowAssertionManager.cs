using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Managers.Assertions;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Models.Assertions;
using Volo.Abp.Timing;

namespace Ptn.DatabaseChecker.Correlation;

// islevi: Batch public akisina istek sayisindan az sonuc ureten kararli bir test yurutme adimi saglar.
// sistemdeki gorevi: Manager'in kismi sonucu dondurmek yerine BatchResultCountMismatch firlattigini dis yuzeyden kanitlar.
internal sealed class BatchResultMismatchRowAssertionManager : RowAssertionManager
{
    private readonly List<RowAssertionResult> _results;

    // islevi: Test manager'ini gercek domain bagimliliklari ve kontrollu kismi sonuc listesiyle kurar.
    public BatchResultMismatchRowAssertionManager(
        DatabaseDataComparisonManager dataManager,
        ValueMatcherEvaluator matcher,
        AssertionSettingsResolver settingsResolver,
        ValueRetentionPolicyResolver retentionPolicyResolver,
        FindingValueRedactor redactor,
        IClock clock,
        List<RowAssertionResult> results)
        : base(dataManager, matcher, settingsResolver, retentionPolicyResolver, redactor, clock)
    {
        _results = results;
    }

    // islevi: Count mismatch kapisini tetiklemek icin verilen kismi sonucu public batch akisina dondurur.
    protected override Task<List<RowAssertionResult>> ExecuteBatchAsync(
        IReadOnlyDictionary<Guid, DatabaseConnection> connectionsById,
        List<RowAssertionRequest> requests,
        CancellationToken cancellationToken)
        => Task.FromResult(_results);
}
