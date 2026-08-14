namespace Ptn.TestModule.Constants.Bridge;

// islevi: PostgreSQL yazma kumesi capability ve temporary slot sorgularini tek sahipte tanimlar.
// sistemdeki gorevi: Provider sorgu tokenlarinin Application servisinde daginik sabit metinlere donusmesini engeller.
public static class PtnWriteSetSql
{
    public const string LogicalWalLevel = "logical";
    public const string SlotNameParameter = "slotName";
    public const string ShowWalLevel = "SHOW wal_level";
    public const string CanReplicate = "SELECT rolreplication OR rolsuper FROM pg_roles WHERE rolname = current_user";
    public const string CreateTemporarySlot = "SELECT slot_name FROM pg_create_logical_replication_slot(@slotName, 'test_decoding', true)";
    public const string ReadChanges = "SELECT data FROM pg_logical_slot_get_changes(@slotName, NULL, NULL)";
    public const string DropSlot = "SELECT pg_drop_replication_slot(@slotName) WHERE EXISTS (SELECT 1 FROM pg_replication_slots WHERE slot_name = @slotName)";
}
