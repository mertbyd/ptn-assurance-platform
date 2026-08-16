namespace Ptn.DatabaseChecker.Constants.Capabilities;

// islevi: Capability dusurme ve slot temizleme gerekcelerinin kapali kod kumesini tanimlar.
// sistemdeki gorevi: Provider veya ortam yetersizligini exception ve ham hata metni sizdirmadan istemciye tasir.
public static class CapabilityReasonCodes
{
    public const string SharedEnvironment = "SharedEnvironment";
    public const string WalLevelNotLogical = "WalLevelNotLogical";
    public const string NoReplicationGrant = "NoReplicationGrant";
    public const string EngineNotSupported = "EngineNotSupported";
    public const string NoCapability = "NoCapability";
    public const string SlotReleaseFailed = "SlotReleaseFailed";

    public static IReadOnlyCollection<string> All { get; } =
    [
        SharedEnvironment,
        WalLevelNotLogical,
        NoReplicationGrant,
        EngineNotSupported,
        NoCapability,
        SlotReleaseFailed
    ];
}
