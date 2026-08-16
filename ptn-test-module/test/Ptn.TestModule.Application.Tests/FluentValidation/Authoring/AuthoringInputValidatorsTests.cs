using System;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.Dtos.Authoring;
using Ptn.TestModule.Dtos.Bridge;
using Ptn.TestModule.ExceptionCodes.Catalog;
using Ptn.TestModule.FluentValidation.Authoring;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.FluentValidation.Authoring;

// islevi: Authoring create, answer ve tek-adim public girdilerinin kodlu retlerini dogrular.
// sistemdeki gorevi: Serbest cevap, ilgisiz snapshot URL'si ve bozuk model JSON'unu sinirda durdurur.
public class AuthoringInputValidatorsTests
{
    // Snapshot'i icermeyen API kaynagi ve bos workflow alanlarini kararli kodlarla reddeder.
    [Fact]
    public void Should_reject_invalid_create_shape()
    {
        var input = ValidCreate();
        input.WorkflowId = "Not Valid";
        input.ApiSourceUrl = "./snapshots/another.openapi.yaml";

        var result = new CreateAuthoringSessionDtoValidator().Validate(input);

        result.Errors.ShouldContain(error =>
            error.ErrorCode == TestModuleScenarioErrorCodes.Validation.AuthoringWorkflowIdInvalid);
        result.Errors.ShouldContain(error =>
            error.ErrorCode == TestModuleScenarioErrorCodes.Validation.AuthoringSourceUrlInvalid);
    }

    // JSON runtime'inda null gelebilen nested grounding ve assertion listesini exception yerine kodla reddeder.
    [Fact]
    public void Should_reject_null_nested_authoring_inputs()
    {
        var create = ValidCreate();
        create.Grounding = null!;
        var step = new AddAuthoringStepDto
        {
            StepId = "create-ticket",
            OperationReferenceId = Guid.NewGuid(),
            AssertionPaths = null!
        };

        var createResult = new CreateAuthoringSessionDtoValidator().Validate(create);
        var stepResult = new AddAuthoringStepDtoValidator().Validate(step);

        createResult.Errors.ShouldContain(error =>
            error.ErrorCode == TestModuleScenarioErrorCodes.Validation.AuthoringGroundingRequired);
        stepResult.Errors.ShouldContain(error =>
            error.ErrorCode == TestModuleScenarioErrorCodes.Validation.AuthoringAssertionPathInvalid);
    }

    // Bos kapali cevap ve gecersiz step body/pointer sekillerini ayri kodlarla reddeder.
    [Fact]
    public void Should_reject_invalid_answer_and_step_shapes()
    {
        var answer = new AnswerAuthoringSessionDtoValidator().Validate(new AnswerAuthoringSessionDto());
        var step = new AddAuthoringStepDtoValidator().Validate(new AddAuthoringStepDto
        {
            StepId = "Not Valid",
            RequestBodyJson = "not-json",
            AssertionPaths = ["id"]
        });

        answer.Errors.ShouldContain(error =>
            error.ErrorCode == TestModuleScenarioErrorCodes.Validation.AuthoringQuestionCodeRequired);
        answer.Errors.ShouldContain(error =>
            error.ErrorCode == TestModuleScenarioErrorCodes.Validation.AuthoringSelectedOptionRequired);
        step.Errors.ShouldContain(error =>
            error.ErrorCode == TestModuleScenarioErrorCodes.Validation.AuthoringStepIdInvalid);
        step.Errors.ShouldContain(error =>
            error.ErrorCode == TestModuleScenarioErrorCodes.Validation.AuthoringOperationReferenceRequired);
        step.Errors.ShouldContain(error =>
            error.ErrorCode == TestModuleScenarioErrorCodes.Validation.AuthoringAssertionPathInvalid);
        step.Errors.ShouldContain(error =>
            error.ErrorCode == TestModuleScenarioErrorCodes.Validation.AuthoringRequestBodyInvalid);
    }

    // Grounding ve mekanik step butcelerine uyan public girdileri kabul eder.
    [Fact]
    public void Should_accept_valid_authoring_inputs()
    {
        var create = ValidCreate();
        var answer = new AnswerAuthoringSessionDto
        {
            QuestionCode = "OperationReferenceRequired",
            SelectedOption = Guid.NewGuid().ToString("D")
        };
        var step = new AddAuthoringStepDto
        {
            StepId = "create-ticket",
            OperationReferenceId = Guid.NewGuid(),
            RequestBodyJson = "{\"title\":\"printer\"}",
            AssertionPaths = ["/id"]
        };

        new CreateAuthoringSessionDtoValidator().Validate(create).IsValid.ShouldBeTrue();
        new AnswerAuthoringSessionDtoValidator().Validate(answer).IsValid.ShouldBeTrue();
        new AddAuthoringStepDtoValidator().Validate(step).IsValid.ShouldBeTrue();
    }

    // Kapali kume disindaki matcher kodunu reddederek ajanin serbest matcher yazmasini engeller (RULE-0007).
    [Fact]
    public void Should_reject_a_database_step_outside_the_closed_matcher_set()
    {
        var step = ValidDatabaseStep();
        step.Expectations[0].MatcherCode = "StartsWith";

        var result = new AddDatabaseAuthoringStepDtoValidator().Validate(step);

        result.Errors.ShouldContain(error =>
            error.ErrorCode == TestModuleScenarioErrorCodes.Validation.AuthoringMatcherCodeInvalid);
    }

    // Bos tablo referansi ve bos kolon adi ayri kararli kodlarla reddedilir.
    [Fact]
    public void Should_reject_an_unbound_database_step()
    {
        var step = ValidDatabaseStep();
        step.TableReferenceId = Guid.Empty;
        step.Expectations[0].ColumnName = string.Empty;

        var result = new AddDatabaseAuthoringStepDtoValidator().Validate(step);

        result.Errors.ShouldContain(error =>
            error.ErrorCode == TestModuleScenarioErrorCodes.Validation.AuthoringTableReferenceInvalid);
        result.Errors.ShouldContain(error =>
            error.ErrorCode == TestModuleScenarioErrorCodes.Validation.AuthoringColumnNameInvalid);
    }

    // Kapali kumeden secilmis gecerli veritabani adimini kabul eder.
    [Fact]
    public void Should_accept_a_grounded_database_step()
    {
        new AddDatabaseAuthoringStepDtoValidator().Validate(ValidDatabaseStep()).IsValid.ShouldBeTrue();
    }

    // Kapali kumeden secilmis tek satir beklentisi tasiyan gecerli veritabani adimini kurar.
    private static AddDatabaseAuthoringStepDto ValidDatabaseStep() => new()
    {
        StepId = "verify-ticket-row",
        TableReferenceId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
        OperationCode = "assertRow",
        Expectations =
        [
            new AuthoringExpectationDto
            {
                ColumnName = "status",
                MatcherCode = PtnDatabaseMatcherCodes.Equals,
                Value = "Open"
            }
        ]
    };

    // Validator testleri icin gercek grounding zorunluluklarini tasiyan gecerli create girdisini kurar.
    private static CreateAuthoringSessionDto ValidCreate()
    {
        var snapshotId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        return new CreateAuthoringSessionDto
        {
            Grounding = new GroundRequestDto
            {
                ProfileKey = "default",
                SpecSnapshotId = snapshotId,
                ConnectionId = Guid.NewGuid(),
                StepIntent = "create ticket",
                ResponseFormat = PtnResponseFormatCodes.Detailed
            },
            WorkflowId = "ticket-lifecycle",
            WorkflowSummary = "Create one support ticket",
            ApiSourceUrl = $"./snapshots/{snapshotId:D}.openapi.yaml",
            DatabaseSourceUrl = "./database-checker.openapi.yaml"
        };
    }
}
