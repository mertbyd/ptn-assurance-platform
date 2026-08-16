# CheckNexus Database Comparison

Reusable ABP module for database connection registration, schema discovery,
comparison definitions, comparison runs, reports, provider adapters, and HTTP API
access.

## Install

Use the composition package when the complete in-process module is required:

```xml
<PackageReference Include="CheckNexus.DatabaseComparison" Version="0.2.0-alpha.7" />
```

The package targets .NET 10 and uses ABP Framework 10.3.0. It transitively includes
the Application, HttpApi, and EntityFrameworkCore packages. Layer-specific packages
remain available for consumers that intentionally need a narrower compile-time
surface.

`0.2.0-alpha.2` and `0.2.0-alpha.6` are public on NuGet.org and immutable. This
source tree is the **next** candidate, `0.2.0-alpha.7`, and is not published yet;
PackageValidation runs against `0.2.0-alpha.6` as its baseline.

## Composition contract

- Add `DatabaseCheckerModule` to the executable ABP module graph.
- Configure the `DatabaseChecker` connection string. When it is absent, ABP falls
  back to `Default`.
- Apply `DatabaseCheckerDbContext` migrations from the executable's migration
  workflow. The module owns only its lookup, connection, definition, and run tables
  plus its migration history table.
- Register an implementation of
  `Ptn.DatabaseChecker.Interface.Secrets.ISecretProvider`. The optional
  `CheckNexus.Vault` package provides the shared Vault KV v2 adapter.
- Supply authentication, user/tenant management, notification delivery, scheduling,
  and production hosting from the composition application.

The repository's `Ptn.DatabaseChecker.HttpApi.Host` project is a development and
verification executable. It is intentionally not included in any NuGet package.

## Test Module API surface

All endpoints return the host's `Result<T>` envelope. Request cancellation is
propagated through controllers, application services, managers, target probes, and
repositories.

| Method and route | Return | Required permission | Stable code sets | Typical response budget |
| --- | --- | --- | --- | --- |
| `POST capabilities/write-set/probe` | Four-level write-set capability | `DatabaseChecker.Capabilities.Probe` | `FootprintStrengthCodes`, `CapabilityReasonCodes` | Small fixed response |
| `POST capabilities/write-set/capture` | Advisory table/column/row-delta footprint | `DatabaseChecker.Capabilities.Capture` | `FootprintStrengthCodes`, `CapabilityReasonCodes` | 100 candidate tables maximum; 3 second capture window |
| `POST capabilities/write-set/release` | Advisory cleanup result | `DatabaseChecker.Capabilities.Capture` | `CapabilityReasonCodes` | Small fixed response |
| `POST api/comparison/assertions/row` | One row/value assertion result | `DatabaseChecker.Assertions.Execute` | `AssertionOutcomeCodes`, `MatcherKindCodes` | Usually under 8 KB |
| `POST api/comparison/assertions/count` | One cardinality assertion result | `DatabaseChecker.Assertions.Execute` | `AssertionOutcomeCodes`, `CardinalityKindCodes` | Usually under 8 KB |
| `POST api/comparison/assertions/absent` | One absence assertion result | `DatabaseChecker.Assertions.Execute` | `AssertionOutcomeCodes` | Usually under 8 KB |
| `POST api/comparison/assertions/batch` | Input-ordered assertion result list | `DatabaseChecker.Assertions.Execute` | `AssertionOutcomeCodes`, `MatcherKindCodes` | Bounded by `Assertion.MaxBatchSize`; normally under 32 KB |
| `POST api/comparison/assertions/derivability` | Per-address database assertion derivability outcomes | `DatabaseChecker.Assertions.ValidateDerivability` | `AssertionDerivabilityCodes` | One catalog snapshot; bounded by request shape |
| `POST api/comparison/projections/rows` | Redacted, bounded rows for a catalog-verified unique key | `DatabaseChecker.Projections.Execute` | `ProjectionOutcomeCodes` | 20 rows by default, 100 maximum |
| `POST api/comparison/diagnosis` | Ranked hypotheses with bounded probe evidence | `DatabaseChecker.Diagnosis.Execute` | `FailureSourceKindCodes`, `HypothesisKindCodes`, `DiagnosisConfidenceCodes`, `ProbeKindCodes` | 4 KB target budget |
| `GET api/comparison/runs/{id}/findings` | ABP `PagedResultDto<FindingDto>` filtered by severity, kind, object type, schema, and table | `DatabaseChecker.Runs.View` | `DifferenceSeverityCodes`, `DifferenceKindCodes`, `SchemaObjectTypeCodes`, `ComparisonConfidenceCodes` | 20 items by default, 100 maximum, 32 KB hard budget |
| `GET api/comparison/schema-discovery/{connectionId}/tables/{schema}/{table}/describe` | Columns, primary/unique keys, one-level foreign-key neighborhood, and bounded schema-lint warnings for one table | `DatabaseChecker.Connections.View` | `CanonicalDataTypeCodes`, `SchemaLintWarningCodes` | Normally under 32 KB; depends on table metadata |
| `GET api/comparison/schema-discovery/{connectionId}/fingerprint` | One canonical snapshot seal plus per-schema and per-table branch seals | `DatabaseChecker.Connections.View` | `SchemaFingerprintConsts` | One 64-character seal per schema and per table; no schema photograph |

Assertion outcome codes are `Passed`, `RowNotFound`, `ValueMismatch`,
`CardinalityMismatch`, `TimedOut`, `KeyNotUnique`, `TableNotFound`, and
`ColumnNotFound`. Finding severity codes are `Breaking`, `NonBreaking`, `Warning`,
and `DocsOnly`.

`TableDescriptionDto.LintWarnings` is deterministic and read-only. It emits
`MissingPrimaryKey` when no primary-key index or constraint exists,
`MissingUniqueKey` when neither a primary key nor an unfiltered unique key can
identify one row, and one `GeneratedColumn` item per generated/computed column
with its `ColumnName`. A primary key satisfies the unique-key requirement; a
filtered unique index does not. The endpoint performs no target mutation and
reuses the existing table-targeted catalog budget.

### Assertion limits and the batch time budget

Every limit resolves through the ABP tenant, global, then package-default chain.

| Setting | Default | Applies to |
|---|---:|---|
| `DatabaseChecker.Assertion.MaxBatchSize` | 20 | Items accepted by one batch call |
| `DatabaseChecker.Assertion.MaxTimeoutMs` | 30000 | Ceiling for **one** assertion's wait |
| `DatabaseChecker.Assertion.MinPollIntervalMs` | 100 | Floor for the retry poll interval |
| `DatabaseChecker.Assertion.MaxRowsPerAssertion` | 100 | Rows read per assertion |
| `DatabaseChecker.Assertion.RegexTimeoutMs` | 200 | One regex matcher evaluation |

**Batch size is rejected, never trimmed.** `AssertBatchAsync` validates the item
count before it opens any target connection. An empty list fails with
`BatchRequired` and a list longer than `MaxBatchSize` fails with `BatchTooLarge`.
Both are business exceptions raised before any assertion runs, so an oversized
batch produces **no partial results** — the caller splits the batch and retries.

**Timeouts are clamped, not rejected.** A request asking for more than
`MaxTimeoutMs` is silently reduced to the ceiling, and a poll interval below
`MinPollIntervalMs` is silently raised to the floor. A caller cannot buy a longer
wait by asking for one, and no error is reported for asking.

**There is no aggregate batch budget.** Items run sequentially in request order,
each with its own clamped timeout, so the worst-case wall time of one batch is
`MaxBatchSize × MaxTimeoutMs` — 600 seconds at the defaults. A caller that needs
a bounded total must either lower `TimeoutMs` per item, reduce the batch size, or
impose its own deadline; the package does not do it. Budget the call accordingly.

A failing item does **not** stop the batch: every item is attempted and results
come back in input order, each carrying its own outcome code and `AttemptCount`.
Cancellation is honoured between items and surfaces as a cancellation, so a
cancelled batch also returns no partial list.

### Stable finding address and incremental filters

`FindingDto.Address` publishes the exact address inputs used by the stable SHA-256
fingerprint: `SourceEngineCode`, `TargetEngineCode`, `SchemaName`,
`ObjectTypeCode`, `ObjectName`, then `ChildName`, in that order. The existing flat
schema/object fields remain on `FindingDto` for compatibility.

The human-readable target address grammar is `schema.object[.child]`. A simple
identifier matching `[a-z_][a-z0-9_]*` is emitted without quotes. Every other
identifier is wrapped in double quotes and an embedded double quote is doubled.
Case is preserved with ordinal semantics. The checker does not infer a default
schema: adapters must provide the resolved catalog name such as `public` or `dbo`.
For display only, a null schema is `<default>`, an empty component is `<empty>`, and
the optional child segment is omitted when null.

Fingerprint components are not built from that display string. Each component is
encoded as `N` when null, otherwise `V{UTF-16-length}:{value}` (`V0:` is an empty
string). The length tags make concatenated components unambiguous; there is no
separator between them. No trimming, lower-casing, unquoting, or default-schema
substitution occurs. After the six address components, the
checker appends `KindCode` and the normalized difference delta with the same
encoding, then returns the uppercase hexadecimal SHA-256 digest.

`GET api/comparison/runs/{id}/findings` accepts an optional `SinceRunId` and up to
100 `Fingerprints`. `SinceRunId` must identify an older completed run visible in
the same tenant and belonging to the same comparison definition. Its known,
non-null fingerprint scalars are removed from the current selection; legacy null
fingerprints are not classified as new. Each explicit fingerprint must be a
64-character hexadecimal SHA-256 value. Duplicates are rejected case-insensitively
and valid values are normalized to uppercase. These filters compose with severity,
kind, object type, schema, and table filters, and the same predicate drives both
`TotalCount` and the bounded page. The repository projects only reference
fingerprint scalars and limited current-family windows; it never materializes a
whole run's finding bodies for this comparison.

### Schema fingerprint canonicality

`GET api/comparison/schema-discovery/{connectionId}/fingerprint` answers "is this
target still structurally what it was" without returning or storing a schema
photograph. It computes a four-level SHA-256 Merkle chain — `column_fp` →
`table_fp` → `schema_fp` → `snapshot_fp` — and returns `SnapshotFingerprint`,
`AlgorithmCode`, `AlgorithmVersion`, a `Schemas` branch list keyed by schema name,
a `Tables` branch list keyed by the `schema.table` address grammar, and an
informational `ComputedAt`. Every seal is a 64-character uppercase hexadecimal
digest. Nothing is written to the target and nothing is persisted by the checker.

Each level encodes its components with the same length-tagged protocol as the
finding fingerprint (`N` for null, otherwise `V{UTF-16-length}:{value}`), prefixed
by a level tag so a value at one level can never collide with another level. Child
lists are sorted ordinally by their own computed part, so read order never changes
a seal. The exact component order per level is published as
`SchemaFingerprintConsts.ComponentOrder`.

**Inside the seal.** Column: name, raw data type, canonical data type, max length,
numeric precision, numeric scale, nullability, default expression, generated flag,
generation expression, persisted flag, collation, identity flag, identity seed,
identity increment. Table: schema name, table name, the sorted column seals, and
the sorted constraint, index, and trigger definitions — a constraint contributes
its name, type, columns, referenced table and columns, referential actions,
definition text, and its validated/enabled/deferrable/initially-deferred state.
Schema: schema name, the sorted table seals, and the sorted non-table object
definitions (name, object type, definition). Snapshot: algorithm code, algorithm
version, engine code, database collation name, collation provider code, and the
sorted schema seals. Identifier, expression, and definition text pass through the
same normalizer the comparison engine uses, so whitespace, statement terminators,
redundant outer parentheses, and identifier quoting are not differences.

**Outside the seal, deliberately.** `CollectedAt` and `ComputedAt`; row counts,
table sizes, and every statistic or estimate; catalog read order; column ordinal
position (a column is identified by name, so inserting a column does not disturb
its neighbours); the database name; `ExtraProperties`; column and object comments,
which the engine already classifies as `DocsOnly`; and `TypeMappingFidelityCode`,
which grades the checker's own type map rather than the target's structure. A
change to any of these leaves every seal bit-identical.

**Version contract.** `AlgorithmVersion` increases whenever the component set, the
component order, the level tags, or the normalization rules change. Two seals are
comparable only when both `AlgorithmCode` and `AlgorithmVersion` match; a mismatch
means "not comparable", not "drifted". Seals are stable across processes, hosts,
and cultures, so a consumer may store one and compare it much later.

## Runtime settings

Settings resolve through the ABP tenant, global, then package-default chain.

- `DatabaseChecker.Assertion.MaxTimeoutMs` (30000), `MinPollIntervalMs` (100),
  `MaxRowsPerAssertion` (100), `RegexTimeoutMs` (200), `MaxBatchSize` (20).
- `DatabaseChecker.Diagnosis.MaxProbeCount` (8), `MaxDurationMs` (3000),
  `ProbeStatementTimeoutMs` (1000), `MaxHypotheses` (5).
- `DatabaseChecker.Connection.ConnectTimeoutSeconds` (10),
  `StatementTimeoutSeconds` (30), `LockTimeoutSeconds` (5),
  `ReadOnlyTransaction` (`true`), `ApplicationNamePrefix`
  (`CheckNexus.DatabaseComparison`).
- `DatabaseChecker.DataComparison.MaxRowsPerTable` (100000),
  `ValueRetentionMode` (`None`), `ValueRedactionSalt` (empty by default).
- `DatabaseChecker.Findings.PageSize` (20), `MaxPageSize` (100), and
  `MaxResponseBytes` (32768).

## OpenTelemetry

Register the `CheckNexus.DatabaseComparison` activity source to collect
`checknexus.db.assert.row`, `checknexus.db.assert.batch`,
`checknexus.db.diagnosis.run`, and `checknexus.db.findings.query`. Emitted
attributes are limited to `db.system.name`, `db.namespace`,
`checknexus.outcome_code`, `checknexus.attempt_count`, `checknexus.probe_count`,
and `checknexus.duration_ms`; cell values, hosts, user names, secret paths, and raw
error messages are never attached.

## Target connection safety

Target connections resolve their timeout and read-only policy through ABP settings
(tenant, then global, then package default). PostgreSQL receives
`statement_timeout`, `lock_timeout`, and `default_transaction_read_only` as startup
options. SQL Server receives the command timeout in the connection string and
`LOCK_TIMEOUT` immediately after the session opens. Both providers identify the
client as `CheckNexus.DatabaseComparison/<package-version>` by default.

TLS is stored per connection. New connections default to `TlsModeCode=Require` and
`TrustServerCertificate=false`. Set certificate trust bypass to `true` only for a
known self-signed target; it disables certificate-chain validation. The connection
test also reports `CanWrite`, `IsSuperUser`, and an excessive-privilege warning code.
Excessive privilege is a finding and does not make an otherwise successful
connection test fail.

## Minimum target grants

Use a distinct login for each environment and grant only the profile the comparison
definition needs. Replace angle-bracket placeholders before executing these examples;
keep login credentials in the configured secret provider.

### PostgreSQL 14+

`SchemaOnly` needs connection access. PostgreSQL system catalogs are readable through
the normal catalog visibility rules:

```sql
CREATE ROLE <checker_login> LOGIN;
GRANT CONNECT ON DATABASE <target_database> TO <checker_login>;
```

`DataCompare` additionally needs read access to the selected data. The predefined
role is the simplest database-wide option; table-specific `SELECT` grants are a
narrower alternative:

```sql
GRANT pg_read_all_data TO <checker_login>;
-- Narrower alternative:
-- GRANT USAGE ON SCHEMA <target_schema> TO <checker_login>;
-- GRANT SELECT ON ALL TABLES IN SCHEMA <target_schema> TO <checker_login>;
```

Do not grant `pg_write_all_data`, database `CREATE`, or superuser. The privilege
probe reports those capabilities as excessive.

### SQL Server

`SchemaOnly` needs a database user plus metadata visibility:

```sql
USE [<target_database>];
CREATE USER [<checker_login>] FOR LOGIN [<checker_login>];
GRANT CONNECT TO [<checker_login>];
GRANT VIEW DEFINITION TO [<checker_login>];
```

`DataCompare` additionally needs read access:

```sql
USE [<target_database>];
ALTER ROLE [db_datareader] ADD MEMBER [<checker_login>];
```

Do not add the login to `sysadmin`, `db_owner`, or `db_datawriter`; the privilege
probe reports any of those memberships as excessive.

## Finding value retention

Data findings use the tenant-aware `DatabaseChecker.DataComparison.ValueRetentionMode`
setting. The default `None` mode persists no primary-key or cell value. `Hashed` uses
deterministic HMAC-SHA256 and requires a non-empty
`DatabaseChecker.DataComparison.ValueRedactionSalt`; `Masked` keeps only limited edge
characters; `Full` explicitly persists raw values. Comparison hashes and difference
detection are computed before redaction and therefore do not change with this policy.

## Package family

- `CheckNexus.DatabaseComparison.Domain.Shared`
- `CheckNexus.DatabaseComparison.Domain`
- `CheckNexus.DatabaseComparison.Application.Contracts`
- `CheckNexus.DatabaseComparison.Application`
- `CheckNexus.DatabaseComparison.EntityFrameworkCore`
- `CheckNexus.DatabaseComparison.HttpApi`
- `CheckNexus.DatabaseComparison.HttpApi.Client`
- `CheckNexus.DatabaseComparison`

This is a prerelease package. Validate module initialization, migrations, external
authentication, target database permissions, and secret-provider configuration in
the target host before use.

Release builds produce deterministic `.nupkg` files plus `.snupkg` symbols. Source
links are embedded when the build runs from a Git checkout with repository metadata;
exported source trees without `.git` still produce the same package payload without
inventing repository information.

## License

MIT
