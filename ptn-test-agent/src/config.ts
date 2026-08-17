import { z } from 'zod';

const EnvironmentSchema = z.object({
  AGENT_MODEL: z.string().trim().min(1),
  OPENAI_API_KEY: z.string().min(1),
  PTN_MCP_URL: z.url(),
  AUTH_ORIGIN: z.url(),
  UI_ORIGIN: z.url().default('http://localhost:3000'),
  AGENT_HOST: z.string().default('127.0.0.1'),
  AGENT_PORT: z.coerce.number().int().min(1).max(65_535).default(4310),
  AGENT_MAX_TURNS: z.coerce.number().int().positive().max(64).default(8),
  AGENT_TOKEN_LIMIT: z.coerce.number().int().positive().default(16_000),
  AGENT_UPLOAD_MAX_BYTES: z.coerce.number().int().positive().max(1_048_576).default(262_144),
});

export interface AgentConfig {
  readonly model: string;
  readonly openAiApiKey: string;
  readonly mcpUrl: URL;
  readonly authOrigin: URL;
  readonly uiOrigin: string;
  readonly host: string;
  readonly port: number;
  readonly maxTurns: number;
  readonly tokenLimit: number;
  readonly uploadMaxBytes: number;
}

export function loadConfig(environment: NodeJS.ProcessEnv = process.env): AgentConfig {
  const parsed = EnvironmentSchema.parse(environment);
  return {
    model: parsed.AGENT_MODEL,
    openAiApiKey: parsed.OPENAI_API_KEY,
    mcpUrl: new URL(parsed.PTN_MCP_URL),
    authOrigin: new URL(parsed.AUTH_ORIGIN),
    uiOrigin: new URL(parsed.UI_ORIGIN).origin,
    host: parsed.AGENT_HOST,
    port: parsed.AGENT_PORT,
    maxTurns: parsed.AGENT_MAX_TURNS,
    tokenLimit: parsed.AGENT_TOKEN_LIMIT,
    uploadMaxBytes: parsed.AGENT_UPLOAD_MAX_BYTES,
  };
}

export function publicConfig(config: AgentConfig): Record<string, unknown> {
  return {
    modelConfigured: config.model.length > 0,
    mcpOrigin: config.mcpUrl.origin,
    host: config.host,
    port: config.port,
    maxTurns: config.maxTurns,
    tokenLimit: config.tokenLimit,
    uploadMaxBytes: config.uploadMaxBytes,
  };
}
