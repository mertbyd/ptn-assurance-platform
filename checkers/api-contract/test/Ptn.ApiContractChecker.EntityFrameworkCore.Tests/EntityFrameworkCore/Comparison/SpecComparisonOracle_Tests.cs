using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Managers.Comparison;
using Shouldly;
using Xunit;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Comparison;

// islevi: Checked-in spec ciftlerini gercek okuyucu ve karsilastirma motoruyla kosup beklenen bulgu kumesine kilitler.
// sistemdeki gorevi: oasdiff oracle turunda mutabik kalinan davranisin CI'da sessizce kaymasini engelleyen regresyon kapisidir.
[Collection(EfCoreIntegrationCollection.Name)]
public class SpecComparisonOracle_Tests : ApiContractCheckerEntityFrameworkCoreTestBase
{
    // Beklenen dosyalarini diske yeniden yazmak icin kullanilan opt-in ortam degiskeni.
    private const string RewriteExpectationsVariable = "ACC_ORACLE_REWRITE";

    // KBP-617 oracle turunda mutabik kalinan cift sayisi; fixture kopyalanmazsa kapi bos kosmaz.
    private const int ExpectedMinimumCaseCount = 19;

    private readonly ISpecDocumentReader _reader;
    private readonly SpecComparisonExecutionManager _executionManager;

    // Gercek okuyucuyu ve tam karsilastirma pipeline'ini test konteynerinden alir.
    public SpecComparisonOracle_Tests()
    {
        _reader = GetRequiredService<ISpecDocumentReader>();
        _executionManager = GetRequiredService<SpecComparisonExecutionManager>();
    }

    // Fixture kumesinin gercekten kopyalandigini kanitlar; aksi halde Theory sessizce bos kosardi.
    [Fact]
    public void Fixture_Set_Should_Be_Present_And_Complete()
    {
        var cases = SpecComparisonOracleFixture.Cases().Select(data => (string)data[0]).ToList();

        cases.Count.ShouldBeGreaterThanOrEqualTo(ExpectedMinimumCaseCount);
        foreach (var caseName in cases)
        {
            File.Exists(SpecComparisonOracleFixture.PathOf(
                caseName, SpecComparisonOracleFixture.BaseDocumentFileName)).ShouldBeTrue(caseName);
            File.Exists(SpecComparisonOracleFixture.PathOf(
                caseName, SpecComparisonOracleFixture.TargetDocumentFileName)).ShouldBeTrue(caseName);
            File.Exists(SpecComparisonOracleFixture.PathOf(
                caseName, SpecComparisonOracleFixture.ExpectedFindingsFileName)).ShouldBeTrue(caseName);
        }
    }

    // Her fixture ciftinin motorumuzda tam olarak mutabik kalinan bulgu kumesini urettigini kanitlar.
    [Theory]
    [MemberData(nameof(SpecComparisonOracleFixture.Cases), MemberType = typeof(SpecComparisonOracleFixture))]
    public async Task Fixture_Pair_Should_Produce_The_Agreed_Finding_Set(string caseName)
    {
        var actual = await CompareAsync(caseName);

        if (Environment.GetEnvironmentVariable(RewriteExpectationsVariable) == "1")
        {
            SpecComparisonOracleFixture.WriteExpected(caseName, actual);
            return;
        }

        actual.ShouldBe(SpecComparisonOracleFixture.ReadExpected(caseName));
    }

    // Ayni cifti iki kez karsilastirmanin bulgu sirasini ve icerigini degistirmedigini kanitlar.
    [Theory]
    [MemberData(nameof(SpecComparisonOracleFixture.Cases), MemberType = typeof(SpecComparisonOracleFixture))]
    public async Task Fixture_Pair_Should_Compare_Deterministically(string caseName)
    {
        var first = await CompareAsync(caseName);
        var second = await CompareAsync(caseName);

        second.ShouldBe(first);
    }

    // Bir cifti ayni spec'le karsilastirmanin hic fark uretmedigini kanitlar.
    [Theory]
    [MemberData(nameof(SpecComparisonOracleFixture.Cases), MemberType = typeof(SpecComparisonOracleFixture))]
    public async Task Comparing_A_Document_With_Itself_Should_Produce_No_Finding(string caseName)
    {
        var document = SpecComparisonOracleFixture.ReadDocument(
            caseName,
            SpecComparisonOracleFixture.TargetDocumentFileName);

        var snapshot = (await _reader.ReadAsync(document)).Snapshot;

        _executionManager.Compare(snapshot, snapshot).Items.ShouldBeEmpty();
    }

    // Fixture ciftini okuyup motoru kosar ve bulgulari kararli satirlara indirger.
    private async Task<IReadOnlyList<string>> CompareAsync(string caseName)
    {
        var baseDocument = SpecComparisonOracleFixture.ReadDocument(
            caseName,
            SpecComparisonOracleFixture.BaseDocumentFileName);
        var targetDocument = SpecComparisonOracleFixture.ReadDocument(
            caseName,
            SpecComparisonOracleFixture.TargetDocumentFileName);

        var baseSnapshot = (await _reader.ReadAsync(baseDocument)).Snapshot;
        var targetSnapshot = (await _reader.ReadAsync(targetDocument)).Snapshot;

        return _executionManager
            .Compare(baseSnapshot, targetSnapshot)
            .Items
            .Select(SpecComparisonOracleFixture.Describe)
            .ToList();
    }
}
