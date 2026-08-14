using Ptn.DatabaseChecker.Dtos.Assertions;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Database;
using Riok.Mapperly.Abstractions;

namespace Ptn.TestModule.Mappers.Bridge;

// islevi: Database checker assertion DTO'lariyla Bridge modelleri arasindaki compile-time eslemeleri uretir.
// sistemdeki gorevi: Application servisini property kopyalamadan checker cagrisi ve semantik kurallara indirger.
[Mapper]
public partial class DatabaseOracleMapper
{
    public partial RowAssertionRequestDto Map(PtnDatabaseAssertionRequest input);
    private partial CardinalityExpectationDto MapCardinality(PtnDatabaseCardinalityExpectation input);
    private partial ColumnExpectationDto MapExpectation(PtnColumnExpectation input);
    public partial PtnAssertionResult Map(RowAssertionResultDto input);
    private partial PtnFailedExpectation MapFailure(FailedExpectationDto input);
}
