using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Constants.Capabilities;

// islevi: Yazma kumesi yoklama, temporary slot, capture butcesi ve girdi gramerini kararli sabitlerde toplar.
// sistemdeki gorevi: Manager, repository ve validator katmanlarinin ayni operasyonel sinirlari kullanmasini saglar.
public static class WriteSetConsts
{
    public const string SlotNamePrefix = "checknexus_ws_";
    public const string CompactCaptureRefFormat = "N";
    public const int CaptureTimeoutMilliseconds = 3000;
    public const int SlotReleaseTimeoutMilliseconds = 5000;
    public const int MaxCandidateTables = 100;
    public const string LogicalWalLevel = "logical";
    public const string TestDecodingPlugin = "test_decoding";
    public const string ShowWalLevelSql = "SHOW wal_level";
    public const string CanReplicateSql =
        "SELECT rolreplication OR rolsuper FROM pg_roles WHERE rolname = current_user";
    public const string CandidateTablePattern = @"^[^.\s]+\.[^.\s]+$";
    public const string TableChangePattern =
        @"^table (?<schema>[^.:\s]+)\.(?<table>[^:\s]+): (?<operation>INSERT|UPDATE|DELETE):(?<payload>.*)$";
    public const string ColumnChangePattern = @"(?<column>[A-Za-z_][A-Za-z0-9_$]*)\[[^\]]+\]:";

    public static IReadOnlyCollection<string> AuditColumnNames { get; } =
    [
        "CreationTime",
        "CreatorId",
        "LastModificationTime",
        "LastModifierId",
        "IsDeleted",
        "DeletionTime",
        "DeleterId",
        "ConcurrencyStamp",
        "ExtraProperties"
    ];
}
