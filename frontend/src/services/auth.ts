// Authentication service for managing tokens

/**
 * Sets the authentication token in local storage
 * @param token Bearer token to store
 */
export const setAuthToken = (token: string): void => {
  localStorage.setItem('authToken', token);
};

/**
 * Get the current authentication token from local storage
 * @returns The token string or null if not present
 */
export const getAuthToken = (): string | null => {
  return localStorage.getItem('authToken');
};

/**
 * Remove the authentication token from local storage
 */
export const removeAuthToken = (): void => {
  localStorage.removeItem('authToken');
};

/**
 * Check if a user is currently authenticated (has a token)
 * @returns True if token exists, false otherwise
 */
export const isAuthenticated = (): boolean => {
  return getAuthToken() !== null;
};

/**
 * Format a raw token into the full Bearer format
 * @param token Raw token
 * @returns Formatted token with "Bearer " prefix
 */
export const formatBearerToken = (token: string): string => {
  if (!token) return '';
  
  // Remove any existing "Bearer " prefix
  const cleanToken = token.replace(/^Bearer\s+/i, '');
  
  return `Bearer ${cleanToken}`;
};