import { describe, expect, it } from 'vitest';
import { loadConfig, publicConfig } from '../../src/config.js';

const validEnvironment: NodeJS.ProcessEnv = {
  AGENT_MODEL: 'gpt-test',
  OPENAI_API_KEY: 'local-test-secret',
  PTN_MCP_URL: 'https://localhost:44366/mcp',
  AUTH_ORIGIN: 'https://localhost:44323',
  UI_ORIGIN: 'http://localhost:3000/path',
};

describe('agent config', () => {
  it('normalizes origins and keeps secrets out of the public payload', () => {
    const config = loadConfig(validEnvironment);
    const exposed = publicConfig(config);

    expect(config.uiOrigin).toBe('http://localhost:3000');
    expect(exposed).not.toHaveProperty('openAiApiKey');
    expect(JSON.stringify(exposed)).not.toContain('local-test-secret');
  });

  it('rejects a missing OpenAI API key', () => {
    expect(() => loadConfig({ ...validEnvironment, OPENAI_API_KEY: '' })).toThrow();
  });
});
