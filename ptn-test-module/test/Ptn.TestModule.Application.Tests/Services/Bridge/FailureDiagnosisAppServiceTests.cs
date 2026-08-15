using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Ptn.TestModule.Services.Bridge;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.Dtos.Bridge.Diagnosis;
using Ptn.TestModule.FluentValidation.Bridge.Diagnosis;
using Ptn.TestModule.Managers.Bridge;
using Shouldly;
using Xunit;
using ApiDiagnoseRequestDto = Ptn.ApiContractChecker.Dtos.Diagnosis.DiagnoseRequestDto;
using ApiDiagnosisReportDto = Ptn.ApiContractChecker.Dtos.Diagnosis.DiagnosisReportDto;
using ApiDiagnosisService = Ptn.ApiContractChecker.Services.Diagnosis.IDiagnosisAppService;
using ApiHypothesisCodes = Ptn.ApiContractChecker.Constants.Diagnosis.HypothesisKindCodes;
using ApiProbeKindCodes = Ptn.ApiContractChecker.Constants.Diagnosis.ProbeKindCodes;
using DatabaseDiagnoseRequestDto = Ptn.DatabaseChecker.Dtos.Diagnosis.DiagnoseRequestDto;
using DatabaseDiagnosisReportDto = Ptn.DatabaseChecker.Dtos.Diagnosis.DiagnosisReportDto;
using DatabaseDiagnosisService = Ptn.DatabaseChecker.Services.Diagnosis.IDiagnosisAppService;
using DatabaseHypothesisCodes = Ptn.DatabaseChecker.Constants.Diagnosis.HypothesisKindCodes;
using DatabaseProbeKindCodes = Ptn.DatabaseChecker.Constants.Diagnosis.ProbeKindCodes;

namespace Ptn.TestModule.Application.Tests.Services.Bridge;

// islevi: Teshis adapter'inin schema anlamlarini, hipotez gramerini ve kaynakli fingerprint'leri dogrular.
// sistemdeki gorevi: Iki checker raporunun ham kod veya ciplak fingerprint ile birlesmesini engeller.
public class FailureDiagnosisAppServiceTests
{
    // API SchemaName alanini ApiSchemaName'e ve H-AU-03 kodunu tek kopru hipotezine cevirir.
    [Fact]
    public async Task Should_normalize_api_diagnosis_and_preserve_source_identity()
    {
        var api = Substitute.For<ApiDiagnosisService>();
        api.DiagnoseAsync(Arg.Any<ApiDiagnoseRequestDto>()).Returns(CreateApiReport());
        var diagnosisService = new FailureDiagnosisAppService(
            api,
            Substitute.For<DatabaseDiagnosisService>(),
            new FailureDiagnosisManager(),
            new DiagnosisRequestDtoValidator());

        var result = await diagnosisService.DiagnoseApiAsync(
            new DiagnosisRequestDto
            {
                SpecSnapshotId = System.Guid.NewGuid(),
                Location = new LocationDto { Path = "/users" }
            },
            CancellationToken.None);

        result.Location.ApiSchemaName.ShouldBe("ProblemDetails");
        result.Location.DbSchemaName.ShouldBeNull();
        result.Hypotheses[0].HypothesisKindCode.ShouldBe(PtnHypothesisCodes.InsufficientScope);
        result.Hypotheses[0].Evidence[0].FactCode.ShouldBe(PtnFactCodes.Match);
        result.Hypotheses[0].Ref.SourceCheckerCode.ShouldBe(PtnSourceCheckerCodes.ApiContract);
        result.Hypotheses[0].Ref.Fingerprint.ShouldStartWith("sha256:");
    }

    // Database SchemaName alanini DbSchemaName'e ve RowNeverCreated kodunu ortak kaynak hipotezine cevirir.
    [Fact]
    public async Task Should_normalize_database_diagnosis_without_api_schema_collision()
    {
        var database = Substitute.For<DatabaseDiagnosisService>();
        database.DiagnoseAsync(Arg.Any<DatabaseDiagnoseRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(CreateDatabaseReport());
        var diagnosisService = new FailureDiagnosisAppService(
            Substitute.For<ApiDiagnosisService>(),
            database,
            new FailureDiagnosisManager(),
            new DiagnosisRequestDtoValidator());

        var result = await diagnosisService.DiagnoseDatabaseAsync(
            new DiagnosisRequestDto
            {
                ConnectionId = System.Guid.NewGuid(),
                OutcomeCode = PtnOutcomeCodes.RowNotFound,
                Location = new LocationDto
                {
                    DbSchemaName = "identity",
                    DbTableName = "users"
                }
            },
            CancellationToken.None);

        result.Location.DbSchemaName.ShouldBe("identity");
        result.Location.ApiSchemaName.ShouldBeNull();
        result.Hypotheses[0].HypothesisKindCode.ShouldBe(PtnHypothesisCodes.ResourceNeverCreated);
        result.Hypotheses[0].Evidence[0].FactCode.ShouldBe(PtnFactCodes.Match);
        result.Hypotheses[0].Ref.SourceCheckerCode.ShouldBe(PtnSourceCheckerCodes.DatabaseComparison);
    }

    // API checker'in ham kod ve sema anlamlarini tasiyan en kucuk kanitli raporu olusturur.
    private static ApiDiagnosisReportDto CreateApiReport()
    {
        return new ApiDiagnosisReportDto
        {
            Location = new Ptn.ApiContractChecker.Dtos.Diagnosis.ObjectReferenceDto
            {
                SchemaName = "ProblemDetails"
            },
            Hypotheses =
            [
                new Ptn.ApiContractChecker.Dtos.Diagnosis.HypothesisAssessmentDto
                {
                    HypothesisKindCode = ApiHypothesisCodes.InsufficientScope,
                    ConfidenceCode = "Confirmed",
                    Evidence =
                    [
                        new Ptn.ApiContractChecker.Dtos.Diagnosis.ProbeEvidenceDto
                        {
                            ProbeKindCode = ApiProbeKindCodes.AuthMetadata,
                            FactCode = ApiProbeKindCodes.Facts.Match
                        }
                    ]
                }
            ]
        };
    }

    // Database checker'in ham kod ve sema anlamlarini tasiyan en kucuk kanitli raporu olusturur.
    private static DatabaseDiagnosisReportDto CreateDatabaseReport()
    {
        return new DatabaseDiagnosisReportDto
        {
            Location = new DatabaseDiagnosisReportDto.LocationDto
            {
                SchemaName = "identity",
                TableName = "users"
            },
            Hypotheses =
            [
                new DatabaseDiagnosisReportDto.HypothesisDto
                {
                    HypothesisKindCode = DatabaseHypothesisCodes.RowNeverCreated,
                    ConfidenceCode = "Confirmed",
                    Evidence =
                    [
                        new DatabaseDiagnosisReportDto.EvidenceDto
                        {
                            ProbeKindCode = DatabaseProbeKindCodes.RowExists,
                            FactCode = DatabaseProbeKindCodes.Facts.Matches
                        }
                    ]
                }
            ]
        };
    }
}
