using Volo.Abp.Modularity;

namespace Ptn.DatabaseChecker;

/* Inherit from this class for your application layer tests.
 * See SampleAppService_Tests for example.
 */
public abstract class DatabaseCheckerApplicationTestBase<TStartupModule> : DatabaseCheckerTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
