// TypeScript models that match the C# backend models

export enum HttpMethod {
  GET = 'GET',
  POST = 'POST',
  PUT = 'PUT',
  DELETE = 'DELETE',
  PATCH = 'PATCH',
  HEAD = 'HEAD',
  OPTIONS = 'OPTIONS'
}

export enum AuthenticationType {
  None = 'None',
  BearerToken = 'BearerToken',
  OAuth2 = 'OAuth2',
  BasicAuth = 'BasicAuth',
  ApiKey = 'ApiKey'
}

export enum ExecutionMode {
  Sequential = 'Sequential',
  Parallel = 'Parallel',
  Conditional = 'Conditional'
}

export interface Integration {
  id: string;
  name: string;
  description: string;
  requests: Request[];
  createdAt: string;
  updatedAt: string;
  executionMode?: ExecutionMode;
  maxParallelRequests?: number;
  retryConfig?: RetryConfig;
  oAuth2Config?: OAuth2Config;
  placeholders?: PlaceholderDefinition[];
}

export interface Request {
  id: string;
  integrationId: string;
  name: string;
  method: HttpMethod;
  url: string;
  headers: Record<string, string>;
  body: string;
  order: number;
  extractVariables?: ExtractVariable[];
  createdAt: string;
  updatedAt: string;
  authenticationType?: AuthenticationType;
  retryConfig?: RetryConfig;
  conditionalLogic?: ConditionalLogic;
  timeout?: number;
}

export interface ExtractVariable {
  name: string;
  jsonPath: string;
}

export interface RequestResult {
  id: string;
  requestId: string;
  requestName: string;
  statusCode: number;
  responseTimeMs: number;
  response?: string;
  responseBody?: string;
  responseHeaders?: Record<string, string>;
  durationMs?: number;
  executedAt: string;
  timestamp?: string;
  error?: string;
  request?: Request;
  attemptNumber?: number;
  retryCount?: number;
  isSuccess?: boolean;
}

export interface PlaceholderMap {
  Values: { [key: string]: string };
}

// New Advanced Models

export interface PlaceholderDefinition {
  name: string;
  description: string;
  defaultValue?: string;
  required: boolean;
}

export interface RetryConfig {
  maxRetries: number;
  delayBetweenRetriesMs: number;
  exponentialBackoff: boolean;
  retryOnStatusCodes: number[];
}

export interface ConditionalLogic {
  condition: string;
  skipIfFalse: boolean;
  continueOnError: boolean;
}

export interface OAuth2Config {
  authorizationUrl: string;
  tokenUrl: string;
  clientId: string;
  clientSecret: string;
  scope: string[];
  redirectUri: string;
  state?: string;
}

export interface ExecutionConfig {
  placeholders: Record<string, string>;
  mode: ExecutionMode;
  maxParallelRequests: number;
  timeoutMs: number;
  stopOnFirstError: boolean;
  enableRetries: boolean;
}

// AI Generation Models
export interface AIGenerationRequest {
  description: string;
  baseUrl?: string;
  examples?: string[];
  authenticationType?: string;
}

export interface AIGenerationResponse {
  integration: Integration;
  explanation: string;
  suggestions: string[];
}

// OpenAPI Import Models
export interface OpenAPIImportRequest {
  url?: string;
  fileContent?: string;
  selectedOperations?: string[];
  baseUrl?: string;
}

export interface OpenAPIImportResponse {
  integration: Integration;
  availableOperations: string[];
  importedCount: number;
  warnings: string[];
}

// OAuth Models
export interface OAuthAuthorizationRequest {
  integrationId: string;
  state?: string;
}

export interface OAuthTokenResponse {
  accessToken: string;
  refreshToken?: string;
  expiresIn: number;
  tokenType: string;
  scope: string[];
}

// Execution Models
export interface ParallelExecutionRequest {
  placeholders: Record<string, string>;
  maxParallelRequests: number;
  timeoutMs: number;
  enableRetries: boolean;
}

export interface ConditionalExecutionRequest {
  placeholders: Record<string, string>;
  stopOnFirstError: boolean;
  enableRetries: boolean;
}