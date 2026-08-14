using System;

namespace Ptn.TestModule.Models.Bridge.Database;

// islevi: Database checker teshis isteginin assertion veya exception union kokunu tasir.
// sistemdeki gorevi: Manager'in sectigi union kolunu Mapperly ile checker DTO'suna tasir.
public sealed class PtnDatabaseDiagnosisRequest
{
    public Guid ConnectionId { get; set; }
    public PtnDatabaseAssertionSignal? Assertion { get; set; }
    public PtnDatabaseExceptionSignal? DbException { get; set; }
}
