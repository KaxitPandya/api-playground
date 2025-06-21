import axios from 'axios';
import { 
  Integration, 
  Request, 
  RequestResult, 
  PlaceholderMap,
  AIGenerationRequest,
  AIGenerationResponse,
  OpenAPIImportRequest,
  OpenAPIImportResponse,
  OAuthAuthorizationRequest,
  OAuthTokenResponse,
  ParallelExecutionRequest,
  ConditionalExecutionRequest,
  ExecutionConfig
} from '../models';

// API Configuration
const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5001';

console.log('API Base URL:', API_BASE_URL); // Debug log for deployment

// API Client
class ApiClient {
  private baseURL: string;

  constructor(baseURL: string) {
    this.baseURL = baseURL;
  }

  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const url = `${this.baseURL}${endpoint}`;
    
    const config: RequestInit = {
      headers: {
        'Content-Type': 'application/json',
        ...options.headers,
      },
      ...options,
    };

    console.log(`API Request: ${config.method || 'GET'} ${url}`); // Debug log

    try {
      const response = await fetch(url, config);
      
      console.log(`API Response: ${response.status} ${response.statusText}`); // Debug log
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error('API Error Response:', errorText); // Debug log
        throw new Error(`HTTP error! status: ${response.status}, message: ${errorText}`);
      }

      const data = await response.json();
      return data;
    } catch (error) {
      console.error('API Request failed:', error);
      throw error;
    }
  }
}

// Get token from local storage if available
const getAuthToken = () => localStorage.getItem('authToken');

const api = axios.create({
  baseURL: API_BASE_URL,
});

// Add request interceptor for authentication
api.interceptors.request.use(
  (config) => {
    const token = getAuthToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

export const IntegrationService = {
  getAll: async (): Promise<Integration[]> => {
    const response = await api.get<Integration[]>('/api/integrations');
    return response.data;
  },

  getById: async (id: string): Promise<Integration> => {
    const response = await api.get<Integration>(`/api/integrations/${id}`);
    return response.data;
  },

  create: async (integration: Partial<Integration>): Promise<Integration> => {
    const response = await api.post<Integration>('/api/integrations', integration);
    return response.data;
  },

  update: async (integration: Integration): Promise<void> => {
    await api.put(`/api/integrations/${integration.id}`, integration);
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/api/integrations/${id}`);
  },
};

export const RequestService = {
  getByIntegrationId: async (integrationId: string): Promise<Request[]> => {
    const response = await api.get<Request[]>(`/api/requests/integration/${integrationId}`);
    return response.data;
  },

  getById: async (id: string): Promise<Request> => {
    const response = await api.get<Request>(`/api/requests/${id}`);
    return response.data;
  },

  create: async (request: Partial<Request>): Promise<Request> => {
    const response = await api.post<Request>('/api/requests', request);
    return response.data;
  },

  update: async (request: Request): Promise<void> => {
    await api.put(`/api/requests/${request.id}`, request);
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/api/requests/${id}`);
  },
};

export const ExecutionService = {
  executeRequest: async (
    requestId: string, 
    placeholders?: PlaceholderMap
  ): Promise<RequestResult> => {
    const response = await api.post<RequestResult>(
      `/api/executions/request/${requestId}`, 
      placeholders
    );
    return response.data;
  },

  executeIntegration: async (
    integrationId: string, 
    placeholders?: PlaceholderMap
  ): Promise<RequestResult[]> => {
    const response = await api.post<RequestResult[]>(
      `/api/executions/integration/${integrationId}`, 
      placeholders
    );
    return response.data;
  },

  // New advanced execution methods
  executeParallel: async (
    integrationId: string,
    request: ParallelExecutionRequest
  ): Promise<RequestResult[]> => {
    const response = await api.post<RequestResult[]>(
      `/api/executions/integration/${integrationId}/parallel`,
      request
    );
    return response.data;
  },

  executeConditional: async (
    integrationId: string,
    request: ConditionalExecutionRequest
  ): Promise<RequestResult[]> => {
    const response = await api.post<RequestResult[]>(
      `/api/executions/integration/${integrationId}/conditional`,
      request
    );
    return response.data;
  },

  executeWithConfig: async (
    integrationId: string,
    config: ExecutionConfig
  ): Promise<RequestResult[]> => {
    const response = await api.post<RequestResult[]>(
      `/api/executions/integration/${integrationId}/config`,
      config
    );
    return response.data;
  },
};

// New AI Generation Service
export const AIService = {
  generateIntegration: async (request: AIGenerationRequest): Promise<AIGenerationResponse> => {
    const response = await api.post<AIGenerationResponse>('/api/ai/generate', request);
    return response.data;
  },

  suggestImprovements: async (integrationId: string): Promise<string[]> => {
    const response = await api.get<string[]>(`/api/ai/suggestions/${integrationId}`);
    return response.data;
  },

  explainIntegration: async (integrationId: string): Promise<string> => {
    const response = await api.get<string>(`/api/ai/explain/${integrationId}`);
    return response.data;
  },
};

// New OpenAPI Import Service
export const OpenAPIService = {
  importFromUrl: async (url: string, baseUrl?: string, selectedOperations?: string[]): Promise<OpenAPIImportResponse> => {
    const response = await api.post<OpenAPIImportResponse>('/api/openapi/import-url', {
      url,
      baseUrl,
      selectedOperations: selectedOperations || []
    });
    return response.data;
  },

  importFromFile: async (fileContent: string, baseUrl?: string, selectedOperations?: string[]): Promise<OpenAPIImportResponse> => {
    const response = await api.post<OpenAPIImportResponse>('/api/openapi/import-file', {
      fileContent,
      baseUrl,
      selectedOperations: selectedOperations || []
    });
    return response.data;
  },

  getAvailableOperations: async (source: string, isUrl: boolean = true): Promise<string[]> => {
    if (isUrl) {
      // For URLs, use GET with query parameter
      const response = await api.get<string[]>(`/api/openapi/operations?url=${encodeURIComponent(source)}`);
      return response.data;
    } else {
      // For file content, use POST with fileContent in body
      const response = await api.post<string[]>('/api/openapi/operations', {
        fileContent: source
      });
      return response.data;
    }
  },
};

// New OAuth Service
export const OAuthService = {
  getAuthorizationUrl: async (integrationId: string, state?: string): Promise<string> => {
    const response = await api.post<string>(`/api/oauth/authorize`, {
      integrationId,
      state
    });
    return response.data;
  },

  exchangeCodeForToken: async (integrationId: string, code: string, state?: string): Promise<OAuthTokenResponse> => {
    const response = await api.post<OAuthTokenResponse>(`/api/oauth/token`, {
      integrationId,
      code,
      state
    });
    return response.data;
  },

  refreshToken: async (integrationId: string, refreshToken: string): Promise<OAuthTokenResponse> => {
    const response = await api.post<OAuthTokenResponse>(`/api/oauth/refresh`, {
      integrationId,
      refreshToken
    });
    return response.data;
  },

  isTokenValid: async (integrationId: string): Promise<boolean> => {
    const response = await api.get<boolean>(`/api/oauth/validate/${integrationId}`);
    return response.data;
  },

  revokeToken: async (integrationId: string): Promise<void> => {
    await api.post(`/api/oauth/revoke/${integrationId}`);
  },
};

// Health check service
export const HealthService = {
  check: async (): Promise<string> => {
    const response = await api.get<string>('/api/health');
    return response.data;
  },
};
