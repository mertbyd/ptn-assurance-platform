using System.Collections.Generic;
using Ptn.TestModule.Dtos.Bridge;
using Ptn.TestModule.Dtos.Bridge.Database;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Database;
using Riok.Mapperly.Abstractions;
using CheckerCardinalityDto = Ptn.DatabaseChecker.Dtos.Assertions.CardinalityExpectationDto;
using CheckerColumnExpectationDto = Ptn.DatabaseChecker.Dtos.Assertions.ColumnExpectationDto;
using CheckerFailedExpectationDto = Ptn.DatabaseChecker.Dtos.Assertions.FailedExpectationDto;
using CheckerRowAssertionRequestDto = Ptn.DatabaseChecker.Dtos.Assertions.RowAssertionRequestDto;
using CheckerRowAssertionResultDto = Ptn.DatabaseChecker.Dtos.Assertions.RowAssertionResultDto;
using CheckerCorrelationRefDto = Ptn.DatabaseChecker.Dtos.Correlation.CorrelationRefDto;
using CheckerProjectionRequestDto = Ptn.DatabaseChecker.Dtos.Projections.ProjectionRequestDto;
using CheckerProjectionResultDto = Ptn.DatabaseChecker.Dtos.Projections.ProjectionResultDto;
using CheckerProjectionRowDto = Ptn.DatabaseChecker.Dtos.Projections.ProjectionRowDto;
using CheckerDerivabilityRequestDto = Ptn.DatabaseChecker.Dtos.Assertions.DerivabilityRequestDto;
using CheckerDerivabilityResultDto = Ptn.DatabaseChecker.Dtos.Assertions.DerivabilityResultDto;
using CheckerDerivabilityItemDto = Ptn.DatabaseChecker.Dtos.Assertions.DerivabilityItemDto;
using CheckerDerivabilityAddressDto = Ptn.DatabaseChecker.Dtos.Assertions.DerivabilityAddressDto;

namespace Ptn.TestModule.Mappers.Bridge;

// islevi: Database checker assertion DTO'lariyla Bridge modelleri arasindaki compile-time eslemeleri uretir.
// sistemdeki gorevi: Application servisini property kopyalamadan checker cagrisi ve semantik kurallara indirger.
[Mapper]
public partial class DatabaseOracleMapper
{
    public partial DatabaseAssertionRequest Map(DatabaseAssertionRequestDto input);
    public partial AssertionResultDto Map(AssertionResult input);
    public partial ProjectionRequest Map(ProjectionRequestDto input);
    public partial ProjectionResultDto Map(ProjectionResult input);
    [MapProperty(nameof(ProjectionRequest.DbSchemaName), nameof(CheckerProjectionRequestDto.SchemaName))]
    public partial CheckerProjectionRequestDto Map(ProjectionRequest input);
    [MapProperty(nameof(CheckerProjectionResultDto.OutcomeCode), nameof(ProjectionResult.StateCode))]
    public partial ProjectionResult Map(CheckerProjectionResultDto input);
    public partial DatabaseDerivabilityRequest Map(DatabaseDerivabilityRequestDto input);
    public partial DatabaseDerivabilityRequestDto MapToDto(DatabaseDerivabilityRequest input);
    public partial DatabaseDerivabilityResultDto Map(DatabaseDerivabilityResult input);
    public partial DatabaseDerivabilityResult Map(DatabaseDerivabilityResultDto input);
    public partial CheckerDerivabilityRequestDto Map(DatabaseDerivabilityRequest input);
    [MapperIgnoreTarget(nameof(DatabaseDerivabilityResult.AllDerivable))]
    public partial DatabaseDerivabilityResult Map(CheckerDerivabilityResultDto input);
    public partial List<DatabaseAssertionRequest> Map(List<DatabaseAssertionRequestDto> input);
    public partial List<AssertionResultDto> Map(IReadOnlyList<AssertionResult> input);
    public partial List<CheckerRowAssertionRequestDto> Map(IReadOnlyList<DatabaseAssertionRequest> input);
    public partial IReadOnlyList<AssertionResult> Map(List<CheckerRowAssertionResultDto> input);
    public partial CheckerRowAssertionRequestDto Map(DatabaseAssertionRequest input);
    public partial CheckerCardinalityDto MapCardinality(DatabaseCardinalityExpectation input);
    public partial CheckerColumnExpectationDto MapExpectation(ColumnExpectation input);
    public partial AssertionResult Map(CheckerRowAssertionResultDto input);
    public partial FailedExpectation MapFailure(CheckerFailedExpectationDto input);
    private partial CorrelationRef Map(CheckerCorrelationRefDto input);
    private partial CheckerCorrelationRefDto Map(CorrelationRef input);
    private partial CorrelationRef Map(CorrelationRefDto input);
    private partial CorrelationRefDto MapToDto(CorrelationRef input);
    private partial Dictionary<string, string?> MapRow(CheckerProjectionRowDto input);
    private partial CheckerDerivabilityAddressDto Map(DatabaseDerivabilityAddress input);
    private partial DatabaseDerivabilityItem Map(CheckerDerivabilityItemDto input);
    private partial DatabaseDerivabilityItemDto Map(DatabaseDerivabilityItem input);
}
