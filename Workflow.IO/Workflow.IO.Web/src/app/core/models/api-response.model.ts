/** Mirrors Workflow.IO.Shared.Contracts.ApiResponse&lt;T&gt; */
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
}

/** Mirrors Workflow.IO.Shared.Contracts.ErrorResponse (400 validation). */
export interface ErrorResponse {
  message: string;
  traceId?: string;
  errors?: Record<string, string[]>;
}
