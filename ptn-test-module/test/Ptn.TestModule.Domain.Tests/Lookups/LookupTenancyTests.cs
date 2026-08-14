using System;
using System.Collections.Generic;
using System.Linq;
using Ptn.TestModule.Entities.Lookups;
using Shouldly;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace Ptn.TestModule.Lookups;

// islevi: Lookup entity'lerinin kiraci filtresine girmedigini dogrular.
// sistemdeki gorevi: Lookup'lar global referans verisidir; IMultiTenant eklenirse referans satirlari kiraci basina bolunur (ADR-0016 §D).
public class LookupTenancyTests
{
    // Bes lookup tipinin tamami; yeni bir lookup eklendiginde bu liste de buyutulur.
    public static IEnumerable<object[]> LookupTypes =>
    [
        [typeof(TestRunStatus)],
        [typeof(TestOutcomeStatus)],
        [typeof(TestFailureCategory)],
        [typeof(TestTriggerKind)],
        [typeof(TestScenarioState)]
    ];

    // Hicbir lookup tipi IMultiTenant uygulamamalidir.
    [Theory]
    [MemberData(nameof(LookupTypes))]
    public void Should_not_implement_multi_tenancy(Type lookupType)
    {
        typeof(IMultiTenant).IsAssignableFrom(lookupType).ShouldBeFalse();
    }

    // Test listesi lookup klasoruyle birlikte buyusun; unutulan tip sessizce denetim disinda kalmasin.
    [Fact]
    public void Should_cover_every_lookup_entity_in_the_assembly()
    {
        var declaredLookupTypes = typeof(TestRunStatus).Assembly.GetTypes()
            .Where(type => type is { IsAbstract: false, IsClass: true }
                           && type.Namespace == typeof(TestRunStatus).Namespace)
            .ToList();

        var coveredTypes = LookupTypes.Select(data => (Type)data[0]).ToList();

        declaredLookupTypes.ShouldBe(coveredTypes, ignoreOrder: true);
    }
}
