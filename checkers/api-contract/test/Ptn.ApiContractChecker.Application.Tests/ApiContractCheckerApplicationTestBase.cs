using Volo.Abp.Modularity;

namespace Ptn.ApiContractChecker;

/* Inherit from this class for your application layer tests.
 * See SampleAppService_Tests for example.
 */
public abstract class ApiContractCheckerApplicationTestBase<TStartupModule> : ApiContractCheckerTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
