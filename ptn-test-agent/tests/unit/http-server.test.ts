import { afterEach, describe, expect, it, vi } from 'vitest';
import { AuthoringAgent } from '../../src/agent/authoring-agent.js';
import { createServer } from '../../src/http/create-server.js';
import type { McpGateway, McpTool } from '../../src/mcp/mcp-gateway.js';
import type { ModelAdapter } from '../../src/model/model-adapter.js';
import { SessionStore } from '../../src/session/session-store.js';

const profileTool: McpTool = {
  name: 'ptn_profile',
  description: 'Returns the closed agent profile.',
  inputSchema: { type: 'object', properties: { momentCode: { type: 'string' } } },
};

const model: ModelAdapter = {
  async *stream() {
    await Promise.resolve();
    yield { type: 'completed', responseId: 'response-1', inputTokens: 1, outputTokens: 1 };
  },
};

function createGateway(): McpGateway {
  return {
    setBearerToken: vi.fn(),
    connect: vi.fn(() => Promise.resolve()),
    close: vi.fn(() => Promise.resolve()),
    readTextResource: vi.fn(() => Promise.resolve('closed policy')),
    listTools: vi.fn(() => Promise.resolve([profileTool])),
    callTool: vi.fn(() => Promise.resolve({
      isError: false,
      value: {
        momentCode: 'Grounding',
        allowedToolCodes: ['ptn_profile'],
        maxTurns: 8,
        tokenLimit: 16_000,
      },
      modelOutput: '{}',
    })),
  };
}

function createAgent(): AuthoringAgent {
  return new AuthoringAgent(
    () => createGateway(),
    model,
    new SessionStore(),
    { maxTurns: 8, tokenLimit: 16_000, uploadMaxBytes: 32_000 },
  );
}

function installAuthFetch(): ReturnType<typeof vi.fn<typeof fetch>> {
  const fetchMock = vi.fn<typeof fetch>((input, init) => {
    if (requestUrl(input) !== 'https://auth.example/api/authenticator/auth/me') {
      return Promise.reject(new Error('Unexpected authentication endpoint.'));
    }
    const authorization = new Headers(init?.headers).get('authorization');
    const userId = authorization === 'Bearer token-a' ? 'user-a' : 'user-b';
    return Promise.resolve(new Response(JSON.stringify({ isSuccess: true, value: { userId } }), {
      status: 200,
      headers: { 'content-type': 'application/json' },
    }));
  });
  vi.stubGlobal('fetch', fetchMock);
  return fetchMock;
}

function requestUrl(input: string | URL | Request): string {
  if (typeof input === 'string') return input;
  return input instanceof URL ? input.toString() : input.url;
}

afterEach(() => {
  vi.unstubAllGlobals();
});

describe('agent HTTP security boundary', () => {
  it('rejects a request without a bearer token', async () => {
    const agent = createAgent();
    const server = createServer(agent, new URL('https://auth.example'), 'http://localhost:3000');

    try {
      const response = await server.inject({
        method: 'POST',
        url: '/api/agent/sessions',
        payload: { momentCode: 'Grounding' },
      });

      expect(response.statusCode).toBe(401);
      expect(response.json()).toEqual({ code: 'unauthorized' });
    } finally {
      await agent.close();
      await server.close();
    }
  });

  it('authenticates against the exact Authenticator route and emits exact-origin CORS', async () => {
    const fetchMock = installAuthFetch();
    const agent = createAgent();
    const server = createServer(agent, new URL('https://auth.example'), 'http://localhost:3000');

    try {
      const response = await server.inject({
        method: 'POST',
        url: '/api/agent/sessions',
        headers: { authorization: 'Bearer token-a', origin: 'http://localhost:3000' },
        payload: { momentCode: 'Grounding' },
      });

      expect(response.statusCode).toBe(201);
      expect(response.headers['access-control-allow-origin']).toBe('http://localhost:3000');
      expect(fetchMock).toHaveBeenCalledOnce();
      const calledInput = fetchMock.mock.calls[0]?.[0];
      expect(calledInput).toBeDefined();
      if (calledInput === undefined) throw new Error('The Authenticator request was not captured.');
      expect(requestUrl(calledInput)).toBe('https://auth.example/api/authenticator/auth/me');
    } finally {
      await agent.close();
      await server.close();
    }
  });

  it('blocks one authenticated user from another users session', async () => {
    installAuthFetch();
    const agent = createAgent();
    const server = createServer(agent, new URL('https://auth.example'), 'http://localhost:3000');

    try {
      const startResponse = await server.inject({
        method: 'POST',
        url: '/api/agent/sessions',
        headers: { authorization: 'Bearer token-a' },
        payload: { momentCode: 'Grounding' },
      });
      const session = startResponse.json<{ id: string }>();

      const foreignResponse = await server.inject({
        method: 'POST',
        url: `/api/agent/sessions/${session.id}/uploads`,
        headers: { authorization: 'Bearer token-b', origin: 'https://untrusted.example' },
        payload: { fileName: 'senaryo.md', content: 'test' },
      });

      expect(foreignResponse.statusCode).toBe(403);
      expect(foreignResponse.json()).toEqual({ code: 'session_forbidden' });
      expect(foreignResponse.headers['access-control-allow-origin']).toBeUndefined();
    } finally {
      await agent.close();
      await server.close();
    }
  });
});
