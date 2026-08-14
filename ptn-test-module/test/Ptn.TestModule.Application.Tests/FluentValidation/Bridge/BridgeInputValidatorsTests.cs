using System;
using FluentValidation;
using Ptn.TestModule.Dtos.Bridge.Api;
using Ptn.TestModule.Dtos.Bridge.Database;
using Ptn.TestModule.Dtos.Bridge.Diagnosis;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.FluentValidation.Bridge.Api;
using Ptn.TestModule.FluentValidation.Bridge.Database;
using Ptn.TestModule.FluentValidation.Bridge.Diagnosis;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Application.Tests.FluentValidation.Bridge;

// islevi: Bridge public girdi DTO'larinin bos tasima sekillerini reddettigini dogrular.
// sistemdeki gorevi: Her public input icin FluentValidation sahibi bulunmasini regresyon kapisina baglar.
public class BridgeInputValidatorsTests
{
    // Tum Bridge input validator'larinin bos kontrati checker cagrisindan once reddettigini kanitlar.
    [Fact]
    public void Should_reject_empty_bridge_inputs()
    {
        new OperationQueryDtoValidator().Validate(new OperationQueryDto()).IsValid.ShouldBeFalse();
        new DerivabilityRequestDtoValidator().Validate(new DerivabilityRequestDto()).IsValid.ShouldBeFalse();
        new ResponseObservationDtoValidator().Validate(new ResponseObservationDto()).IsValid.ShouldBeFalse();
        new DatabaseAssertionRequestDtoValidator().Validate(new DatabaseAssertionRequestDto()).IsValid.ShouldBeFalse();
        new DatabaseAssertionBatchRequestDtoValidator().Validate(new DatabaseAssertionBatchRequestDto()).IsValid.ShouldBeFalse();
        new ProjectionRequestDtoValidator().Validate(new ProjectionRequestDto()).IsValid.ShouldBeFalse();
        new TableQueryDtoValidator().Validate(new TableQueryDto()).IsValid.ShouldBeFalse();
        new DiagnosisRequestDtoValidator().Validate(new DiagnosisRequestDto { Location = null! }).IsValid.ShouldBeFalse();
    }

    // Nested public input'larin parent validator zincirinden kacamadigini kanitlar.
    [Fact]
    public void Should_reject_invalid_nested_bridge_inputs()
    {
        var assertion = new DatabaseAssertionRequestDto
        {
            ConnectionId = Guid.NewGuid(),
            SchemaName = "public",
            TableName = "users",
            TimeoutMs = 1000,
            PollIntervalMs = 100,
            Expectations = [new ColumnExpectationDto()],
            Cardinality = new DatabaseCardinalityExpectationDto()
        };

        new DatabaseAssertionRequestDtoValidator().Validate(assertion).IsValid.ShouldBeFalse();
    }

    // Ayni diagnosis DTO'sunun API ve database cagrisinda farkli zorunlu kimligi tasidigini kanitlar.
    [Fact]
    public void Should_apply_source_specific_diagnosis_rules()
    {
        var validator = new DiagnosisRequestDtoValidator();
        var input = new DiagnosisRequestDto { Location = new LocationDto { Path = "/users" } };

        validator.Validate(input, options => options.IncludeRuleSets(PtnBridgeValidationRuleSets.Api))
            .IsValid.ShouldBeFalse();
        validator.Validate(input, options => options.IncludeRuleSets(PtnBridgeValidationRuleSets.Database))
            .IsValid.ShouldBeFalse();
    }
}
