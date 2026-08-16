export interface McpTool {
  readonly name: string;
  readonly description: string;
  readonly inputSchema: Record<string, unknown>;
  readonly outputSchema?: Record<string, unknown>;
}

export interface McpToolResult {
  readonly isError: boolean;
  readonly value: unknown;
  readonly modelOutput: string;
}

export interface McpGateway {
  connect(): Promise<void>;
  close(): Promise<void>;
  readTextResource(uri: string, signal?: AbortSignal): Promise<string>;
  listTools(signal?: AbortSignal): Promise<McpTool[]>;
  callTool(tool: McpTool, input: unknown, signal?: AbortSignal): Promise<McpToolResult>;
}
