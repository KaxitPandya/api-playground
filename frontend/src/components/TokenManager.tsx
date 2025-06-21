import React, { useState, useEffect } from 'react';
import { 
  Box, 
  TextField, 
  Button, 
  Typography, 
  Paper, 
  InputAdornment, 
  IconButton,
  Snackbar,
  Alert
} from '@mui/material';
import ContentCopyIcon from '@mui/icons-material/ContentCopy';
import DeleteIcon from '@mui/icons-material/Delete';
import { getAuthToken, setAuthToken, removeAuthToken, formatBearerToken } from '../services/auth';

interface TokenManagerProps {
  onTokenChange?: (token: string | null) => void;
}

const TokenManager: React.FC<TokenManagerProps> = ({ onTokenChange }) => {
  const [token, setToken] = useState<string>('');
  const [snackbarOpen, setSnackbarOpen] = useState<boolean>(false);
  const [snackbarMessage, setSnackbarMessage] = useState<string>('');

  // Load token from local storage on component mount
  useEffect(() => {
    const storedToken = getAuthToken();
    if (storedToken) {
      setToken(storedToken);
    }
  }, []);

  const handleTokenChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setToken(event.target.value);
  };

  const saveToken = () => {
    const formattedToken = token.trim();
    if (formattedToken) {
      setAuthToken(formattedToken);
      if (onTokenChange) {
        onTokenChange(formattedToken);
      }
      setSnackbarMessage('Bearer token saved successfully');
      setSnackbarOpen(true);
    }
  };

  const clearToken = () => {
    setToken('');
    removeAuthToken();
    if (onTokenChange) {
      onTokenChange(null);
    }
    setSnackbarMessage('Bearer token cleared');
    setSnackbarOpen(true);
  };

  const copyToken = () => {
    const fullToken = formatBearerToken(token);
    navigator.clipboard.writeText(fullToken);
    setSnackbarMessage('Bearer token copied to clipboard');
    setSnackbarOpen(true);
  };

  const handleCloseSnackbar = () => {
    setSnackbarOpen(false);
  };

  return (
    <Paper sx={{ p: 2, mb: 2 }}>
      <Typography variant="h6" gutterBottom>
        Authentication Token
      </Typography>
      <Box sx={{ display: 'flex', alignItems: 'flex-start' }}>
        <TextField
          label="Bearer Token"
          variant="outlined"
          fullWidth
          value={token}
          onChange={handleTokenChange}
          placeholder="Enter your Bearer token"
          margin="normal"
          InputProps={{
            endAdornment: token && (
              <InputAdornment position="end">
                <IconButton onClick={copyToken} edge="end" title="Copy token">
                  <ContentCopyIcon />
                </IconButton>
              </InputAdornment>
            ),
          }}
        />
      </Box>
      <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1, mt: 1 }}>
        <Button 
          variant="outlined" 
          color="error" 
          onClick={clearToken}
          startIcon={<DeleteIcon />}
        >
          Clear
        </Button>
        <Button 
          variant="contained" 
          color="primary" 
          onClick={saveToken}
        >
          Save Token
        </Button>
      </Box>
      <Snackbar 
        open={snackbarOpen} 
        autoHideDuration={3000} 
        onClose={handleCloseSnackbar}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert onClose={handleCloseSnackbar} severity="success" sx={{ width: '100%' }}>
          {snackbarMessage}
        </Alert>
      </Snackbar>
    </Paper>
  );
};

export default TokenManager;