import { z } from 'zod';

export const AgentMomentSchema = z.enum([
  'Grounding',
  'Drafting',
  'Validation',
  'Approval',
  'Execution',
  'Diagnosis',
]);

export const StartSessionSchema = z.object({
  momentCode: AgentMomentSchema.default('Grounding'),
});

export const ClosedAnswerSchema = z.object({
  questionCode: z.string().trim().min(1).max(128),
  selectedOption: z.string().trim().min(1).max(2048),
});

export const SendMessageSchema = z.object({
  message: z.string().trim().min(1).max(32_000),
  answers: z.array(ClosedAnswerSchema).max(20).default([]),
});

export const UploadSchema = z.object({
  fileName: z.enum(['senaryo.md', 'kurallar.md']),
  content: z.string(),
});

export const StepProposalSchema = z.object({
  stepId: z.string().trim().regex(/^[A-Za-z][A-Za-z0-9_-]{0,63}$/),
  operationReferenceId: z.uuid(),
  requestBodyJson: z.string().max(64_000).nullable().optional(),
  assertionPaths: z.array(z.string().trim().min(1).max(512)).min(1).max(50),
}).strict();

export const ClosedQuestionSchema = z.object({
  questionCode: z.string().min(1),
  prompt: z.string().min(1),
  options: z.array(z.string().min(1)).min(1),
  gapKindCode: z.string().optional(),
});

export const AgentProfileSchema = z.preprocess((value) => {
  if (typeof value !== 'object' || value === null) {
    return value;
  }

  const source = value as Record<string, unknown>;
  return {
    momentCode: source.momentCode ?? source.MomentCode,
    allowedToolCodes: source.allowedToolCodes ?? source.AllowedToolCodes,
    maxTurns: source.maxTurns ?? source.MaxTurns,
    tokenLimit: source.tokenLimit ?? source.TokenLimit,
  };
}, z.object({
  momentCode: AgentMomentSchema,
  allowedToolCodes: z.array(z.string().min(1)).max(12),
  maxTurns: z.number().int().positive(),
  tokenLimit: z.number().int().positive(),
}));

export type AgentMoment = z.infer<typeof AgentMomentSchema>;
export type ClosedAnswer = z.infer<typeof ClosedAnswerSchema>;
export type ClosedQuestion = z.infer<typeof ClosedQuestionSchema>;
export type StepProposal = z.infer<typeof StepProposalSchema>;

export type AgentEvent =
  | { type: 'text_delta'; delta: string }
  | { type: 'tool_call'; name: string }
  | { type: 'input_required'; questions: ClosedQuestion[] }
  | { type: 'approval_required'; proposal: StepProposal }
  | { type: 'completed'; turns: number; tokens: number }
  | { type: 'cancelled' }
  | { type: 'error'; code: string; message: string };
