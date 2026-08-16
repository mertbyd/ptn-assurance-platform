# CheckNexus API Contracts

Reusable ABP module for registering OpenAPI sources, normalizing specifications,
creating snapshots, comparing API contracts, and exposing the workflow through
application services and HTTP API controllers.

## Install

Use the composition package when the complete in-process module is required:

```xml
<PackageReference Include="CheckNexus.ApiContracts" Version="0.2.0-alpha.3" />
```

The package targets .NET 10 and uses ABP Framework 10.3.0. It transitively includes
the Application, HttpApi, and EntityFrameworkCore packages. Layer-specific packages
remain available for consumers that intentionally need a narrower compile-time
surface.

`0.2.0-alpha.2` is public on NuGet.org and immutable. This source tree is the
**next** candidate, `0.2.0-alpha.3`, and is not published yet; PackageValidation
runs against `0.2.0-alpha.2` as its baseline.

## Composition contract

- Add `ApiContractCheckerModule` to the executable ABP module graph.
- Configure the `ApiContractChecker` connection string. When it is absent, ABP falls
  back to `Default`.
- Apply `ApiContractCheckerDbContext` migrations from the executable's migration
  workflow. The module owns only its checker tables and its migration history table.
- Register an implementation of
  `Ptn.ApiContractChecker.Interface.Secrets.ISecretProvider`. The optional
  `CheckNexus.Vault` package provides the shared Vault KV v2 adapter.
- Supply authentication, user/tenant management, notification delivery, scheduling,
  and production hosting from the composition application.

The repository's `Ptn.ApiContractChecker.HttpApi.Host` project is a development and
verification executable. It is intentionally not included in any NuGet package.

## Test Module API surface

All endpoints return the host's `Result<T>` envelope. The two execution permissions
are independent so a consumer can grant conformance without granting diagnosis.

| Method and route | Return | Required permission | Swagger group |
| --- | --- | --- | --- |
| `POST api/contract-checks/conformance/response` | Response conformance outcome and bounded violations | `ApiContractChecker.Conformance.Execute` | `Conformance` |
| `POST api/contract-checks/conformance/request` | Request conformance outcome and bounded violations | `ApiContractChecker.Conformance.Execute` | `Conformance` |
| `POST api/contract-checks/conformance/request-example` | Bounded placeholder request | `ApiContractChecker.Conformance.Execute` | `Conformance` |
| `POST api/contract-checks/conformance/operation-bindings` | Ranked operation binding suggestions | `ApiContractChecker.Conformance.Execute` | `Conformance` |
| `POST api/contract-checks/conformance/assertion-derivability` | Scenario assertion derivability result | `ApiContractChecker.Conformance.Execute` | `Conformance` |
| `POST api/contract-checks/diagnosis` | Ranked deterministic diagnosis report | `ApiContractChecker.Diagnosis.Execute` | `Diagnosis` |
| `GET api/checks/{id}/findings` | Filtered, bounded finding page with stable address and change state | `ApiContractChecker.Checks.View` | `Checks` |

`FindingDto` also exposes the additive `Fingerprint` and `ChangeState` fields. Use
`FindingChangeStateCodes` rather than localized labels when filtering or branching.

### Stable finding address and operation fingerprint

`FindingDto.Address` publishes all eight typed address components in the exact
fingerprint order: `OperationId`, `HttpMethod`, `Path`, `SchemaName`,
`PropertyPath`, `ParameterName`, `ResponseStatus`, then `MediaType`. Producers trim
every non-empty component and store whitespace-only values as missing. HTTP methods
are uppercased and media types are lowercased; operation IDs, normalized OpenAPI path
templates, schema names, JSON Pointer property paths, parameter names, and response
status values otherwise preserve case and content. `Path` is the normalized OpenAPI
template produced by the comparison model (for example `/orders/{id}`), without a
query string. `PropertyPath` is the comparison model's JSON Pointer-like path.

The complete SHA-256 input order is `KindCode`, `DirectionCode`, those eight address
components, `OldDelta`, then `NewDelta`. A missing/blank address component becomes
`<empty>`; other address values retain the stored normalization above. Each component
is framed as `{UTF-16-length}:{value}` and the framed components are joined with `|`.
Under `None` value retention, a missing delta is `missing` and a present delta is
`value:<JsonValueKind>` (`String` is used for non-JSON text). Under retaining modes,
the delta is `missing` or `value:` followed by the retained value. The UTF-8 payload
is hashed with SHA-256 and returned as uppercase hexadecimal. Consumers therefore do
not need to infer any omitted operation-fingerprint input.

The findings query accepts optional `SinceRunId` and up to 100 `Fingerprints` in
addition to severity, kind, change-state, path, and schema filters. Each fingerprint
must be a 64-character hexadecimal SHA-256 value; duplicates are rejected
case-insensitively and accepted values are normalized to uppercase. `SinceRunId`
must be an older completed, visible run whose base and target snapshots belong to
the same base/target specification-document pair as the current run. With no
explicit change-state filter it selects findings that are `New` relative to that
reference. Explicit change-state and fingerprint filters are intersected, and the
same selection drives both count and page queries. Legacy null fingerprints remain
`Unknown` and are never promoted to `New`. Production providers project only the two
run fingerprint scalars plus the bounded current page; the SQLite verification
fallback materializes only the specifically selected run, never all runs.

## Upgrade note from 0.1.x

The new DTOs, routes, permissions, and `FindingDto` fields are additive for normal
ABP proxy consumers. However, 0.2 adds methods to the existing
`IContractCheckRunAppService`, `ISpecSnapshotAppService`, and
`IContractCheckRunRepository` interfaces. Consumers that implement these interfaces
directly must implement the new members before upgrading. The intentional 0.2 binary
compatibility decision is recorded in ADR-0010; the former six-argument `Finding`
constructor remains available.

## JSON Schema dependency

`CheckNexus.ApiContracts.Domain` has a public runtime dependency on `NJsonSchema`
11.6.1. It is intentionally transitive because the exported schema-resolution model
uses NJsonSchema types. Consumers should validate their resolved dependency graph and
must not exclude this package from runtime assets. The selection and compatibility
policy are recorded in ADR-0009.

## Stable contract codes

Consumers must branch on the exported code constants, not localized text:

- Conformance: `ConformanceOutcomeCodes`, `ConformanceRuleCodes`,
  `AssertionDerivabilityCodes`
- Diagnosis: `HypothesisKindCodes`, `DiagnosisConfidenceCodes`, `ProbeKindCodes`
- Difference: `DifferenceKindCodes`, `DifferenceDirectionCodes`,
  `DifferenceSeverityCodes`
- Finding maintenance: `FindingChangeStateCodes` (`New`, `Known`, `Resolved`,
  `Unknown`)

## Output ceilings

| Surface | Maximum UTF-8 output |
|---|---:|
| Request/response conformance and G2 derivability | 512 B |
| Diagnosis report | 4 KB |
| Paged findings | 32 KB |
| Operation, schema, request-example, and binding summaries | 2 KB |
| Raw OpenAPI or observed HTTP body | never returned |

Finding responses use ABP paging plus severity, kind, change-state, path, and
schema filters and report every effective clamp explicitly. Truncated operation
and schema authoring summaries supply a short-lived `resultRef`; retrieving that
reference is a separate authorized call and does not repeat the original operation.

### Conformance violation truncation

The conformance ceiling is enforced by two settings, resolved through the ABP
tenant, global, then package-default chain:

| Setting | Default |
|---|---:|
| `ApiContractChecker.Conformance.MaxViolations` | 4 |
| `ApiContractChecker.Conformance.MaxResponseBytes` | 512 |

`AssertResponseAsync` and `AssertRequestAsync` never fail because a response is
too large; they truncate deterministically, in this exact order:

1. **Count trim.** If the result carries more than `MaxViolations` violations,
   the entries past that count are dropped.
2. **Byte trim.** The result is serialized and measured as UTF-8. While it
   exceeds `MaxResponseBytes` minus a fixed 128-byte transport serialization
   margin (so 384 bytes at the defaults), the **last** remaining violation is
   dropped and the result is measured again.

Two consequences the caller must plan for:

- Truncation always removes from the **tail**, so violation order is
  significant: the earliest-reported violations are the ones that survive. The
  order is deterministic for a given snapshot and observed response.
- `OutcomeCode` is computed **before** truncation, from the full candidate set.
  A `Fail` outcome stays `Fail` even if every violation explaining it was
  trimmed, so the outcome never silently softens.

The result carries no truncation flag. A caller that needs to record whether
trimming occurred compares the returned violation count against
`MaxViolations`; a result at exactly the ceiling should be treated as possibly
truncated. Violations never carry observed values — only a JSON Pointer, a rule
code, and a schema keyword — so a trimmed result leaks no payload content.

MCP tools do not live in this package. The composition host curates MCP tools and
delegates to these HTTP/application contracts, so package consumers do not acquire
an MCP dependency and runtime conformance remains a zero-model-call path.

## Package family

- `CheckNexus.ApiContracts.Domain.Shared`
- `CheckNexus.ApiContracts.Domain`
- `CheckNexus.ApiContracts.Application.Contracts`
- `CheckNexus.ApiContracts.Application`
- `CheckNexus.ApiContracts.EntityFrameworkCore`
- `CheckNexus.ApiContracts.HttpApi`
- `CheckNexus.ApiContracts.HttpApi.Client`
- `CheckNexus.ApiContracts`

This is a prerelease package. Validate module initialization, migrations, external
authentication, and secret-provider configuration in the target host before use.

Release builds produce deterministic `.nupkg` files plus `.snupkg` symbols. Source
links are embedded when the build runs from the package family's Azure Repos Git
checkout; exported source trees without `.git` do not invent repository metadata.

## License

MIT
