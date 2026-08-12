namespace CheckNexus.Vault;

// islevi: Composition adapter'inin Vault'a nasil kimlik sunacagini tanimlar.
// sistemdeki gorevi: Local sinirli token ile production Vault Agent/Proxy akisini ayni typed options sozlesmesinde ayirir.
public enum VaultAuthenticationMode
{
    Token = 1,
    AgentProxy = 2
}
