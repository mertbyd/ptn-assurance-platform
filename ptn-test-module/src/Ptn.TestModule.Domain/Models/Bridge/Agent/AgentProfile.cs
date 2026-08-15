using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge.Agent;

// islevi: Tek ajan ani icin izinli tool, tur ve token tavanini tasir.
// sistemdeki gorevi: ABP Setting degerlerini tipli ve tenant-aware domain kararina cevirir.
public class AgentProfile
{
    public string MomentCode { get; set; } = string.Empty;
    public List<string> AllowedToolCodes { get; set; } = [];
    public int MaxTurns { get; set; }
    public int TokenLimit { get; set; }
}
