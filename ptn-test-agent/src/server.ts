import { AuthoringAgent } from './agent/authoring-agent.js';
import { loadConfig, publicConfig } from './config.js';
import { createServer } from './http/create-server.js';
import { SdkMcpGateway } from './mcp/sdk-mcp-gateway.js';
import { OpenAiModelAdapter } from './model/openai-model-adapter.js';
import { SessionStore } from './session/session-store.js';

const config = loadConfig();
const gateway = new SdkMcpGateway(config.mcpUrl, config.mcpBearerToken);
await gateway.connect();

const model = new OpenAiModelAdapter(config.openAiApiKey, config.model);
const agent = new AuthoringAgent(gateway, model, new SessionStore(), {
  maxTurns: config.maxTurns,
  tokenLimit: config.tokenLimit,
  uploadMaxBytes: config.uploadMaxBytes,
});
const server = createServer(agent);

const shutdown = async (): Promise<void> => {
  await server.close();
  await gateway.close();
};
process.once('SIGINT', () => void shutdown());
process.once('SIGTERM', () => void shutdown());

await server.listen({ host: config.host, port: config.port });
process.stdout.write(`${JSON.stringify({ event: 'agent_started', ...publicConfig(config) })}\n`);
