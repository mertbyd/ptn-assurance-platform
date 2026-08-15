using System;
using System.Linq;
using Ptn.TestModule.Models.Bridge;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Bridge;

// islevi: Birlestirilmis konum ve rapor tiplerinde API ve DB sema anlamlarini yeniden birlestiren alan adini tarar.
// sistemdeki gorevi: SchemaName cakismasinin ajana giden yuzeye yeni bir alanla sessizce geri gelmesini engeller.
public class BridgeVocabularyTests
{
    // ADR-0018 §I: yasak yalniz birlestirilmis, ajana giden konum ve onu gomen rapor yuzeyinde gecerlidir.
    // Checker tarafi kaynak modelleri ve tek yonlu DB modelleri tek anlam tasidigi icin bilincli olarak kapsam disidir.
    private static readonly Type[] AgentFacingTypes =
    [
        typeof(Location),
        typeof(DiagnosisReport),
        typeof(DiagnosisHypothesis),
        typeof(Evidence),
        typeof(OperationLinkResult),
        typeof(OperationLinkCandidate),
        typeof(TableDescription),
        typeof(ForeignKeyNeighbor),
        typeof(SchemaLintWarning),
        typeof(ResponseObservation)
    ];

    // Kapsam icindeki konum ve rapor tiplerinde yasakli SchemaName adini arar.
    [Fact]
    public void Should_not_expose_ambiguous_schema_name_property_on_agent_facing_surface()
    {
        var ambiguousProperties = AgentFacingTypes
            .SelectMany(type => type.GetProperties())
            .Where(property => property.Name == "SchemaName")
            .Select(property => $"{property.DeclaringType?.Name}.{property.Name}")
            .ToList();

        ambiguousProperties.ShouldBeEmpty();
    }
}
