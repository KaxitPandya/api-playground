import React, { useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
  Box,
  TextField,
  FormControl,
  FormLabel,
  RadioGroup,
  FormControlLabel,
  Radio,
  Checkbox,
  Paper,
  Divider,
  Grid,
  Chip,
  Alert,
  CircularProgress,
  IconButton,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Switch,
  FormGroup,
} from '@mui/material';
import {
  Close,
  ExpandMore,
  PlayArrow,
  Security,
  Refresh,
  Speed,
  AccountTree,
} from '@mui/icons-material';
import { ExecutionService, OAuthService } from '../services/api';
import { 
  Integration, 
  RequestResult, 
  ExecutionMode, 
  ParallelExecutionRequest,
  ConditionalExecutionRequest,
  ExecutionConfig
} from '../models';

interface EnhancedExecutionViewProps {
  integration: Integration;
  onExecutionComplete: (results: RequestResult[]) => void;
  onClose: () => void;
}

const EnhancedExecutionView: React.FC<EnhancedExecutionViewProps> = ({
  integration,
  onExecutionComplete,
  onClose
}) => {
  const [executionMode, setExecutionMode] = useState<ExecutionMode>(ExecutionMode.Sequential);
  const [maxParallelRequests, setMaxParallelRequests] = useState(5);
  const [placeholders, setPlaceholders] = useState<Record<string, string>>({});
  const [conditions, setConditions] = useState<Record<string, string>>({});
  const [timeoutMs, setTimeoutMs] = useState(30000);
  
  // Retry configuration
  const [enableRetries, setEnableRetries] = useState(false);
  const [maxRetries, setMaxRetries] = useState(3);
  const [delayBetweenRetriesMs, setDelayBetweenRetriesMs] = useState(1000);
  const [exponentialBackoff, setExponentialBackoff] = useState(true);
  const [retryOnStatusCodes, setRetryOnStatusCodes] = useState<string>('500,502,503,504');
  
  // OAuth configuration
  const [enableOAuth, setEnableOAuth] = useState(false);
  const [oauthLoading, setOauthLoading] = useState(false);
  const [oauthToken, setOauthToken] = useState<string>('');
  const [oauthError, setOauthError] = useState<string | null>(null);
  
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  // Extract placeholders from integration requests
  const extractPlaceholders = () => {
    const placeholderSet = new Set<string>();
    integration.requests.forEach(request => {
      const urlMatches = request.url.match(/\{\{([^}]+)\}\}/g);
      const bodyMatches = request.body?.match(/\{\{([^}]+)\}\}/g);
      
      urlMatches?.forEach(match => {
        const placeholder = match.replace(/[{}]/g, '');
        if (!placeholder.startsWith('$')) {
          placeholderSet.add(placeholder);
        }
      });
      
      bodyMatches?.forEach(match => {
        const placeholder = match.replace(/[{}]/g, '');
        if (!placeholder.startsWith('$')) {
          placeholderSet.add(placeholder);
        }
      });

      // Check headers for placeholders
      Object.values(request.headers).forEach(headerValue => {
        const headerMatches = headerValue.match(/\{\{([^}]+)\}\}/g);
        headerMatches?.forEach(match => {
          const placeholder = match.replace(/[{}]/g, '');
          if (!placeholder.startsWith('$')) {
            placeholderSet.add(placeholder);
          }
        });
      });
    });
    
    return Array.from(placeholderSet);
  };

  const availablePlaceholders = extractPlaceholders();

  const handlePlaceholderChange = (key: string, value: string) => {
    setPlaceholders(prev => ({ ...prev, [key]: value }));
  };

  const handleConditionChange = (key: string, value: string) => {
    setConditions(prev => ({ ...prev, [key]: value }));
  };

  const handleOAuthFlow = async () => {
    setOauthLoading(true);
    setOauthError(null);
    
    try {
      // Get authorization URL
      const authUrl = await OAuthService.getAuthorizationUrl(integration.id, 'advanced_execution');
      
      // Open popup window for OAuth flow
      const popup = window.open(
        authUrl, 
        'oauth_popup', 
        'width=600,height=600,scrollbars=yes,resizable=yes'
      );
      
      // Listen for the OAuth callback
      const checkClosed = setInterval(() => {
        if (popup?.closed) {
          clearInterval(checkClosed);
          // Check if token was stored
          const token = localStorage.getItem(`oauth_token_${integration.id}`);
          if (token) {
            setOauthToken(token);
            setPlaceholders(prev => ({ ...prev, token: token }));
          } else {
            setOauthError('OAuth flow was cancelled or failed');
          }
          setOauthLoading(false);
        }
      }, 1000);
      
    } catch (err) {
      setOauthError(err instanceof Error ? err.message : 'OAuth flow failed');
      setOauthLoading(false);
    }
  };

  const buildRetryConfig = () => {
    if (!enableRetries) return undefined;
    
    return {
      maxAttempts: maxRetries,
      delayMs: delayBetweenRetriesMs,
      exponentialBackoff,
      retryOnStatusCodes: retryOnStatusCodes.split(',').map(code => parseInt(code.trim())).filter(code => !isNaN(code))
    };
  };

  const handleExecute = async () => {
    setLoading(true);
    setError(null);

    try {
      let results: RequestResult[];

      if (executionMode === ExecutionMode.Parallel) {
        const request: ParallelExecutionRequest = {
          placeholders,
          maxParallelRequests,
          timeoutMs,
          enableRetries
        };
        results = await ExecutionService.executeParallel(integration.id, request);
      } else if (executionMode === ExecutionMode.Conditional) {
        const request: ConditionalExecutionRequest = {
          placeholders,
          stopOnFirstError: true,
          enableRetries
        };
        results = await ExecutionService.executeConditional(integration.id, request);
      } else {
        // Sequential with advanced config
        const config: ExecutionConfig = {
          placeholders,
          mode: executionMode,
          maxParallelRequests: 1,
          timeoutMs,
          stopOnFirstError: false,
          enableRetries
        };
        results = await ExecutionService.executeWithConfig(integration.id, config);
      }

      onExecutionComplete(results);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to execute integration');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog 
      open={true} 
      onClose={onClose}
      maxWidth="lg"
      fullWidth
      PaperProps={{
        sx: { borderRadius: 3, maxHeight: '95vh' }
      }}
    >
      <DialogTitle sx={{ pb: 1 }}>
        <Box display="flex" alignItems="center" justifyContent="space-between">
          <Box display="flex" alignItems="center">
            <Speed color="primary" sx={{ mr: 1.5, fontSize: 28 }} />
            <Typography variant="h5" component="div" fontWeight="bold">
              Advanced Execution: {integration.name}
            </Typography>
          </Box>
          <IconButton onClick={onClose} size="small">
            <Close />
          </IconButton>
        </Box>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
          Configure advanced execution modes, retries, and OAuth authentication
        </Typography>
      </DialogTitle>

      <Divider />

      <DialogContent sx={{ pt: 2 }}>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
          
          {/* Execution Mode Selection */}
          <Paper elevation={1} sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center' }}>
              <AccountTree sx={{ mr: 1 }} />
              Execution Mode
            </Typography>
            <FormControl component="fieldset">
              <RadioGroup
                value={executionMode}
                onChange={(e) => setExecutionMode(e.target.value as ExecutionMode)}
              >
                <Grid container spacing={2}>
                  <Grid item xs={12} md={4}>
                    <FormControlLabel 
                      value={ExecutionMode.Sequential} 
                      control={<Radio />} 
                      label={
                        <Box>
                          <Typography variant="body1" fontWeight="bold">Sequential</Typography>
                          <Typography variant="body2" color="text.secondary">
                            Execute requests one by one
                          </Typography>
                        </Box>
                      }
                    />
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <FormControlLabel 
                      value={ExecutionMode.Parallel} 
                      control={<Radio />} 
                      label={
                        <Box>
                          <Typography variant="body1" fontWeight="bold">Parallel</Typography>
                          <Typography variant="body2" color="text.secondary">
                            Execute multiple requests simultaneously
                          </Typography>
                        </Box>
                      }
                    />
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <FormControlLabel 
                      value={ExecutionMode.Conditional} 
                      control={<Radio />} 
                      label={
                        <Box>
                          <Typography variant="body1" fontWeight="bold">Conditional</Typography>
                          <Typography variant="body2" color="text.secondary">
                            Execute based on conditions
                          </Typography>
                        </Box>
                      }
                    />
                  </Grid>
                </Grid>
              </RadioGroup>
            </FormControl>
          </Paper>

          {/* Parallel Configuration */}
          {executionMode === ExecutionMode.Parallel && (
            <Paper elevation={1} sx={{ p: 3, bgcolor: 'blue.50' }}>
              <Typography variant="h6" gutterBottom>Parallel Execution Settings</Typography>
              <Grid container spacing={2}>
                <Grid item xs={12} md={6}>
                  <TextField
                    label="Max Parallel Requests"
                    type="number"
                    value={maxParallelRequests}
                    onChange={(e) => setMaxParallelRequests(parseInt(e.target.value) || 5)}
                    inputProps={{ min: 1, max: 20 }}
                    fullWidth
                    helperText="Number of requests to run simultaneously"
                  />
                </Grid>
                <Grid item xs={12} md={6}>
                  <TextField
                    label="Timeout (ms)"
                    type="number"
                    value={timeoutMs}
                    onChange={(e) => setTimeoutMs(parseInt(e.target.value) || 30000)}
                    inputProps={{ min: 1000 }}
                    fullWidth
                    helperText="Request timeout in milliseconds"
                  />
                </Grid>
              </Grid>
            </Paper>
          )}

          {/* Conditional Configuration */}
          {executionMode === ExecutionMode.Conditional && (
            <Paper elevation={1} sx={{ p: 3, bgcolor: 'orange.50' }}>
              <Typography variant="h6" gutterBottom>Conditional Logic</Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                Define conditions for each request. Use JSONPath expressions like $.status === 200
              </Typography>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                {integration.requests.map((request, index) => (
                  <TextField
                    key={request.id}
                    label={`Condition for "${request.name}"`}
                    value={conditions[request.id] || ''}
                    onChange={(e) => handleConditionChange(request.id, e.target.value)}
                    fullWidth
                    placeholder="e.g., $.status === 200 || $.data.length > 0"
                    helperText={`JSONPath condition for request ${index + 1}`}
                  />
                ))}
              </Box>
            </Paper>
          )}

          {/* OAuth 2.0 Configuration */}
          <Paper elevation={1} sx={{ p: 3, bgcolor: 'green.50' }}>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
              <Security sx={{ mr: 1 }} />
              <Typography variant="h6">OAuth 2.0 Authentication</Typography>
              <Switch
                checked={enableOAuth}
                onChange={(e) => setEnableOAuth(e.target.checked)}
                sx={{ ml: 'auto' }}
              />
            </Box>

            {enableOAuth && (
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                <Typography variant="body2" color="text.secondary">
                  Automatically handle OAuth 2.0 token flow for authenticated requests
                </Typography>
                
                {oauthToken ? (
                  <Alert severity="success">
                    <Typography variant="body2">
                      🔐 OAuth token obtained: {oauthToken.substring(0, 20)}...
                    </Typography>
                  </Alert>
                ) : (
                  <Button
                    variant="outlined"
                    onClick={handleOAuthFlow}
                    disabled={oauthLoading}
                    startIcon={oauthLoading ? <CircularProgress size={20} /> : <Security />}
                    sx={{ alignSelf: 'flex-start' }}
                  >
                    {oauthLoading ? 'Authenticating...' : 'Start OAuth Flow'}
                  </Button>
                )}

                {oauthError && (
                  <Alert severity="error">
                    {oauthError}
                  </Alert>
                )}
              </Box>
            )}
          </Paper>

          {/* Retry Configuration */}
          <Paper elevation={1} sx={{ p: 3, bgcolor: 'purple.50' }}>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
              <Refresh sx={{ mr: 1 }} />
              <Typography variant="h6">Automatic Retries</Typography>
              <Switch
                checked={enableRetries}
                onChange={(e) => setEnableRetries(e.target.checked)}
                sx={{ ml: 'auto' }}
              />
            </Box>

            {enableRetries && (
              <Grid container spacing={2}>
                <Grid item xs={12} md={6}>
                  <TextField
                    label="Max Retries"
                    type="number"
                    value={maxRetries}
                    onChange={(e) => setMaxRetries(parseInt(e.target.value) || 3)}
                    inputProps={{ min: 1, max: 10 }}
                    fullWidth
                    helperText="Maximum number of retry attempts"
                  />
                </Grid>
                <Grid item xs={12} md={6}>
                  <TextField
                    label="Delay Between Retries (ms)"
                    type="number"
                    value={delayBetweenRetriesMs}
                    onChange={(e) => setDelayBetweenRetriesMs(parseInt(e.target.value) || 1000)}
                    inputProps={{ min: 100 }}
                    fullWidth
                    helperText="Base delay between retry attempts"
                  />
                </Grid>
                <Grid item xs={12} md={6}>
                  <FormGroup>
                    <FormControlLabel
                      control={
                        <Checkbox
                          checked={exponentialBackoff}
                          onChange={(e) => setExponentialBackoff(e.target.checked)}
                        />
                      }
                      label="Exponential Backoff"
                    />
                    <Typography variant="body2" color="text.secondary">
                      Increase delay exponentially with each retry
                    </Typography>
                  </FormGroup>
                </Grid>
                <Grid item xs={12} md={6}>
                  <TextField
                    label="Retry on Status Codes"
                    value={retryOnStatusCodes}
                    onChange={(e) => setRetryOnStatusCodes(e.target.value)}
                    fullWidth
                    helperText="Comma-separated list of HTTP status codes"
                    placeholder="500,502,503,504"
                  />
                </Grid>
              </Grid>
            )}
          </Paper>

          {/* Placeholders */}
          {availablePlaceholders.length > 0 && (
            <Paper elevation={1} sx={{ p: 3 }}>
              <Typography variant="h6" gutterBottom>Placeholders</Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                Define values for placeholders found in your requests
              </Typography>
              <Grid container spacing={2}>
                {availablePlaceholders.map((placeholder) => (
                  <Grid item xs={12} md={6} key={placeholder}>
                    <TextField
                      label={placeholder}
                      value={placeholders[placeholder] || ''}
                      onChange={(e) => handlePlaceholderChange(placeholder, e.target.value)}
                      fullWidth
                      helperText={`Value for {{${placeholder}}}`}
                    />
                  </Grid>
                ))}
              </Grid>
            </Paper>
          )}

          {error && (
            <Alert severity="error">
              {error}
            </Alert>
          )}
        </Box>
      </DialogContent>

      <Divider />

      <DialogActions sx={{ p: 3, pt: 2 }}>
        <Button onClick={onClose} size="large">
          Cancel
        </Button>
        <Button
          onClick={handleExecute}
          variant="contained"
          size="large"
          disabled={loading}
          startIcon={loading ? <CircularProgress size={20} /> : <PlayArrow />}
          sx={{ minWidth: 200 }}
        >
          {loading ? 'Executing...' : '🚀 Execute Integration'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default EnhancedExecutionView; 