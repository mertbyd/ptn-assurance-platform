using Ptn.ApiContractChecker.Configuration;
using Shouldly;
using Xunit;

namespace Ptn.ApiContractChecker.Options;

// islevi: Zamanlanmis izleme worker'inin periyot ve tik basi dokuman esiklerinin baslangic dogrulamasini sinar.
// sistemdeki gorevi: Hic tetiklenmeyen ya da Timer.Period'a sigmayan yapilandirmanin sessizce kabul edilmesini engeller.
public class SpecMonitoringOptionsValidator_Tests
{
    private readonly SpecMonitoringOptionsValidator _validator = new();

    // Varsayilan yapilandirmanin dogrulamayi gectigini kanitlar.
    [Fact]
    public void Default_Options_Should_Be_Valid()
    {
        var result = _validator.Validate(null, new SpecMonitoringOptions());

        result.Succeeded.ShouldBeTrue();
    }

    // Sifir, negatif ve ust siniri asan periyodun reddedildigini kanitlar.
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(SpecMonitoringOptions.MaxWorkerPeriodSeconds + 1)]
    public void Unusable_Worker_Period_Should_Fail(int workerPeriodSeconds)
    {
        var options = new SpecMonitoringOptions { WorkerPeriodSeconds = workerPeriodSeconds };

        var result = _validator.Validate(null, options);

        result.Failed.ShouldBeTrue();
    }

    // Hicbir dokumani kuyruga almayacak tik tavaninin reddedildigini kanitlar.
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Non_Positive_Document_Budget_Should_Fail(int maxDocumentsPerTick)
    {
        var options = new SpecMonitoringOptions { MaxDocumentsPerTick = maxDocumentsPerTick };

        var result = _validator.Validate(null, options);

        result.Failed.ShouldBeTrue();
    }
}
