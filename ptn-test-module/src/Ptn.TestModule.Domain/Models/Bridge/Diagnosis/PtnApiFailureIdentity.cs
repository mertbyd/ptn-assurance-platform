using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: API diagnosis identity'sinden kanit yolunda kullanilan olgu alanlarini tasir.
// sistemdeki gorevi: Manager'in checker DTO'suna baglanmadan challenge ve status olgularini islemesini saglar.
public sealed class PtnApiFailureIdentity
{
    public int? StatusCode { get; set; }
    public List<string> ChallengeScopes { get; set; } = [];
    public List<string> AllowedMethods { get; set; } = [];
}
