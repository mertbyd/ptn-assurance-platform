using System.Collections.Generic;
using Ptn.TestModule.Models.Bridge;

namespace Ptn.TestModule.Models.Runs;

// islevi: Birincil kirmizi adim icin cagrilacak teshis yuzeyini tipli listelerle tasir.
// sistemdeki gorevi: Kaynak checker secimini Application servisinden OracleDispatchManager'a tasir.
public sealed class OracleDiagnosisPlan
{
    public IReadOnlyList<DiagnosisRequest> ApiRequests { get; set; } = [];

    public IReadOnlyList<DiagnosisRequest> DatabaseRequests { get; set; } = [];
}
