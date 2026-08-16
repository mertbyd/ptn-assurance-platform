namespace Ptn.TestModule.Constants.Bridge;

// islevi: Kopru profil paketi konumunun kararli ayar adini ve varsayilan yolunu tanimlar.
// sistemdeki gorevi: Domain, provider ve host katmanlarinin ayni Domain.Shared ayar sozlesmesini kullanmasini saglar.
public static class PtnBridgeSettingNames
{
    public const string ProfilePackPath = "TestModule.Bridge.ProfilePackPath";
    public const string DefaultProfilePackPath = "Authoring/profiles";
    public const string ProfilePackKey = "TestModule.Bridge.ProfilePackKey";
    public const string DefaultProfilePackKey = "acme-ticketing";
    public const string ProfilePackExtension = ".yaml";
    public const string BusinessRulesPath = "TestModule.Bridge.BusinessRulesPath";
    public const string DefaultBusinessRulesPath = "Authoring";
    public const string BusinessRulesFileName = "kurallar.md";
    public const string AgentPolicyPath = "TestModule.Bridge.AgentPolicyPath";
    public const string DefaultAgentPolicyPath = "Authoring";
    public const string AgentPolicyFileName = "agent-policy.md";
    public const string FingerprintPrefix = "sha256:";
    public const string AgentProfilePrefix = "TestModule.Bridge.AgentProfile";

    public static string AllowedTools(string momentCode) => $"{AgentProfilePrefix}.{momentCode}.AllowedTools";
    public static string MaxTurns(string momentCode) => $"{AgentProfilePrefix}.{momentCode}.MaxTurns";
    public static string TokenLimit(string momentCode) => $"{AgentProfilePrefix}.{momentCode}.TokenLimit";

    public static string DefaultAllowedTools(string momentCode) => momentCode switch
    {
        AgentMomentCodes.Grounding => string.Join(',', PtnToolCodes.Ground, PtnToolCodes.Knowledge, PtnToolCodes.Profile),
        AgentMomentCodes.Drafting => string.Join(',', PtnToolCodes.Ground, PtnToolCodes.Knowledge, PtnToolCodes.Impact),
        AgentMomentCodes.Validation => string.Join(',', PtnToolCodes.Validate, PtnToolCodes.Impact),
        AgentMomentCodes.Approval => string.Join(',', PtnToolCodes.Validate, PtnToolCodes.Task),
        AgentMomentCodes.Execution => string.Join(',', PtnToolCodes.Run, PtnToolCodes.Result, PtnToolCodes.Task),
        AgentMomentCodes.Diagnosis => string.Join(',', PtnToolCodes.Result, PtnToolCodes.Explain, PtnToolCodes.DryRun),
        _ => string.Empty
    };
}
