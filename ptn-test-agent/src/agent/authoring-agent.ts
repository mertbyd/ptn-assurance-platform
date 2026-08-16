import { Buffer } from 'node:buffer';
import {
  AgentProfileSchema,
  ClosedQuestionSchema,
  StepProposalSchema,
  type AgentEvent,
  type AgentMoment,
  type ClosedAnswer,
  type ClosedQuestion,
} from '../contracts.js';
import type { McpGateway, McpTool, McpToolResult } from '../mcp/mcp-gateway.js';
import type { ModelAdapter, ModelEvent, ModelInput, ModelTool } from '../model/model-adapter.js';
import { ProposeStepToolName } from '../model/openai-model-adapter.js';
import { SessionStore, type AgentSession } from '../session/session-store.js';

const AgentPolicyUri = 'ptn://authoring/agent-policy.md';
const BusinessRulesUri = 'ptn://authoring/kurallar.md';
const ProfileToolName = 'ptn_profile';

export interface AuthoringAgentLimits {
  readonly maxTurns: number;
  readonly tokenLimit: number;
  readonly uploadMaxBytes: number;
}

export class AuthoringAgent {
  public constructor(
    private readonly gateway: McpGateway,
    private readonly model: ModelAdapter,
    private readonly sessions: SessionStore,
    private readonly limits: AuthoringAgentLimits,
  ) {}

  public async startSession(momentCode: AgentMoment): Promise<AgentSession> {
    const [policy, businessRules, listedTools] = await Promise.all([
      this.gateway.readTextResource(AgentPolicyUri),
      this.gateway.readTextResource(BusinessRulesUri),
      this.gateway.listTools(),
    ]);
    const profileTool = requiredTool(listedTools, ProfileToolName);
    const profileResult = await this.gateway.callTool(profileTool, profileArguments(profileTool, momentCode));
    if (profileResult.isError) {
      throw new Error('The MCP server rejected the requested agent profile.');
    }
    const profile = AgentProfileSchema.parse(profileResult.value);
    if (profile.momentCode !== momentCode) {
      throw new Error('The MCP profile moment does not match the requested moment.');
    }

    const allowed = new Set(profile.allowedToolCodes);
    const tools = listedTools.filter((tool) => allowed.has(tool.name) && tool.name !== ProfileToolName);
    if (tools.length !== allowed.size - Number(allowed.has(ProfileToolName))) {
      throw new Error('The MCP profile references a tool that is not discoverable.');
    }

    return this.sessions.create({
      momentCode,
      instructions: createInstructions(policy, businessRules),
      tools,
      maxTurns: Math.min(profile.maxTurns, this.limits.maxTurns),
      tokenLimit: Math.min(profile.tokenLimit, this.limits.tokenLimit),
    });
  }

  public upload(sessionId: string, fileName: 'senaryo.md' | 'kurallar.md', content: string): void {
    const session = this.sessions.get(sessionId);
    if (session.status === 'cancelled') {
      throw new Error('Cancelled sessions cannot accept uploads.');
    }
    if (Buffer.byteLength(content, 'utf8') > this.limits.uploadMaxBytes) {
      throw new Error('The upload exceeds the configured byte limit.');
    }
    session.uploads.set(fileName, content);
  }

  public cancel(sessionId: string): void {
    const session = this.sessions.get(sessionId);
    session.activeAbort?.abort();
    session.status = 'cancelled';
  }

  public resolveApproval(sessionId: string, approved: boolean): void {
    const session = this.sessions.get(sessionId);
    if (session.status !== 'approval_required' || session.pendingProposal === undefined) {
      throw new Error('The session has no proposal awaiting approval.');
    }
    session.pendingProposal = undefined;
    session.status = approved ? 'ready' : 'cancelled';
  }

  public async *sendMessage(sessionId: string, message: string, answers: ClosedAnswer[]): AsyncIterable<AgentEvent> {
    const session = this.sessions.get(sessionId);
    if (session.status === 'running' || session.status === 'approval_required' || session.status === 'cancelled') {
      throw new Error(`The session cannot accept a message while ${session.status}.`);
    }

    const answerContext = validateAnswers(session, answers);
    const abort = new AbortController();
    session.activeAbort = abort;
    session.status = 'running';
    let input: ModelInput[] = [{ type: 'user', content: `${answerContext}${message}` }];

    try {
      while (true) {
        if (session.turns >= session.maxTurns || session.tokens >= session.tokenLimit) {
          session.status = 'ready';
          yield { type: 'error', code: 'budget_exceeded', message: 'The authoring budget was exhausted.' };
          return;
        }

        session.turns += 1;
        const remainingTokens = session.tokenLimit - session.tokens;
        const modelEvents = this.model.stream({
          instructions: withUploads(session),
          input,
          tools: session.tools.map(toModelTool),
          ...(session.previousResponseId === undefined ? {} : { previousResponseId: session.previousResponseId }),
          maxOutputTokens: Math.min(4_096, remainingTokens),
        }, abort.signal);

        const toolCalls: Extract<ModelEvent, { type: 'tool_call' }>[] = [];
        for await (const event of modelEvents) {
          if (event.type === 'text_delta') {
            yield { type: 'text_delta', delta: event.delta };
          } else if (event.type === 'tool_call') {
            toolCalls.push(event);
          } else {
            session.previousResponseId = event.responseId;
            session.tokens += event.inputTokens + event.outputTokens;
          }
        }

        if (session.tokens > session.tokenLimit) {
          session.status = 'ready';
          yield { type: 'error', code: 'budget_exceeded', message: 'The authoring token budget was exhausted.' };
          return;
        }

        if (toolCalls.length === 0) {
          session.status = 'ready';
          yield { type: 'completed', turns: session.turns, tokens: session.tokens };
          return;
        }

        input = [];
        for (const call of toolCalls) {
          if (call.name === ProposeStepToolName) {
            const proposal = StepProposalSchema.parse(call.arguments);
            session.pendingProposal = proposal;
            session.status = 'approval_required';
            yield { type: 'approval_required', proposal };
            return;
          }

          const tool = requiredTool(session.tools, call.name);
          yield { type: 'tool_call', name: tool.name };
          const result = await this.gateway.callTool(tool, call.arguments, abort.signal);
          const questions = extractQuestions(result);
          if (questions.length > 0) {
            session.pendingQuestions = questions;
            session.status = 'input_required';
            yield { type: 'input_required', questions };
            return;
          }
          input.push({ type: 'tool_output', callId: call.callId, output: result.modelOutput });
        }
      }
    } catch (error) {
      if (abort.signal.aborted) {
        session.status = 'cancelled';
        yield { type: 'cancelled' };
        return;
      }
      session.status = 'ready';
      yield { type: 'error', code: 'agent_failure', message: safeErrorMessage(error) };
    } finally {
      session.activeAbort = undefined;
    }
  }
}

function createInstructions(policy: string, businessRules: string): string {
  return [
    'You are the PTN scenario authoring agent. Use only the listed MCP tools.',
    'Never guess operation, table, column, schema, or controlled codes.',
    'Never emit final Arazzo YAML. Use propose_authoring_step for exactly one grounded step.',
    'Stop when a tool returns closed questions; the host will request a human answer.',
    '<agent-policy>', policy, '</agent-policy>',
    '<business-rules>', businessRules, '</business-rules>',
  ].join('\n');
}

function withUploads(session: AgentSession): string {
  if (session.uploads.size === 0) {
    return session.instructions;
  }
  const uploads = [...session.uploads.entries()]
    .map(([name, content]) => `<user-file name="${name}">\n${content}\n</user-file>`)
    .join('\n');
  return `${session.instructions}\n${uploads}`;
}

function profileArguments(tool: McpTool, momentCode: AgentMoment): Record<string, unknown> {
  const properties = tool.inputSchema.properties;
  return typeof properties === 'object' && properties !== null && 'input' in properties
    ? { input: { momentCode } }
    : { momentCode };
}

function requiredTool(tools: McpTool[], name: string): McpTool {
  const tool = tools.find((candidate) => candidate.name === name);
  if (tool === undefined) {
    throw new Error(`Required MCP tool is unavailable: ${name}.`);
  }
  return tool;
}

function toModelTool(tool: McpTool): ModelTool {
  return { name: tool.name, description: tool.description, inputSchema: tool.inputSchema };
}

function extractQuestions(result: McpToolResult): ClosedQuestion[] {
  if (typeof result.value !== 'object' || result.value === null) {
    return [];
  }
  const source = result.value as Record<string, unknown>;
  const rawQuestions = source.questions ?? source.Questions;
  if (!Array.isArray(rawQuestions)) {
    return [];
  }
  return rawQuestions.map((question) => normalizeQuestion(question)).map((question) => ClosedQuestionSchema.parse(question));
}

function normalizeQuestion(value: unknown): unknown {
  if (typeof value !== 'object' || value === null) {
    return value;
  }
  const source = value as Record<string, unknown>;
  return {
    questionCode: source.questionCode ?? source.QuestionCode,
    prompt: source.prompt ?? source.Prompt,
    options: source.options ?? source.Options,
    gapKindCode: source.gapKindCode ?? source.GapKindCode,
  };
}

function validateAnswers(session: AgentSession, answers: ClosedAnswer[]): string {
  if (session.pendingQuestions.length === 0) {
    if (answers.length > 0) {
      throw new Error('No closed answers are expected for this session.');
    }
    return '';
  }

  const answerMap = new Map(answers.map((answer) => [answer.questionCode, answer.selectedOption]));
  if (answerMap.size !== session.pendingQuestions.length) {
    throw new Error('Every pending closed question must be answered exactly once.');
  }
  for (const question of session.pendingQuestions) {
    const selected = answerMap.get(question.questionCode);
    if (selected === undefined || !question.options.includes(selected)) {
      throw new Error(`Answer for ${question.questionCode} is not one of the closed options.`);
    }
  }

  session.pendingQuestions = [];
  session.status = 'ready';
  return `Closed human answers (authoritative; do not reinterpret): ${JSON.stringify(answers)}\n`;
}

function safeErrorMessage(error: unknown): string {
  if (error instanceof Error && error.name === 'ZodError') {
    return 'A provider or MCP response failed runtime schema validation.';
  }
  return 'The authoring turn failed without exposing provider or credential details.';
}
