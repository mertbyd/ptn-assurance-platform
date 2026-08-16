using Volo.Abp.Modularity;

namespace Ptn.DatabaseChecker;

/* Inherit from this class for your domain layer tests.
 * See SampleManager_Tests for example.
 */
public abstract class DatabaseCheckerDomainTestBase<TStartupModule> : DatabaseCheckerTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
