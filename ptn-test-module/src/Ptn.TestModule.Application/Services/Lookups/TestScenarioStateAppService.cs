using System;
using System.Collections.Generic;
using Nexum.Abp.Foundation.Lookups;
using Ptn.TestModule.Dtos.Lookups;
using Ptn.TestModule.Entities.Lookups;
using Ptn.TestModule.Interface.Lookups;
using Ptn.TestModule.Managers.Lookups;
using Ptn.TestModule.Mappers.Lookups;
using Ptn.TestModule.Permissions;
using Volo.Abp;

namespace Ptn.TestModule.Services.Lookups;

// islevi: Senaryo yayin durumu lookup'inin okuma use-case'lerini Foundation lookup tabani uzerinden sunar.
// sistemdeki gorevi: Okuma akisi tabandan gelir; bu tip yalniz okuma policy'sini ve Mapperly eslemesini sahiplenir.
[RemoteService(IsEnabled = false)]
public class TestScenarioStateAppService : LookupAppService<
    TestScenarioState,
    Guid,
    TestScenarioStateDto,
    LookupCreateDto,
    LookupUpdateDto,
    TestScenarioStateManager,
    ITestScenarioStateRepository>, ITestScenarioStateAppService
{
    /// <summary>Lookup dikeyinin saf katmanlar-arasi eslemelerini yapar.</summary>
    private static readonly TestLookupMapper Mapper = new();

    protected override string GetPolicyName => TestModulePermissions.Lookups.Default;

    // Lookup satirini Mapperly ile public okuma DTO'suna cevirir.
    protected override TestScenarioStateDto MapToDto(TestScenarioState entity)
    {
        return Mapper.Map(entity);
    }

    // Lookup sayfasini tek Mapperly collection eslemesiyle DTO listesine cevirir.
    protected override List<TestScenarioStateDto> MapToDto(IReadOnlyList<TestScenarioState> entities)
    {
        return Mapper.Map([.. entities]);
    }
}
