using System.Text.Json;
using Ptn.ApiContractChecker.Dtos.Diagnosis;
using Shouldly;
using Xunit;

namespace Ptn.ApiContractChecker.Diagnosis;

// islevi: Diagnosis raporunun RFC ve checknexus uzanti anahtarlarini birebir tel kumesiyle dogrular.
// sistemdeki gorevi: API ve Database checker raporlarinin farkli JSON adlariyla yayinlanmasini engeller.
public class DiagnosisReportWire_Tests
{
    [Fact]
    public void Serialized_Report_Should_Expose_The_Aligned_Wire_Key_Set()
    {
        var keys = JsonSerializer.SerializeToElement(new DiagnosisReportDto())
            .EnumerateObject()
            .Select(property => property.Name);

        keys.ShouldBe(
        [
            "type",
            "title",
            "status",
            "detail",
            "instance",
            "checknexus:identity",
            "checknexus:location",
            "checknexus:hypotheses",
            "checknexus:nextChecks",
            "checknexus:correlation"
        ], ignoreOrder: true);
    }
}
