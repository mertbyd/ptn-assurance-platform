import { describe, expect, it } from 'vitest';
import type { McpGateway } from '../../src/mcp/mcp-gateway.js';
import {
  SessionForbiddenError,
  SessionNotFoundError,
  SessionStore,
} from '../../src/session/session-store.js';

const gateway: McpGateway = {
  setBearerToken: () => undefined,
  connect: () => Promise.resolve(),
  close: () => Promise.resolve(),
  readTextResource: () => Promise.resolve(''),
  listTools: () => Promise.resolve([]),
  callTool: () => Promise.resolve({ isError: false, value: null, modelOutput: '' }),
};

function createSession(store: SessionStore, ownerId: string) {
  return store.create({
    ownerId,
    gateway,
    momentCode: 'Grounding',
    instructions: 'test policy',
    tools: [],
    maxTurns: 8,
    tokenLimit: 16_000,
  });
}

describe('SessionStore', () => {
  it('returns a session only to its owner', () => {
    const store = new SessionStore();
    const session = createSession(store, 'user-1');

    expect(store.get(session.id, 'user-1')).toBe(session);
    expect(() => store.get(session.id, 'user-2')).toThrow(SessionForbiddenError);
  });

  it('does not reveal whether another session id exists', () => {
    const store = new SessionStore();

    expect(() => store.get('missing', 'user-1')).toThrow(SessionNotFoundError);
  });
});
