# ADR-0001 — TypeScript authoring agent runtime and provider boundary

- Status: Accepted
- Date: 2026-08-16
- Owner: Test Platform team
- Scope: `ptn-test-agent/`

## Context

KBP-111 needs a separately deployed authoring agent with HTTP chat, SSE streaming, file input,
cancellation, an MCP tool loop, and structured single-step proposals. The agent may call Test
Module only through its authorized `/mcp` endpoint. It must never enter the deterministic run or
judgment dependency graph, connect to checker packages/databases, or generate final Arazzo YAML.

## Decision

- Runtime: Node.js 24 LTS, ESM TypeScript, pinned with `engines.node >=24 <25`.
- Package manager: pnpm 11.19.0 with a committed lockfile and frozen installs in verification.
- HTTP/streaming: Fastify 5; JSON commands and standard Server-Sent Events for model output.
- MCP: `@modelcontextprotocol/client` 2.0.0 over Streamable HTTP with bearer auth. The live
  server instructions, Resources, discoverable tools, and moment profile define the model's tool
  surface. No local expanded catalog is allowed.
- Runtime schemas: Zod validates config, uploads, public HTTP payloads, MCP tool input/output,
  model calls, and the one permitted authoring-step proposal.
- Model boundary: provider-neutral `ModelAdapter`; the first implementation uses the official
  OpenAI JavaScript SDK 7.4.0 and the Responses API function-calling/streaming loop. The model ID
  is mandatory `AGENT_MODEL` configuration, not a hard-coded alias.
- Secrets: `OPENAI_API_KEY` and `PTN_MCP_BEARER_TOKEN` come only from process/deployment secrets.
  They are rejected when missing and redacted from logs, errors, DTOs, SSE events, and traces.
- Deployment: `ptn-test-agent` is a separate process/container owned by the Test Platform team.
  Test Module, checker, runner, run, and judgment projects cannot depend on this package.
- Local-model support remains closed unless the KBP-111 evaluation proves tool-selection
  F1 >= 0.90.

The client enforces `maxTurns` and token budgets before every model call; the MCP server's
decision remains authoritative. `input_required` pauses for a closed user answer. Tier-4 actions
are never auto-applied. Uploads accept only UTF-8 `senaryo.md` and `kurallar.md` within a fixed
byte budget.

## Alternatives rejected

- A .NET model client: violates the run/judgment isolation boundary.
- OpenAI Agents SDK as the domain runtime: couples orchestration rules to the first provider.
- Node 26 Current: not an LTS production line on the decision date.
- Express or raw `node:http`: respectively adds a broader middleware surface or recreates body,
  validation, streaming, and redaction infrastructure.
- A static local tool catalog: can bypass the server's moment profile and tool budget.

## Consequences

Provider changes remain inside adapters; MCP and model fixtures can test the loop without real
secrets. The deployment must maintain abort propagation, SSE disconnect cleanup, secret scans,
frozen dependency installs, and tool-schema drift tests. Raw model prompts and outputs are not
persisted; only conversation/model references and token counters may use OpenTelemetry GenAI
attributes.

## Sources

- https://nodejs.org/en/about/previous-releases
- https://fastify.dev/docs/latest/Reference/LTS/
- https://github.com/modelcontextprotocol/typescript-sdk/blob/main/docs/client.md
- https://github.com/modelcontextprotocol/typescript-sdk/blob/main/docs/get-started/packages.md
- https://developers.openai.com/api/docs/guides/function-calling
- https://developers.openai.com/api/docs/guides/streaming-responses
