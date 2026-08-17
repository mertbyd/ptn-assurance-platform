import { AuthoringAgent } from './agent/authoring-agent.js';
import { loadConfig, publicConfig } from './config.js';
import { createServer } from './http/create-server.js';
import { SdkMcpGateway } from './mcp/sdk-mcp-gateway.js';
import { OpenAiModelAdapter } from './model/openai-model-adapter.js';
import { SessionStore } from './session/session-store.js';

const config = loadConfig();
const model = new OpenAiModelAdapter(config.openAiApiKey, config.model);
const agent = new AuthoringAgent(
  (bearerToken) => new SdkMcpGateway(config.mcpUrl, bearerToken),
  model,
  new SessionStore(),
  {
    maxTurns: config.maxTurns,
    tokenLimit: config.tokenLimit,
    uploadMaxBytes: config.uploadMaxBytes,
  },
);
const server = createServer(agent, config.authOrigin, config.uiOrigin);

const shutdown = async (): Promise<void> => {
  try {
    await agent.close();
  } finally {
    await server.close();
  }
};
process.once('SIGINT', () => void shutdown());
process.once('SIGTERM', () => void shutdown());

await server.listen({ host: config.host, port: config.port });
process.stdout.write(`${JSON.stringify({ event: 'agent_started', ...publicConfig(config) })}\n`);
