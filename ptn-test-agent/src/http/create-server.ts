import type { FastifyInstance, FastifyReply, FastifyRequest } from 'fastify';
import Fastify from 'fastify';
import { z } from 'zod';
import {
  SendMessageSchema,
  StartSessionSchema,
  UploadSchema,
  type AgentEvent,
} from '../contracts.js';
import type { AuthoringAgent } from '../agent/authoring-agent.js';
import { SessionForbiddenError, SessionNotFoundError, type AgentSession } from '../session/session-store.js';

const SessionParamsSchema = z.object({ id: z.uuid() });
const ApprovalSchema = z.object({ approved: z.boolean() });

export function createServer(agent: AuthoringAgent, authOrigin: URL, uiOrigin: string): FastifyInstance {
  const server = Fastify({ logger: false, bodyLimit: 1_048_576 });

  server.addHook('onSend', async (request, reply, payload) => {
    if (request.headers.origin === uiOrigin) {
      reply.header('access-control-allow-origin', uiOrigin);
      reply.header('access-control-allow-headers', 'authorization, content-type');
      reply.header('access-control-allow-methods', 'POST, OPTIONS');
      reply.header('vary', 'Origin');
    }
    return payload;
  });

  server.options('/api/agent/*', async (_request, reply) => reply.code(204).send());

  server.post('/api/agent/sessions', async (request, reply) => {
    const principal = await authenticate(request, authOrigin);
    const input = StartSessionSchema.parse(request.body);
    const session = await agent.startSession(input.momentCode, principal.userId, principal.bearerToken);
    return reply.code(201).send(toSessionDto(session));
  });

  server.post('/api/agent/sessions/:id/messages', async (request, reply) => {
    const principal = await authenticate(request, authOrigin);
    const { id } = SessionParamsSchema.parse(request.params);
    const input = SendMessageSchema.parse(request.body);
    return streamEvents(reply, agent.sendMessage(id, principal.userId, principal.bearerToken, input.message, input.answers));
  });

  server.post('/api/agent/sessions/:id/uploads', async (request, reply) => {
    const principal = await authenticate(request, authOrigin);
    const { id } = SessionParamsSchema.parse(request.params);
    const input = UploadSchema.parse(request.body);
    agent.upload(id, principal.userId, principal.bearerToken, input.fileName, input.content);
    return reply.code(204).send();
  });

  server.post('/api/agent/sessions/:id/cancel', async (request, reply) => {
    const principal = await authenticate(request, authOrigin);
    const { id } = SessionParamsSchema.parse(request.params);
    await agent.cancel(id, principal.userId, principal.bearerToken);
    return reply.code(204).send();
  });

  server.post('/api/agent/sessions/:id/approval', async (request, reply) => {
    const principal = await authenticate(request, authOrigin);
    const { id } = SessionParamsSchema.parse(request.params);
    const input = ApprovalSchema.parse(request.body);
    await agent.resolveApproval(id, principal.userId, principal.bearerToken, input.approved);
    return reply.code(204).send();
  });

  server.setErrorHandler((error, _request, reply) => {
    if (error instanceof SessionNotFoundError) {
      return reply.code(404).send({ code: 'session_not_found' });
    }
    if (error instanceof SessionForbiddenError) {
      return reply.code(403).send({ code: 'session_forbidden' });
    }
    if (error instanceof AgentUnauthorizedError) {
      return reply.code(401).send({ code: 'unauthorized' });
    }
    if (error instanceof z.ZodError) {
      return reply.code(400).send({ code: 'invalid_request' });
    }
    return reply.code(409).send({ code: 'agent_state_conflict' });
  });

  return server;
}

interface AgentPrincipal {
  readonly userId: string;
  readonly bearerToken: string;
}

class AgentUnauthorizedError extends Error {}

async function authenticate(request: FastifyRequest, authOrigin: URL): Promise<AgentPrincipal> {
  const authorization = request.headers.authorization;
  if (authorization === undefined || !authorization.startsWith('Bearer ')) {
    throw new AgentUnauthorizedError();
  }

  const bearerToken = authorization.slice('Bearer '.length).trim();
  const url = new URL('/api/authenticator/auth/me', authOrigin);
  const response = await fetch(url, { headers: { authorization: `Bearer ${bearerToken}`, accept: 'application/json' } });
  if (!response.ok) {
    throw new AgentUnauthorizedError();
  }

  const body: unknown = await response.json();
  const payload = unwrapResult(body);
  const userId = readUserId(payload);
  if (userId === undefined) {
    throw new AgentUnauthorizedError();
  }
  return { userId, bearerToken };
}

function unwrapResult(body: unknown): unknown {
  if (typeof body === 'object' && body !== null && 'isSuccess' in body && 'value' in body) {
    return (body as { value: unknown }).value;
  }
  return body;
}

function readUserId(body: unknown): string | undefined {
  if (typeof body !== 'object' || body === null) return undefined;
  const value = (body as Record<string, unknown>).userId;
  return typeof value === 'string' && value.length > 0 ? value : undefined;
}

async function streamEvents(reply: FastifyReply, events: AsyncIterable<AgentEvent>): Promise<void> {
  reply.hijack();
  reply.raw.statusCode = 200;
  reply.raw.setHeader('content-type', 'text/event-stream; charset=utf-8');
  reply.raw.setHeader('cache-control', 'no-cache, no-transform');
  reply.raw.setHeader('connection', 'keep-alive');
  for await (const event of events) {
    reply.raw.write(`event: ${event.type}\ndata: ${JSON.stringify(event)}\n\n`);
  }
  reply.raw.end();
}

function toSessionDto(session: AgentSession): Record<string, unknown> {
  return {
    id: session.id,
    momentCode: session.momentCode,
    status: session.status,
    allowedToolCodes: session.tools.map((tool) => tool.name),
    maxTurns: session.maxTurns,
    tokenLimit: session.tokenLimit,
  };
}
