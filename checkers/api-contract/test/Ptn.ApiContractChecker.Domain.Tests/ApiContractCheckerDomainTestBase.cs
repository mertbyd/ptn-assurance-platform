using Volo.Abp.Modularity;

namespace Ptn.ApiContractChecker;

/* Inherit from this class for your domain layer tests.
 * See SampleManager_Tests for example.
 */
public abstract class ApiContractCheckerDomainTestBase<TStartupModule> : ApiContractCheckerTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
