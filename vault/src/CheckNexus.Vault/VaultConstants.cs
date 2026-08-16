namespace CheckNexus.Vault;

// islevi: Vault HTTP ve KV v2 wire sozlesmesindeki kararlı adlari tek yerde tutar.
// sistemdeki gorevi: Provider icinde configuration/header/payload stringlerinin dagilmasini engeller.
internal static class VaultConstants
{
    public const string HttpClientName = "CheckNexus.Vault";
    public const string TokenHeaderName = "X-Vault-Token";
    public const string NamespaceHeaderName = "X-Vault-Namespace";
    public const string DataPropertyName = "data";
    public const string HeaderNamePropertyName = "headerName";
    public const string HeaderValuePropertyName = "headerValue";
    public const string UsernamePropertyName = "username";
    public const string PasswordPropertyName = "password";
}
