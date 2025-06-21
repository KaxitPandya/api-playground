import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import TokenManager from '../TokenManager';
import * as authService from '../../services/auth';

// Mock the clipboard API
Object.assign(navigator, {
  clipboard: {
    writeText: jest.fn(),
  },
});

// Mock the auth service
jest.mock('../../services/auth', () => ({
  getAuthToken: jest.fn(),
  setAuthToken: jest.fn(),
  removeAuthToken: jest.fn(),
  formatBearerToken: jest.fn((token) => `Bearer ${token}`),
}));

describe('TokenManager Component', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  test('renders with empty token initially', () => {
    (authService.getAuthToken as jest.Mock).mockReturnValue(null);
    
    render(<TokenManager />);
    
    expect(screen.getByLabelText(/Bearer Token/i)).toHaveValue('');
  });

  test('loads token from local storage on mount', () => {
    (authService.getAuthToken as jest.Mock).mockReturnValue('test-token');
    
    render(<TokenManager />);
    
    expect(screen.getByLabelText(/Bearer Token/i)).toHaveValue('test-token');
  });

  test('saves token when Save button is clicked', () => {
    (authService.getAuthToken as jest.Mock).mockReturnValue(null);
    const onTokenChangeMock = jest.fn();
    
    render(<TokenManager onTokenChange={onTokenChangeMock} />);
    
    const input = screen.getByLabelText(/Bearer Token/i);
    fireEvent.change(input, { target: { value: 'new-token' } });
    
    fireEvent.click(screen.getByText(/Save Token/i));
    
    expect(authService.setAuthToken).toHaveBeenCalledWith('new-token');
    expect(onTokenChangeMock).toHaveBeenCalledWith('new-token');
  });

  test('clears token when Clear button is clicked', () => {
    (authService.getAuthToken as jest.Mock).mockReturnValue('test-token');
    const onTokenChangeMock = jest.fn();
    
    render(<TokenManager onTokenChange={onTokenChangeMock} />);
    
    fireEvent.click(screen.getByText(/Clear/i));
    
    expect(authService.removeAuthToken).toHaveBeenCalled();
    expect(onTokenChangeMock).toHaveBeenCalledWith(null);
  });

  test('copies token to clipboard when copy button is clicked', () => {
    (authService.getAuthToken as jest.Mock).mockReturnValue('test-token');
    
    render(<TokenManager />);
    
    // Find the copy button (iconbutton) and click it
    const copyButton = screen.getByTitle(/Copy token/i);
    fireEvent.click(copyButton);
    
    expect(navigator.clipboard.writeText).toHaveBeenCalledWith('Bearer test-token');
  });
});