using System;
using System.Linq;
using Ptn.TestModule.Dtos.Catalog;
using Ptn.TestModule.Services.Catalog;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Application.Tests.Services.Catalog;

// islevi: Yayin kanitinin hicbir public girdi sozlesmesinde tasinmadigini dogrular.
// sistemdeki gorevi: Istemcinin IsSchemaValid veya compiled_document iddia etmesini yapisal olarak imkansiz tutar (ADR-0015 §C).
public class ScenarioPublicationContractTests
{
    // Kanit alan adlari yalniz derleyiciden gelir; hicbir public girdi DTO'su bunlari tasiyamaz.
    private static readonly string[] EvidencePropertyNames =
    [
        "CompiledDocument",
        "CompiledHash",
        "AssertionCount",
        "IsSchemaValid",
        "AreAssertionsDerivable",
        "SourceDescriptionSpecSnapshotIds",
        "SchemaLintWarnings"
    ];

    // Senaryo olusturma girdisi yalniz kaynak belgeyi ve malzeme muhrunu kabul etmelidir.
    [Fact]
    public void Should_not_accept_publication_evidence_on_the_create_contract()
    {
        PropertyNamesOf<CreateTestScenarioDto>().ShouldNotContain(name => EvidencePropertyNames.Contains(name));
    }

    // Senaryo guncelleme girdisi de kanit alani tasimamalidir.
    [Fact]
    public void Should_not_accept_publication_evidence_on_the_update_contract()
    {
        PropertyNamesOf<UpdateTestScenarioDto>().ShouldNotContain(name => EvidencePropertyNames.Contains(name));
    }

    // Istemcinin kanit yollayabilecegi ayri bir yayin girdisi kalmamalidir.
    [Fact]
    public void Should_not_expose_a_publication_input_contract()
    {
        typeof(CreateTestScenarioDto).Assembly
            .GetType("Ptn.TestModule.Dtos.Catalog.PublishTestScenarioDto")
            .ShouldBeNull();
    }

    // Yayin ve degerlendirme uclari yalniz senaryo kimligi almalidir; kanit sunucuda uretilir.
    [Theory]
    [InlineData(nameof(ITestScenarioAppService.PublishAsync))]
    [InlineData(nameof(ITestScenarioAppService.EvaluatePublicationAsync))]
    public void Should_only_accept_the_scenario_identity_on_publication_endpoints(string methodName)
    {
        var parameters = typeof(ITestScenarioAppService).GetMethod(methodName)!.GetParameters();

        parameters.Length.ShouldBe(1);
        parameters[0].ParameterType.ShouldBe(typeof(Guid));
    }

    // Verilen sozlesmenin public property adlarini okur.
    private static string[] PropertyNamesOf<TContract>()
    {
        return [.. typeof(TContract).GetProperties().Select(property => property.Name)];
    }
}
