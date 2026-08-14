using Volo.Abp.Modularity;

namespace Ptn.TestModule;

/* Inherit from this class for your domain layer tests.
 * See SampleManager_Tests for example.
 */
public abstract class TestModuleDomainTestBase<TStartupModule> : TestModuleTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
