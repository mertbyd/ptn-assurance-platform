# CheckNexus Vault

HashiCorp Vault KV v2 composition adapter for the CheckNexus API Contracts and
Database Comparison modules.

## Install

```xml
<PackageReference Include="CheckNexus.Vault" Version="0.1.0-alpha.5" />
```

Add `CheckNexusVaultModule` to the executable ABP module graph. The module registers
one `VaultSecretProvider` singleton and explicitly exposes it through both checker
`ISecretProvider` contracts.

## Configuration

The adapter uses the `Vault` configuration section:

- `Address`: absolute Vault or Vault Agent/Proxy address.
- `Mount`: one KV v2 mount segment.
- `AuthenticationMode`: `Token` or `AgentProxy`.
- `Token`: local token injection; do not store it in appsettings.
- `TokenFile`: token file or container secret path.
- `Namespace`: optional Vault Enterprise/HCP namespace.
- `RequestTimeoutSeconds`: positive request timeout.

Use .NET user-secrets only for local development. Production deployments should use
workload identity with Vault Agent/Proxy or an environment-appropriate authentication
method, least-privilege policies, TLS, audit logging, and a documented unseal and
rotation procedure.

The adapter never owns checker entities or migrations. Checker packages retain their
secret-store ports, while the executable composition host owns provider selection and
lifecycle.

## License

MIT
