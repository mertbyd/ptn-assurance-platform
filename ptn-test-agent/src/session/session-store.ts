import { randomUUID } from 'node:crypto';
import type { AgentMoment, ClosedQuestion, StepProposal } from '../contracts.js';
import type { McpTool } from '../mcp/mcp-gateway.js';

export type SessionStatus = 'ready' | 'running' | 'input_required' | 'approval_required' | 'cancelled';

export interface AgentSession {
  readonly id: string;
  readonly momentCode: AgentMoment;
  readonly instructions: string;
  readonly tools: McpTool[];
  readonly maxTurns: number;
  readonly tokenLimit: number;
  readonly uploads: Map<string, string>;
  status: SessionStatus;
  turns: number;
  tokens: number;
  previousResponseId?: string | undefined;
  pendingQuestions: ClosedQuestion[];
  pendingProposal?: StepProposal | undefined;
  activeAbort?: AbortController | undefined;
}

export class SessionStore {
  readonly #sessions = new Map<string, AgentSession>();

  public create(input: Omit<AgentSession, 'id' | 'uploads' | 'status' | 'turns' | 'tokens' | 'pendingQuestions'>): AgentSession {
    const session: AgentSession = {
      ...input,
      id: randomUUID(),
      uploads: new Map(),
      status: 'ready',
      turns: 0,
      tokens: 0,
      pendingQuestions: [],
    };
    this.#sessions.set(session.id, session);
    return session;
  }

  public get(id: string): AgentSession {
    const session = this.#sessions.get(id);
    if (session === undefined) {
      throw new SessionNotFoundError();
    }
    return session;
  }
}

export class SessionNotFoundError extends Error {
  public constructor() {
    super('Agent session was not found.');
  }
}
