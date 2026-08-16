using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Ptn.TestModule.Services.Bridge;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Application.Tests.Composition;

// islevi: Bridge AppService'inin her public metodunun CheckPolicyAsync ile basladigini kaynak taramasiyla dogrular.
// sistemdeki gorevi: MCP yuzeyinde izinsiz tokenlarin AppService katmanindan gecmesini kalici regresyon kapisina cevirir.
public class BridgeAuthorizationTests
{
    // PtnBridgeAppService kaynak dosyasindaki her public metot govdesinin CheckPolicyAsync ile baslamasi zorunludur.
    [Fact]
    public void Every_bridge_app_service_method_should_start_with_check_policy()
    {
        var serviceRoot = Path.Combine(
            FindModuleRoot().FullName,
            "src",
            "Ptn.TestModule.Application",
            "Services",
            "Bridge");
        var bridgeSource = File.ReadAllText(Path.Combine(serviceRoot, "PtnBridgeAppService.cs"));
        var publicMethods = typeof(PtnBridgeAppService)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(method => method.DeclaringType == typeof(PtnBridgeAppService))
            .Select(method => method.Name)
            .ToArray();

        publicMethods.ShouldNotBeEmpty();

        var methodBodyPattern = new Regex(
            @"public\s+(?:async\s+)?Task<[^>]+>\s+(?<name>\w+)\s*\([^)]*\)\s*\{(?<body>[^}]+)",
            RegexOptions.Singleline);
        var matches = methodBodyPattern.Matches(bridgeSource);
        var unchecked_ = matches
            .Cast<Match>()
            .Where(match => publicMethods.Contains(match.Groups["name"].Value))
            .Where(match =>
            {
                var body = match.Groups["body"].Value;
                var firstStatement = body
                    .Split('\n')
                    .Select(line => line.Trim())
                    .FirstOrDefault(line => !string.IsNullOrWhiteSpace(line));
                return firstStatement is null ||
                       !firstStatement.StartsWith("await CheckPolicyAsync(", StringComparison.Ordinal);
            })
            .Select(match => match.Groups["name"].Value)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        unchecked_.ShouldBeEmpty(
            $"Bridge AppService methods without CheckPolicyAsync as first statement: {string.Join(", ", unchecked_)}");
    }

    // PtnBridgeAppService'in public metot sayisi Controller'daki [Authorize] attribute sayisiyla uyusmalidir.
    [Fact]
    public void Bridge_app_service_method_count_should_match_controller_authorize_count()
    {
        var serviceMethodCount = typeof(PtnBridgeAppService)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Count(method => method.DeclaringType == typeof(PtnBridgeAppService));

        var controllerAuthorizeCount = typeof(Ptn.TestModule.Controllers.Bridge.PtnBridgeController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Count(method => method.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false).Any());

        serviceMethodCount.ShouldBe(controllerAuthorizeCount);
    }

    // Test assembly konumundan Test Module cozum kokunu bulur.
    private static DirectoryInfo FindModuleRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Ptn.TestModule.slnx")))
        {
            directory = directory.Parent;
        }

        return directory ?? throw new DirectoryNotFoundException("Ptn.TestModule.slnx");
    }
}
