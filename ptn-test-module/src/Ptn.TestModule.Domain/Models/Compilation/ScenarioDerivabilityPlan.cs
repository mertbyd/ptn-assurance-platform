using System.Collections.Generic;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Database;

namespace Ptn.TestModule.Models.Compilation;

// islevi: Derleme sonrasinda sorgulanacak API ve veritabani turetilebilirlik isteklerini tasir.
// sistemdeki gorevi: Checker cagri kararlarini Application sinirina veri olarak gecirir.
public sealed class ScenarioDerivabilityPlan
{
    public IReadOnlyList<DerivabilityRequest> ApiRequests { get; set; } = [];

    public IReadOnlyList<DatabaseDerivabilityRequest> DatabaseRequests { get; set; } = [];
}
