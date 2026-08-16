using System.Linq;
using System.Text.Json;
using Ptn.DatabaseChecker.Application.Mappers.Diagnosis;
using Ptn.DatabaseChecker.Constants.Diagnosis;
using Ptn.DatabaseChecker.Models.Diagnosis;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.Mappers;

// islevi: Mapperly diagnosis raporunun gercek RFC 9457 JSON alan adlariyla 4 KB transport tavanini korudugunu dogrular.
// sistemdeki gorevi: Domain emniyet payinin checknexus extension alan adlari sonrasinda da MCP govdesini sinirda tuttugunun kontrat kanitidir.
public class DiagnosisMapper_Tests
{
    // islevi: Mapperly DTO ve checknexus JSON adlari eklendiginde transport govdesinin 4 KB altinda kaldigini dogrular.
    [Fact]
    public void Mapped_Transport_Body_Should_Remain_Under_Four_Kilobytes()
    {
        var report = new DiagnosisReport
        {
            Detail = new string('D', 2000),
            Hypotheses = Enumerable.Range(0, 10).Select(index =>
                new HypothesisAssessment($"H{index}", index, DiagnosisConfidenceCodes.Possible, new()
                {
                    new ProbeEvidence { ObservedValue = new string('V', 1200) }
                })
                {
                    Title = new string('T', 200),
                    Detail = new string('X', 1000),
                    NextChecks = new() { new string('N', 400) }
                }).ToList()
        };
        report.TrimToBudget();

        var dto = new DiagnosisMapper().MapToDto(report);
        var body = JsonSerializer.SerializeToUtf8Bytes(dto);

        body.Length.ShouldBeLessThanOrEqualTo(FailureSourceKindCodes.Report.MaxUtf8Bytes);
    }
}
