using System.Collections.Generic;
using Ptn.TestModule.Dtos.Bridge.Database;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Database;
using Riok.Mapperly.Abstractions;
using CheckerCardinalityDto = Ptn.DatabaseChecker.Dtos.Assertions.CardinalityExpectationDto;
using CheckerColumnExpectationDto = Ptn.DatabaseChecker.Dtos.Assertions.ColumnExpectationDto;
using CheckerFailedExpectationDto = Ptn.DatabaseChecker.Dtos.Assertions.FailedExpectationDto;
using CheckerRowAssertionRequestDto = Ptn.DatabaseChecker.Dtos.Assertions.RowAssertionRequestDto;
using CheckerRowAssertionResultDto = Ptn.DatabaseChecker.Dtos.Assertions.RowAssertionResultDto;

namespace Ptn.TestModule.Mappers.Bridge;

// islevi: Database checker assertion DTO'lariyla Bridge modelleri arasindaki compile-time eslemeleri uretir.
// sistemdeki gorevi: Application servisini property kopyalamadan checker cagrisi ve semantik kurallara indirger.
[Mapper]
public partial class DatabaseOracleMapper
{
    public partial PtnDatabaseAssertionRequest Map(DatabaseAssertionRequestDto input);
    public partial AssertionResultDto Map(PtnAssertionResult input);
    public partial PtnProjectionRequest Map(ProjectionRequestDto input);
    public partial ProjectionResultDto Map(PtnProjectionResult input);
    public partial List<PtnDatabaseAssertionRequest> Map(List<DatabaseAssertionRequestDto> input);
    public partial List<AssertionResultDto> Map(IReadOnlyList<PtnAssertionResult> input);
    public partial List<CheckerRowAssertionRequestDto> Map(IReadOnlyList<PtnDatabaseAssertionRequest> input);
    public partial IReadOnlyList<PtnAssertionResult> Map(List<CheckerRowAssertionResultDto> input);
    public partial CheckerRowAssertionRequestDto Map(PtnDatabaseAssertionRequest input);
    public partial CheckerCardinalityDto MapCardinality(PtnDatabaseCardinalityExpectation input);
    public partial CheckerColumnExpectationDto MapExpectation(PtnColumnExpectation input);
    public partial PtnAssertionResult Map(CheckerRowAssertionResultDto input);
    public partial PtnFailedExpectation MapFailure(CheckerFailedExpectationDto input);
}
