import type { FastifyInstance, FastifyReply } from 'fastify';
import Fastify from 'fastify';
import { z } from 'zod';
import {
  SendMessageSchema,
  StartSessionSchema,
  UploadSchema,
  type AgentEvent,
} from '../contracts.js';
import type { AuthoringAgent } from '../agent/authoring-agent.js';
import { SessionNotFoundError, type AgentSession } from '../session/session-store.js';

const SessionParamsSchema = z.object({ id: z.uuid() });
const ApprovalSchema = z.object({ approved: z.boolean() });

export function createServer(agent: AuthoringAgent): FastifyInstance {
  const server = Fastify({ logger: false, bodyLimit: 1_048_576 });

  server.post('/api/agent/sessions', async (request, reply) => {
    const input = StartSessionSchema.parse(request.body);
    const session = await agent.startSession(input.momentCode);
    return reply.code(201).send(toSessionDto(session));
  });

  server.post('/api/agent/sessions/:id/messages', async (request, reply) => {
    const { id } = SessionParamsSchema.parse(request.params);
    const input = SendMessageSchema.parse(request.body);
    return streamEvents(reply, agent.sendMessage(id, input.message, input.answers));
  });

  server.post('/api/agent/sessions/:id/uploads', async (request, reply) => {
    const { id } = SessionParamsSchema.parse(request.params);
    const input = UploadSchema.parse(request.body);
    agent.upload(id, input.fileName, input.content);
    return reply.code(204).send();
  });

  server.post('/api/agent/sessions/:id/cancel', async (request, reply) => {
    const { id } = SessionParamsSchema.parse(request.params);
    agent.cancel(id);
    return reply.code(204).send();
  });

  server.post('/api/agent/sessions/:id/approval', async (request, reply) => {
    const { id } = SessionParamsSchema.parse(request.params);
    const input = ApprovalSchema.parse(request.body);
    agent.resolveApproval(id, input.approved);
    return reply.code(204).send();
  });

  server.setErrorHandler((error, _request, reply) => {
    if (error instanceof SessionNotFoundError) {
      return reply.code(404).send({ code: 'session_not_found' });
    }
    if (error instanceof z.ZodError) {
      return reply.code(400).send({ code: 'invalid_request' });
    }
    return reply.code(409).send({ code: 'agent_state_conflict' });
  });

  return server;
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
