import OpenAI from 'openai';
import type { ResponseInput, Tool } from 'openai/resources/responses/responses';
import type { ModelAdapter, ModelEvent, ModelInput, ModelTool, ModelTurnRequest } from './model-adapter.js';

const ProposeStepToolName = 'propose_authoring_step';

export class OpenAiModelAdapter implements ModelAdapter {
  readonly #client: OpenAI;
  readonly #model: string;

  public constructor(apiKey: string, model: string) {
    this.#client = new OpenAI({ apiKey });
    this.#model = model;
  }

  public async *stream(request: ModelTurnRequest, signal: AbortSignal): AsyncIterable<ModelEvent> {
    const responseStream = await this.#client.responses.create({
      model: this.#model,
      instructions: request.instructions,
      input: mapInput(request.input),
      tools: [...request.tools.map(mapTool), proposalTool()],
      ...(request.previousResponseId === undefined ? {} : { previous_response_id: request.previousResponseId }),
      max_output_tokens: request.maxOutputTokens,
      parallel_tool_calls: false,
      store: false,
      stream: true,
    }, { signal });

    for await (const event of responseStream) {
      if (event.type === 'response.output_text.delta') {
        yield { type: 'text_delta', delta: event.delta };
      }

      if (event.type === 'response.completed') {
        for (const output of event.response.output) {
          if (output.type === 'function_call') {
            yield {
              type: 'tool_call',
              callId: output.call_id,
              name: output.name,
              arguments: JSON.parse(output.arguments) as unknown,
            };
          }
        }

        yield {
          type: 'completed',
          responseId: event.response.id,
          inputTokens: event.response.usage?.input_tokens ?? 0,
          outputTokens: event.response.usage?.output_tokens ?? 0,
        };
      }
    }
  }
}

function mapInput(input: ModelInput[]): ResponseInput {
  return input.map((item) => item.type === 'user'
    ? { role: 'user' as const, content: item.content }
    : { type: 'function_call_output' as const, call_id: item.callId, output: item.output });
}

function mapTool(tool: ModelTool): Tool {
  return {
    type: 'function',
    name: tool.name,
    description: tool.description,
    parameters: tool.inputSchema,
    strict: false,
  };
}

function proposalTool(): Tool {
  return {
    type: 'function',
    name: ProposeStepToolName,
    description: 'Propose exactly one grounded authoring step for explicit user approval. Never emit final Arazzo YAML.',
    parameters: proposalJsonSchema(),
    strict: true,
  };
}

function proposalJsonSchema(): Record<string, unknown> {
  return {
    type: 'object',
    additionalProperties: false,
    required: ['stepId', 'operationReferenceId', 'assertionPaths'],
    properties: {
      stepId: { type: 'string', pattern: '^[A-Za-z][A-Za-z0-9_-]{0,63}$' },
      operationReferenceId: { type: 'string', format: 'uuid' },
      requestBodyJson: { type: ['string', 'null'], maxLength: 64_000 },
      assertionPaths: {
        type: 'array',
        minItems: 1,
        maxItems: 50,
        items: { type: 'string', minLength: 1, maxLength: 512 },
      },
    },
  };
}

export { ProposeStepToolName };
