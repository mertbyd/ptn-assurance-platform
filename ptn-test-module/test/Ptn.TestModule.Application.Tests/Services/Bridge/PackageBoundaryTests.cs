using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Application.Tests.Services.Bridge;

// islevi: Application paket grafiginin checker Domain ve Npgsql sinirini statik olarak dogrular.
// sistemdeki gorevi: Checker sahipligi devrinden sonra yasak bagimliliklarin csproj'a geri eklenmesini engeller.
public class PackageBoundaryTests
{
    // Application projesinde yalniz iki checker Application.Contracts referansini kabul eder.
    [Fact]
    public void Application_project_should_not_reference_npgsql_or_checker_domain()
    {
        var moduleRoot = FindModuleRoot();
        var project = XDocument.Load(Path.Combine(
            moduleRoot.FullName,
            "src",
            "Ptn.TestModule.Application",
            "Ptn.TestModule.Application.csproj"));
        var packageIds = project.Descendants("PackageReference")
            .Select(element => (string?)element.Attribute("Include"))
            .Where(value => value is not null)
            .Cast<string>()
            .ToArray();

        packageIds.ShouldNotContain("Npgsql");
        packageIds.ShouldNotContain("CheckNexus.DatabaseComparison.Domain");
        packageIds.ShouldContain("CheckNexus.DatabaseComparison.Application.Contracts");
    }

    // Iki checker ailesinin ayri immutable surum degiskenleri kullandigini dogrular.
    [Fact]
    public void Checker_families_should_use_separate_version_properties()
    {
        var content = File.ReadAllText(Path.Combine(FindModuleRoot().FullName, "common.props"));

        content.ShouldContain("<CheckNexusApiContractsVersion>0.2.0-alpha.5</CheckNexusApiContractsVersion>");
        content.ShouldContain("<CheckNexusDatabaseComparisonVersion>0.2.0-alpha.8</CheckNexusDatabaseComparisonVersion>");
        content.ShouldNotContain("<CheckNexusVersion>");
    }

    // MCP SDK'sinin yalniz composition hostta kaldigini ve model istemci paketinin eklenmedigini dogrular.
    [Fact]
    public void Mcp_protocol_should_stay_in_the_host_without_a_model_client()
    {
        var moduleRoot = FindModuleRoot();
        var projects = Directory.GetFiles(moduleRoot.FullName, "*.csproj", SearchOption.AllDirectories);
        var mcpOwners = projects.Where(file => File.ReadAllText(file).Contains("ModelContextProtocol.AspNetCore", StringComparison.Ordinal)).ToArray();
        mcpOwners.Length.ShouldBe(1);
        mcpOwners[0].ShouldEndWith(Path.Combine("host", "Ptn.TestModule.HttpApi.Host", "Ptn.TestModule.HttpApi.Host.csproj"));

        var production = Directory.GetFiles(moduleRoot.FullName, "*.cs", SearchOption.AllDirectories)
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}test{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase));
        production.Select(File.ReadAllText).ShouldAllBe(content =>
            !content.Contains("IChatClient", StringComparison.Ordinal) &&
            !content.Contains("Gemini", StringComparison.Ordinal) &&
            !content.Contains("OpenAIClient", StringComparison.Ordinal));
    }

    // Test assembly konumundan cozum ve common.props sahibi modul kokunu bulur.
    private static DirectoryInfo FindModuleRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Ptn.TestModule.slnx")))
        {
            directory = directory.Parent;
        }

        return directory ?? throw new DirectoryNotFoundException("Ptn.TestModule.slnx bulunamadi.");
    }
}
