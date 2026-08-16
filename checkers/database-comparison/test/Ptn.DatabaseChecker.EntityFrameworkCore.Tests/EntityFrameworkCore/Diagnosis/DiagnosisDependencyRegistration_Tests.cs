using System.Collections.Generic;
using System.Linq;
using Ptn.DatabaseChecker.Interface.Diagnosis;
using Ptn.DatabaseChecker.Managers.Diagnosis;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Diagnosis;

// islevi: Diagnosis kurali, probe ve engine extractor koleksiyonlarinin ABP conventional DI ile eksiksiz toplandigini dogrular.
// sistemdeki gorevi: Yeni hipotezin tek dosyayla koleksiyona katilmasi ve motor seciminin runtime'da eksik servis vermemesi icin wiring regresyon kanitidir.
public class DiagnosisDependencyRegistration_Tests : DatabaseCheckerEntityFrameworkCoreTestBase
{
    // islevi: On kural, uc probe, iki extractor ve DiagnosisManager'in runtime DI'dan cozuldugunu dogrular.
    [Fact]
    public void Should_Register_All_Rules_Probes_And_Extractors()
    {
        var rules = GetRequiredService<IEnumerable<IDiagnosisRule>>().ToList();
        var probes = GetRequiredService<IEnumerable<IDiagnosisProbe>>().ToList();
        var extractors = GetRequiredService<IEnumerable<IFailureIdentityExtractor>>().ToList();
        var manager = GetRequiredService<DiagnosisManager>();

        rules.Count.ShouldBe(10);
        probes.Count.ShouldBe(3);
        extractors.Count.ShouldBe(2);
        manager.ShouldNotBeNull();
    }
}
