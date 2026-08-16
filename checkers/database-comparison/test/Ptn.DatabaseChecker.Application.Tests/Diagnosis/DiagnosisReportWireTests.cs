using System.Linq;
using System.Text.Json;
using Ptn.DatabaseChecker.Dtos.Correlation;
using Ptn.DatabaseChecker.Dtos.Diagnosis;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.Diagnosis;

// islevi: Diagnosis report'un RFC ve checknexus top-level JSON anahtarlarini kararli tel sozlesmesine karsi dogrular.
// sistemdeki gorevi: Echo correlation alaninin serializer politikasindan bagimsiz checknexus uzantisi olarak yayinlanmasini korur.
public sealed class DiagnosisReportWireTests
{
    // islevi: Serialize edilen raporun correlation dahil tam beklenen top-level anahtar kumesini tasidigini dogrular.
    [Fact]
    public void Serialized_Report_Should_Contain_The_Exact_Checknexus_Correlation_Key()
    {
        var report = new DiagnosisReportDto
        {
            Correlation = new CorrelationRefDto
            {
                TraceId = new string('a', 32),
                StepKey = "step-1"
            }
        };

        using var document = JsonSerializer.SerializeToDocument(report);
        document.RootElement.EnumerateObject()
            .Select(property => property.Name)
            .OrderBy(name => name)
            .ShouldBe([
                "checknexus:correlation",
                "checknexus:hypotheses",
                "checknexus:identity",
                "checknexus:location",
                "checknexus:nextChecks",
                "detail",
                "instance",
                "status",
                "title",
                "type"
            ]);
    }
}
