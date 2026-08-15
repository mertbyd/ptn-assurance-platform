namespace Ptn.TestModule.Dtos.Bridge;

// islevi: Cozumlenecek ajan anini public sozlesmede tasir.
// sistemdeki gorevi: Serbest moment kodunu validator kapisindan Manager'a aktarir.
public sealed class AgentProfileRequestDto { public string MomentCode { get; set; } = string.Empty; }
