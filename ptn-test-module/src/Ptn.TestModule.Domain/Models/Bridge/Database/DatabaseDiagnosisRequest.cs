using System;
using Ptn.TestModule.Models.Bridge;

namespace Ptn.TestModule.Models.Bridge.Database;

// islevi: Database checker teshis isteginin assertion veya exception union kokunu tasir.
// sistemdeki gorevi: Manager'in sectigi union kolunu Mapperly ile checker DTO'suna tasir.
public sealed class DatabaseDiagnosisRequest
{
    public Guid ConnectionId { get; set; }
    public DatabaseAssertionSignal? Assertion { get; set; }
    public DatabaseExceptionSignal? DbException { get; set; }
    public CorrelationRef? Correlation { get; set; }
}
