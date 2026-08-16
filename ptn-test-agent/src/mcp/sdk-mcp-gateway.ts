import { Ajv, type ValidateFunction } from 'ajv/dist/ajv.js';
import {
  Client,
  StreamableHTTPClientTransport,
  type CallToolResult,
  type ContentBlock,
  type Tool,
} from '@modelcontextprotocol/client';
import { z } from 'zod';
import type { McpGateway, McpTool, McpToolResult } from './mcp-gateway.js';

const JsonValueSchema: z.ZodType<unknown> = z.lazy(() => z.union([
  z.string(),
  z.number(),
  z.boolean(),
  z.null(),
  z.array(JsonValueSchema),
  z.record(z.string(), JsonValueSchema),
]));

export class SdkMcpGateway implements McpGateway {
  readonly #client: Client;
  readonly #transport: StreamableHTTPClientTransport;
  readonly #ajv = new Ajv({ allErrors: true, strict: false });

  public constructor(url: URL, bearerToken: string) {
    this.#client = new Client({ name: 'ptn-test-authoring-agent', version: '0.1.0' });
    this.#transport = new StreamableHTTPClientTransport(url, {
      authProvider: { token: async () => bearerToken },
      onInsufficientScope: 'throw',
    });
  }

  public connect(): Promise<void> {
    return this.#client.connect(this.#transport);
  }

  public close(): Promise<void> {
    return this.#client.close();
  }

  public async readTextResource(uri: string, signal?: AbortSignal): Promise<string> {
    const result = await this.#client.readResource({ uri }, requestOptions(signal));
    const text = result.contents
      .filter((content): content is Extract<typeof content, { text: string }> => 'text' in content)
      .map((content) => content.text)
      .join('\n');
    return z.string().min(1).parse(text);
  }

  public async listTools(signal?: AbortSignal): Promise<McpTool[]> {
    const result = await this.#client.listTools(undefined, requestOptions(signal));
    return result.tools.map(mapMcpTool);
  }

  public async callTool(tool: McpTool, input: unknown, signal?: AbortSignal): Promise<McpToolResult> {
    this.#validate(tool.inputSchema, input, `${tool.name} input`);
    const result = await this.#client.callTool({ name: tool.name, arguments: asArguments(input) }, requestOptions(signal));
    const value = extractValue(result);
    JsonValueSchema.parse(value);
    if (tool.outputSchema !== undefined) {
      this.#validate(tool.outputSchema, value, `${tool.name} output`);
    }

    return {
      isError: result.isError === true,
      value,
      modelOutput: JSON.stringify(value),
    };
  }

  #validate(schema: Record<string, unknown>, value: unknown, label: string): void {
    const validate: ValidateFunction = this.#ajv.compile(schema);
    if (!validate(value)) {
      throw new Error(`${label} failed runtime schema validation: ${this.#ajv.errorsText(validate.errors)}`);
    }
  }
}

function requestOptions(signal?: AbortSignal): { signal?: AbortSignal } | undefined {
  return signal === undefined ? undefined : { signal };
}

function mapMcpTool(tool: Tool): McpTool {
  const result: McpTool = {
    name: tool.name,
    description: tool.description ?? '',
    inputSchema: tool.inputSchema as Record<string, unknown>,
    ...(tool.outputSchema === undefined ? {} : { outputSchema: tool.outputSchema as Record<string, unknown> }),
  };
  return result;
}

function asArguments(value: unknown): Record<string, unknown> {
  if (typeof value !== 'object' || value === null || Array.isArray(value)) {
    throw new Error('MCP tool arguments must be a JSON object.');
  }
  return value as Record<string, unknown>;
}

function extractValue(result: CallToolResult): unknown {
  if (result.structuredContent !== undefined) {
    return result.structuredContent;
  }

  const text = result.content.filter(isTextContent).map((content) => content.text).join('\n');
  if (text.length === 0) {
    return result.isError === true ? { isError: true } : {};
  }

  try {
    return JSON.parse(text) as unknown;
  } catch {
    return { text };
  }
}

function isTextContent(content: ContentBlock): content is Extract<ContentBlock, { type: 'text' }> {
  return content.type === 'text';
}
