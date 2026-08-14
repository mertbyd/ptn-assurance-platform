using Ptn.TestModule.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Ptn.TestModule;

public abstract class TestModuleController : AbpControllerBase
{
    protected TestModuleController()
    {
        LocalizationResource = typeof(TestModuleResource);
    }
}
