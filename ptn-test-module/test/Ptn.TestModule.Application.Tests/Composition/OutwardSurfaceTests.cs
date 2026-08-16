using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Ptn.TestModule.Controllers.Runs;
using Ptn.TestModule.Services.Runs;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Application.Tests.Composition;

// islevi: Public Application.Contracts metotlarinin controller action karsiligini tarar.
// sistemdeki gorevi: Tuketici portu Bridge istisnasi disinda ulasilamaz AppService regresyonunu engeller.
public class OutwardSurfaceTests
{
    private const int ExpectedControllerActionCount = 64;
    private const string ServiceNamespace = "Ptn.TestModule.Services";
    private const string BridgeNamespace = "Ptn.TestModule.Services.Bridge";

    [Fact]
    public void Every_public_app_service_method_should_have_a_controller_action()
    {
        var actionNames = typeof(TestRunController).Assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.IsPublic)
            .Where(type => typeof(ControllerBase).IsAssignableFrom(type))
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(method => method.GetCustomAttributes().Any(attribute =>
                attribute.GetType().IsSubclassOf(typeof(HttpMethodAttribute))))
            .Select(method => method.Name + "Async")
            .ToHashSet(StringComparer.Ordinal);

        var unreachable = typeof(ITestRunAppService).Assembly.GetTypes()
            .Where(type => type.IsInterface && type.IsPublic)
            .Where(type => type.Namespace?.StartsWith(ServiceNamespace, StringComparison.Ordinal) == true)
            .Where(type => type.Namespace != BridgeNamespace || type.Name == "IPtnBridgeAppService")
            .SelectMany(type => type.GetMethods())
            .Where(method => !actionNames.Contains(method.Name))
            .Select(method => $"{method.DeclaringType!.FullName}.{method.Name}")
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

        unreachable.ShouldBeEmpty();
    }

    [Fact]
    public void Every_controller_action_should_have_http_and_swagger_metadata()
    {
        var controllers = typeof(TestRunController).Assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.IsPublic)
            .Where(type => typeof(ControllerBase).IsAssignableFrom(type))
            .ToArray();

        var withoutSwaggerGroup = controllers
            .Where(type => string.IsNullOrWhiteSpace(type.GetCustomAttribute<ApiExplorerSettingsAttribute>()?.GroupName))
            .Select(type => type.FullName)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

        var withoutHttpMethod = controllers
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(method => !method.GetCustomAttributes().Any(attribute =>
                attribute.GetType().IsSubclassOf(typeof(HttpMethodAttribute))))
            .Select(method => $"{method.DeclaringType!.FullName}.{method.Name}")
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

        withoutSwaggerGroup.ShouldBeEmpty();
        withoutHttpMethod.ShouldBeEmpty();
    }

    // KBP-111 authoring yuzeyindeki dort action'in sessizce kaybolmasini sayisal kontratla engeller.
    [Fact]
    public void Controller_action_count_should_match_the_authoring_contract()
    {
        var actionCount = typeof(TestRunController).Assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.IsPublic)
            .Where(type => typeof(ControllerBase).IsAssignableFrom(type))
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Count(method => method.GetCustomAttributes().Any(attribute =>
                attribute.GetType().IsSubclassOf(typeof(HttpMethodAttribute))));

        actionCount.ShouldBe(ExpectedControllerActionCount);
    }
}
