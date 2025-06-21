import React, { useState, useEffect } from 'react';
import { useParams, Link as RouterLink } from 'react-router-dom';
import {
  Container,
  Typography,
  Paper,
  Box,
  Button,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  TextField,
  Chip,
  Divider,
  CircularProgress,
  Alert,
  IconButton,
} from '@mui/material';
import {
  ExpandMore,
  Add,
  Delete,
  PlayArrow,
  Settings,
} from '@mui/icons-material';
import { Integration, RequestResult, PlaceholderMap } from '../models';
import { IntegrationService, ExecutionService } from '../services/api';
import EnhancedExecutionView from './EnhancedExecutionView';

const ExecutionView: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [integration, setIntegration] = useState<Integration | null>(null);
  const [placeholders, setPlaceholders] = useState<{ key: string; value: string; id: number }[]>([
    { key: '', value: '', id: Date.now() }
  ]);
  const [results, setResults] = useState<RequestResult[]>([]);
  const [loading, setLoading] = useState(true);
  const [executing, setExecuting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showAdvancedExecution, setShowAdvancedExecution] = useState(false);

  console.log('ExecutionView: Component mounting with id:', id); // Debug log

  useEffect(() => {
    if (id) {
      console.log('ExecutionView: Loading integration with id:', id); // Debug log
      loadIntegration(id);
    }
  }, [id]);

  const loadIntegration = async (integrationId: string) => {
    try {
      setLoading(true);
      console.log('ExecutionView: Fetching integration...'); // Debug log
      const data = await IntegrationService.getById(integrationId);
      console.log('ExecutionView: Integration loaded:', data); // Debug log
      setIntegration(data);
      
      // Pre-populate placeholders based on URLs and bodies
      const placeholderMap = new Map<string, { key: string; value: string; id: number }>();
      
      data.requests.forEach(request => {
        // Extract placeholders from URL
        const urlMatches = request.url.match(/\{\{([^}]+)\}\}/g) || [];
        urlMatches.forEach(match => {
          const key = match.replace(/\{\{|\}\}/g, '');
          // Skip JSONPath expressions (they start with $ or $[)
          if (!key.startsWith('$')) {
            placeholderMap.set(key, { key, value: '', id: Date.now() + Math.random() });
          }
        });
        
        // Extract placeholders from headers
        for (const headerKey in request.headers) {
          const headerValue = request.headers[headerKey];
          const headerMatches = headerValue.match(/\{\{([^}]+)\}\}/g) || [];
          headerMatches.forEach(match => {
            const key = match.replace(/\{\{|\}\}/g, '');
            if (!key.startsWith('$')) {
              placeholderMap.set(key, { key, value: '', id: Date.now() + Math.random() });
            }
          });
        }
        
        // Extract placeholders from body if present
        if (request.body) {
          const bodyMatches = request.body.match(/\{\{([^}]+)\}\}/g) || [];
          bodyMatches.forEach(match => {
            const key = match.replace(/\{\{|\}\}/g, '');
            if (!key.startsWith('$')) {
              placeholderMap.set(key, { key, value: '', id: Date.now() + Math.random() });
            }
          });
        }
      });
      
      const extractedPlaceholders = Array.from(placeholderMap.values());
      if (extractedPlaceholders.length > 0) {
        setPlaceholders(extractedPlaceholders);
      }
      
      setError(null);
    } catch (err) {
      console.error('ExecutionView: Error loading integration:', err); // Debug log
      setError('Failed to load integration: ' + (err instanceof Error ? err.message : 'Unknown error'));
    } finally {
      setLoading(false);
    }
  };

  const handlePlaceholderChange = (id: number, field: 'key' | 'value', value: string) => {
    setPlaceholders(
      placeholders.map((p: any) => (p.id === id ? { ...p, [field]: value } : p))
    );
  };

  const addPlaceholder = () => {
    setPlaceholders([...placeholders, { key: '', value: '', id: Date.now() }]);
  };

  const removePlaceholder = (id: number) => {
    setPlaceholders(placeholders.filter((p: any) => p.id !== id));
  };

  const executeIntegration = async () => {
    if (!id) return;
    
    try {
      setExecuting(true);
      
      // Build placeholders object in the correct format
      const placeholderValues: { [key: string]: string } = {};
      placeholders.forEach((p: any) => {
        if (p.key.trim()) {
          placeholderValues[p.key.trim()] = p.value;
        }
      });
      
      const placeholderMap: PlaceholderMap = { Values: placeholderValues };
      
      const execResults = await ExecutionService.executeIntegration(id, placeholderMap);
      setResults(execResults);
      setError(null);
    } catch (err) {
      setError('Failed to execute integration');
      console.error(err);
    } finally {
      setExecuting(false);
    }
  };

  const handleAdvancedExecutionComplete = (results: RequestResult[]) => {
    setResults(results);
    setShowAdvancedExecution(false);
  };

  const getStatusColor = (status: number) => {
    if (status >= 200 && status < 300) return 'success';
    if (status >= 300 && status < 400) return 'info';
    if (status >= 400 && status < 500) return 'warning';
    if (status >= 500) return 'error';
    return 'default';
  };

  const parseResponse = (responseString?: string) => {
    if (!responseString) return null;
    
    try {
      return JSON.parse(responseString);
    } catch (e) {
      return responseString;
    }
  };

  if (loading) {
    return (
      <Container maxWidth="md" sx={{ mt: 4, textAlign: 'center' }}>
        <CircularProgress />
        <Typography sx={{ mt: 2 }}>Loading integration...</Typography>
      </Container>
    );
  }

  if (!integration && !loading) {
    return (
      <Container maxWidth="md" sx={{ mt: 4 }}>
        <Typography color="error">Integration not found</Typography>
        <Button 
          variant="contained" 
          component={RouterLink} 
          to="/"
          sx={{ mt: 2 }}
        >
          Back to Integrations
        </Button>
      </Container>
    );
  }

  return (
    <Container maxWidth="md" sx={{ mt: 4, mb: 4 }}>
      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4">Execute: {integration?.name}</Typography>
        <Box display="flex" gap={1}>
          <Button 
            variant="outlined"
            component={RouterLink}
            to={`/integrations/${id}`}
          >
            Edit Integration
          </Button>
        </Box>
      </Box>
      
      <Paper elevation={2} sx={{ p: 3, mb: 3 }}>
        <Typography variant="h6" gutterBottom>
          Placeholders
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Define values to replace placeholders in your requests. Use JSONPath like $.data.id to extract from previous responses.
        </Typography>
        
        {placeholders.map((placeholder: any) => (
          <Box key={placeholder.id} sx={{ mb: 2, display: 'flex', gap: 1 }}>
            <TextField
              label="Placeholder Name"
              variant="outlined"
              size="small"
              sx={{ flexBasis: '40%' }}
              value={placeholder.key}
              onChange={(e: any) => handlePlaceholderChange(placeholder.id, 'key', e.target.value)}
            />
            <TextField
              label="Value"
              variant="outlined"
              size="small"
              sx={{ flexGrow: 1 }}
              value={placeholder.value}
              onChange={(e: any) => handlePlaceholderChange(placeholder.id, 'value', e.target.value)}
            />
            <IconButton
              color="error"
              onClick={() => removePlaceholder(placeholder.id)}
            >
              <Delete />
            </IconButton>
          </Box>
        ))}
        
        <Button startIcon={<Add />} onClick={addPlaceholder} sx={{ mt: 1 }}>
          Add Placeholder
        </Button>
        
        <Box display="flex" justifyContent="space-between" alignItems="center" sx={{ mt: 3 }}>
          <Button
            variant="outlined"
            startIcon={<Settings />}
            onClick={() => setShowAdvancedExecution(true)}
            color="secondary"
          >
            Advanced Execution
          </Button>
          
          <Button
            variant="contained"
            color="primary"
            startIcon={executing ? <CircularProgress size={20} color="inherit" /> : <PlayArrow />}
            disabled={executing}
            onClick={executeIntegration}
          >
            {executing ? 'Executing...' : 'Execute Integration'}
          </Button>
        </Box>
      </Paper>

      {results.length > 0 && (
        <Box>
          <Typography variant="h5" sx={{ mb: 2 }}>
            Results
          </Typography>
          
          {results.map((result: any, index: any) => (
            <Accordion key={result.id} defaultExpanded={index === 0} sx={{ mb: 2 }}>
              <AccordionSummary expandIcon={<ExpandMore />}>
                <Box display="flex" alignItems="center" width="100%">
                  <Typography sx={{ flexGrow: 1 }}>{result.request?.name}</Typography>
                  <Box display="flex" alignItems="center" gap={1}>
                    <Chip
                      label={`${result.statusCode}`}
                      color={getStatusColor(result.statusCode) as any}
                      size="small"
                    />
                    <Typography variant="body2">
                      {result.responseTimeMs.toFixed(2)} ms
                    </Typography>
                    {result.retryCount && result.retryCount > 0 && (
                      <Chip
                        label={`${result.retryCount} retries`}
                        color="warning"
                        size="small"
                      />
                    )}
                  </Box>
                </Box>
              </AccordionSummary>
              <AccordionDetails>
                <Divider sx={{ mb: 2 }} />
                <Box sx={{ overflowX: 'auto' }}>
                  {result.response ? (
                    <Paper
                      sx={{
                        backgroundColor: '#f5f5f5',
                        padding: 2,
                        borderRadius: 1,
                        fontFamily: 'monospace',
                        fontSize: '0.875rem',
                        whiteSpace: 'pre-wrap',
                        maxHeight: '400px',
                        overflow: 'auto'
                      }}
                    >
                      {typeof parseResponse(result.response) === 'object' 
                        ? JSON.stringify(parseResponse(result.response), null, 2)
                        : result.response
                      }
                    </Paper>
                  ) : (
                    <Typography>No response data</Typography>
                  )}
                </Box>
              </AccordionDetails>
            </Accordion>
          ))}
        </Box>
      )}

      {/* Advanced Execution Modal */}
      {showAdvancedExecution && integration && (
        <EnhancedExecutionView
          integration={integration}
          onExecutionComplete={handleAdvancedExecutionComplete}
          onClose={() => setShowAdvancedExecution(false)}
        />
      )}
    </Container>
  );
};

export default ExecutionView;
