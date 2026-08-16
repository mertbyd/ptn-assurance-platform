export interface ModelTool {
  readonly name: string;
  readonly description: string;
  readonly inputSchema: Record<string, unknown>;
}

export type ModelInput =
  | { readonly type: 'user'; readonly content: string }
  | { readonly type: 'tool_output'; readonly callId: string; readonly output: string };

export interface ModelTurnRequest {
  readonly instructions: string;
  readonly input: ModelInput[];
  readonly tools: ModelTool[];
  readonly previousResponseId?: string;
  readonly maxOutputTokens: number;
}

export type ModelEvent =
  | { readonly type: 'text_delta'; readonly delta: string }
  | { readonly type: 'tool_call'; readonly callId: string; readonly name: string; readonly arguments: unknown }
  | {
      readonly type: 'completed';
      readonly responseId: string;
      readonly inputTokens: number;
      readonly outputTokens: number;
    };

export interface ModelAdapter {
  stream(request: ModelTurnRequest, signal: AbortSignal): AsyncIterable<ModelEvent>;
}
