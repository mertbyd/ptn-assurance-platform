using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

    // Application assembly'sindeki somut servis tiplerini getirir.
    private static IReadOnlyList<Type> GetServiceTypes()
    {
        return typeof(TestScenarioAppService).Assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.IsPublic)
            .Where(type => type.Namespace?.StartsWith(ServiceNamespaceRoot, StringComparison.Ordinal) == true)
            .ToList();
    }
}
