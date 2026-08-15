using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Ptn.TestModule.Services.Catalog;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Application.Tests.Composition;

// islevi: Application servislerinin private is metodu tasimayan duz orkestrasyon seklini dogrular.
// sistemdeki gorevi: House profile servis sinirini reflection tabanli kalici regresyon kapisina cevirir.
public class ServiceShapeTests
{
    private const string ServiceNamespaceRoot = "Ptn.TestModule.Services";

    // Her somut Application servisi bildirilen private metotlardan tamamen arinmis olmalidir.
    [Fact]
    public void Application_services_should_not_declare_private_methods()
    {
        var privateMethods = GetServiceTypes()
            .SelectMany(type => type.GetMethods(
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            .Where(method => method.IsPrivate && !(method.IsFinal && method.IsVirtual))
            .Select(method => $"{method.DeclaringType!.FullName}.{method.Name}")
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        privateMethods.ShouldBeEmpty();
    }

    // Mapperly orneklerinin private static readonly alan olarak kalmasi is helper'i sayilmaz.
    [Fact]
    public void Mapperly_instance_fields_should_be_recognized_as_the_allowed_private_field_shape()
    {
        var mapperFields = GetServiceTypes()
            .SelectMany(type => type.GetFields(
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            .Where(field => field.FieldType.Name.EndsWith("Mapper", StringComparison.Ordinal))
            .ToArray();

        mapperFields.ShouldNotBeEmpty();
        mapperFields.ShouldAllBe(field => field.IsPrivate && field.IsStatic && field.IsInitOnly);
    }

    // Servis kaynaklarinda guard veya dogrudan throw ifadesinin yeniden yerlesmesini engeller.
    [Fact]
    public void Application_service_sources_should_not_contain_guards_or_throw_expressions()
    {
        var serviceRoot = Path.Combine(
            FindModuleRoot().FullName,
            "src",
            "Ptn.TestModule.Application",
            "Services");
        var offenders = Directory.GetFiles(serviceRoot, "*.cs", SearchOption.AllDirectories)
            .SelectMany(file => File.ReadAllLines(file)
                .Select((line, index) => new { File = file, Line = line, Number = index + 1 }))
            .Where(item => item.Line.Contains("ThrowIf", StringComparison.Ordinal) ||
                           Regex.IsMatch(item.Line, @"\bthrow\b"))
            .Select(item => $"{Path.GetRelativePath(serviceRoot, item.File)}:{item.Number}")
            .OrderBy(location => location, StringComparer.Ordinal)
            .ToArray();

        offenders.ShouldBeEmpty();
    }

    // Application assembly'sindeki somut servis tiplerini getirir.
    private static IReadOnlyList<Type> GetServiceTypes()
    {
        return typeof(TestScenarioAppService).Assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.IsPublic)
            .Where(type => type.Namespace?.StartsWith(ServiceNamespaceRoot, StringComparison.Ordinal) == true)
            .ToList();
    }

    // Test kosucusunun bin klasorunden solution kokune cikar.
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
