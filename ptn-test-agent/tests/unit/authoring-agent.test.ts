import { describe, expect, it, vi } from 'vitest';
import { AuthoringAgent } from '../../src/agent/authoring-agent.js';
import type { McpGateway, McpTool } from '../../src/mcp/mcp-gateway.js';
import type { ModelAdapter } from '../../src/model/model-adapter.js';
import { SessionStore } from '../../src/session/session-store.js';

const profileTool: McpTool = {
  name: 'ptn_profile',
  description: 'Returns the closed agent profile.',
  inputSchema: {
    type: 'object',
    properties: { momentCode: { type: 'string' } },
    required: ['momentCode'],
  },
};

const model: ModelAdapter = {
  async *stream() {
    await Promise.resolve();
    yield { type: 'completed', responseId: 'response-1', inputTokens: 1, outputTokens: 1 };
  },
};

function createGateway(overrides: Partial<McpGateway> = {}): McpGateway {
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
    ...overrides,
  };
}

function createAgent(gateway: McpGateway): AuthoringAgent {
  return new AuthoringAgent(
    () => gateway,
    model,
    new SessionStore(),
    { maxTurns: 8, tokenLimit: 16_000, uploadMaxBytes: 32_000 },
  );
}

describe('AuthoringAgent gateway lifecycle', () => {
  it('closes the gateway when session initialization fails', async () => {
    const close = vi.fn(() => Promise.resolve());
    const gateway = createGateway({
      close,
      listTools: () => Promise.reject(new Error('MCP initialization failed.')),
    });
    const agent = createAgent(gateway);

    await expect(agent.startSession('Grounding', 'user-1', 'token-1')).rejects.toThrow('MCP initialization failed.');
    expect(close).toHaveBeenCalledOnce();
  });

  it('closes a cancelled session exactly once', async () => {
    const close = vi.fn(() => Promise.resolve());
    const agent = createAgent(createGateway({ close }));
    const session = await agent.startSession('Grounding', 'user-1', 'token-1');

    await agent.cancel(session.id, 'user-1', 'token-1');
    await agent.cancel(session.id, 'user-1', 'token-1');

    expect(session.status).toBe('cancelled');
    expect(close).toHaveBeenCalledOnce();
  });

  it('closes the session when a human rejects the pending proposal', async () => {
    const close = vi.fn(() => Promise.resolve());
    const agent = createAgent(createGateway({ close }));
    const session = await agent.startSession('Grounding', 'user-1', 'token-1');
    session.status = 'approval_required';
    session.pendingProposal = {
      stepId: 'createOrder',
      operationReferenceId: '300e183d-988f-4171-8527-cb376aa6b3be',
      assertionPaths: ['$.status'],
    };

    await agent.resolveApproval(session.id, 'user-1', 'token-1', false);

    expect(session.status).toBe('cancelled');
    expect(close).toHaveBeenCalledOnce();
  });

  it('aborts and closes every session during shutdown', async () => {
    const firstClose = vi.fn(() => Promise.resolve());
    const secondClose = vi.fn(() => Promise.resolve());
    const gateways = [createGateway({ close: firstClose }), createGateway({ close: secondClose })];
    const agent = new AuthoringAgent(
      () => {
        const gateway = gateways.shift();
        if (gateway === undefined) throw new Error('No gateway remains for the test.');
        return gateway;
      },
      model,
      new SessionStore(),
      { maxTurns: 8, tokenLimit: 16_000, uploadMaxBytes: 32_000 },
    );
    const first = await agent.startSession('Grounding', 'user-1', 'token-1');
    const second = await agent.startSession('Grounding', 'user-2', 'token-2');

    await agent.close();

    expect(first.status).toBe('cancelled');
    expect(second.status).toBe('cancelled');
    expect(firstClose).toHaveBeenCalledOnce();
    expect(secondClose).toHaveBeenCalledOnce();
  });
});
