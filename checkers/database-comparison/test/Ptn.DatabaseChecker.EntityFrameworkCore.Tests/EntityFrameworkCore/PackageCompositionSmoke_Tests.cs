using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Ptn.DatabaseChecker.Constants;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Controllers.Capabilities;
using Ptn.DatabaseChecker.Controllers.Assertions;
using Ptn.DatabaseChecker.Controllers.Projections;
using Ptn.DatabaseChecker.Entities.Runs;
using Ptn.DatabaseChecker.Interface.Capabilities;
using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Permissions;
using Ptn.DatabaseChecker.Repository.Comparison;
using Shouldly;
using Volo.Abp.EntityFrameworkCore;
using Xunit;

namespace Ptn.DatabaseChecker.EntityFrameworkCore;

// islevi: Composition paketinin ince hostta controller ve EF model yuzeylerini birlikte yukledigini dogrular.
// sistemdeki gorevi: NuGet consumer'i modulu eklediginde initialization, route ve model kayiplarini release oncesi yakalar.
public class PackageCompositionSmoke_Tests : DatabaseCheckerEntityFrameworkCoreTestBase
{
    // Composition modulunun public assertion route'unu ve checker entity modelini ayni servis grafiginde acar.
    [Fact]
    public async Task Composition_Module_Should_Expose_Route_And_Ef_Model()
    {
        var controllerFeature = new ControllerFeature();
        GetRequiredService<ApplicationPartManager>().PopulateFeature(controllerFeature);
        controllerFeature.Controllers.ShouldContain(controller =>
            controller.AsType() == typeof(AssertionController));

        var route = typeof(AssertionController).GetCustomAttributes(typeof(RouteAttribute), false)
            .Cast<RouteAttribute>()
            .Single();
        route.Template.ShouldBe(DatabaseCheckerHttpApiConstants.Routes.Assertions);
        var group = typeof(AssertionController)
            .GetCustomAttributes(typeof(ApiExplorerSettingsAttribute), false)
            .Cast<ApiExplorerSettingsAttribute>()
            .Single();
        group.GroupName.ShouldBe(DatabaseCheckerHttpApiConstants.Groups.Assertions);

        await WithUnitOfWorkAsync(async () =>
        {
            var provider = GetRequiredService<IDbContextProvider<DatabaseCheckerDbContext>>();
            var dbContext = await provider.GetDbContextAsync();
            dbContext.Model.FindEntityType(typeof(ComparisonRun)).ShouldNotBeNull();
        });
    }

    // Yeni projection ve derivability HTTP yuzeylerinin composition ile kesfedilip dogru named permission'larla korundugunu dogrular.
    [Fact]
    public void Composition_Module_Should_Expose_Protected_Projection_And_Derivability_Routes()
    {
        var controllerFeature = new ControllerFeature();
        GetRequiredService<ApplicationPartManager>().PopulateFeature(controllerFeature);
        controllerFeature.Controllers.ShouldContain(controller =>
            controller.AsType() == typeof(ProjectionController));

        var projectionRoute = typeof(ProjectionController)
            .GetCustomAttributes(typeof(RouteAttribute), false).Cast<RouteAttribute>().Single();
        projectionRoute.Template.ShouldBe(DatabaseCheckerHttpApiConstants.Routes.Projections);
        var projectionGroup = typeof(ProjectionController)
            .GetCustomAttributes(typeof(ApiExplorerSettingsAttribute), false)
            .Cast<ApiExplorerSettingsAttribute>().Single();
        projectionGroup.GroupName.ShouldBe(DatabaseCheckerHttpApiConstants.Groups.Projections);
        GetPolicy(typeof(ProjectionController)).ShouldBe(DatabaseCheckerPermissions.Projections.Execute);

        var derivabilityAction = typeof(AssertionController)
            .GetMethod(nameof(AssertionController.ValidateDerivability))!;
        GetPolicy(derivabilityAction).ShouldBe(DatabaseCheckerPermissions.Assertions.ValidateDerivability);
    }

    // Write-set controller rotalarini, named permission'lari ve iki mevcut motor repository kaydini dogrular.
    [Fact]
    public void Composition_Module_Should_Expose_Protected_WriteSet_Capabilities()
    {
        var controllerFeature = new ControllerFeature();
        GetRequiredService<ApplicationPartManager>().PopulateFeature(controllerFeature);
        controllerFeature.Controllers.ShouldContain(controller =>
            controller.AsType() == typeof(WriteSetCapabilityController));

        var route = typeof(WriteSetCapabilityController)
            .GetCustomAttributes(typeof(RouteAttribute), false).Cast<RouteAttribute>().Single();
        route.Template.ShouldBe(DatabaseCheckerHttpApiConstants.Routes.WriteSetCapabilities);
        GetPolicy(typeof(WriteSetCapabilityController).GetMethod(nameof(WriteSetCapabilityController.Probe))!)
            .ShouldBe(DatabaseCheckerPermissions.Capabilities.Probe);
        GetPolicy(typeof(WriteSetCapabilityController).GetMethod(nameof(WriteSetCapabilityController.Capture))!)
            .ShouldBe(DatabaseCheckerPermissions.Capabilities.Capture);
        GetPolicy(typeof(WriteSetCapabilityController).GetMethod(nameof(WriteSetCapabilityController.Release))!)
            .ShouldBe(DatabaseCheckerPermissions.Capabilities.Capture);

        var resolver = GetRequiredService<IEngineComponentResolver<IWriteSetRepository>>();
        resolver.Resolve(DatabaseEngineCodes.PostgreSql)
            .ShouldBeOfType<PostgreSqlDatabaseDataComparisonRepository>();
        resolver.Resolve(DatabaseEngineCodes.SqlServer)
            .ShouldBeOfType<SqlServerDatabaseDataComparisonRepository>();
    }

    // Controller veya action uzerindeki tek named authorization politikasini smoke assertion'ina tasir.
    private static string? GetPolicy(System.Reflection.MemberInfo member)
        => member.GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .Cast<AuthorizeAttribute>()
            .Single()
            .Policy;
}
