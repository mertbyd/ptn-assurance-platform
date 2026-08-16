using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Controllers.Conformance;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Shouldly;
using Volo.Abp.EntityFrameworkCore;
using Xunit;

namespace Ptn.ApiContractChecker.EntityFrameworkCore;

// islevi: Composition paketinin ince hostta controller ve EF model yuzeylerini birlikte yukledigini dogrular.
// sistemdeki gorevi: NuGet consumer'i modulu eklediginde initialization, route ve model kayiplarini release oncesi yakalar.
[Collection(EfCoreIntegrationCollection.Name)]
public class PackageCompositionSmoke_Tests : ApiContractCheckerEntityFrameworkCoreTestBase
{
    // Composition modulunun public conformance route'unu ve checker entity modelini ayni servis grafiginde acar.
    [Fact]
    public async Task Composition_Module_Should_Expose_Route_And_Ef_Model()
    {
        var controllerFeature = new ControllerFeature();
        GetRequiredService<ApplicationPartManager>().PopulateFeature(controllerFeature);
        controllerFeature.Controllers.ShouldContain(controller =>
            controller.AsType() == typeof(ResponseConformanceController));

        var route = typeof(ResponseConformanceController).GetCustomAttributes(typeof(RouteAttribute), false)
            .Cast<RouteAttribute>()
            .Single();
        route.Template.ShouldBe(ApiContractCheckerRoutes.Conformance);
        var group = typeof(ResponseConformanceController)
            .GetCustomAttributes(typeof(ApiExplorerSettingsAttribute), false)
            .Cast<ApiExplorerSettingsAttribute>()
            .Single();
        group.GroupName.ShouldBe(ApiContractCheckerSwaggerConstants.ConformanceGroupName);

        await WithUnitOfWorkAsync(async () =>
        {
            var provider = GetRequiredService<IDbContextProvider<ApiContractCheckerDbContext>>();
            var dbContext = await provider.GetDbContextAsync();
            dbContext.Model.FindEntityType(typeof(SpecSnapshot)).ShouldNotBeNull();
        });
    }
}
