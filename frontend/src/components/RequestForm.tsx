import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Container,
  Typography,
  Paper,
  TextField,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Box,
  IconButton,
  Grid,
  Divider,
  Alert,
} from '@mui/material';
import { Delete, Add } from '@mui/icons-material';
import { RequestService } from '../services/api';
import { HttpMethod, Request } from '../models';

interface HeaderRow {
  key: string;
  value: string;
  id: number;
}

const RequestForm: React.FC = () => {
  const { id, requestId } = useParams<{ id: string; requestId?: string }>();
  const navigate = useNavigate();
  const location = window.location.pathname;
  
  // Determine if we're in edit mode based on the URL pattern
  const isEditMode = location.includes('/edit');
  const actualRequestId = isEditMode ? id : requestId; // For edit: id is requestId, for new: requestId is the param
  const integrationId = isEditMode ? null : id; // For edit: we'll get integrationId from loaded request data
  
  // Form state
  const [name, setName] = useState('');
  const [method, setMethod] = useState<HttpMethod>(HttpMethod.GET);
  const [url, setUrl] = useState('');
  const [body, setBody] = useState('');
  const [headers, setHeaders] = useState<HeaderRow[]>([
    { key: '', value: '', id: Date.now() }
  ]);
  
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [requestData, setRequestData] = useState<Request | null>(null);

  useEffect(() => {
    if (isEditMode && actualRequestId) {
      loadRequest(actualRequestId);
    }
  }, [isEditMode, actualRequestId]);

  const loadRequest = async (id: string) => {
    try {
      setLoading(true);
      const request = await RequestService.getById(id);
      setRequestData(request);
      
      // Populate form values
      setName(request.name);
      setMethod(request.method);
      setUrl(request.url);
      setBody(request.body || '');
      
      // Convert headers object to array form
      const headerRows: HeaderRow[] = Object.entries(request.headers).map(
        ([key, value], index) => ({
          key,
          value,
          id: index,
        })
      );
      
      setHeaders(headerRows.length > 0 ? headerRows : [{ key: '', value: '', id: Date.now() }]);
      
      setError(null);
    } catch (err) {
      setError('Failed to load request data');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleHeaderChange = (id: number, field: 'key' | 'value', value: string) => {
    setHeaders(
      headers.map((header) =>
        header.id === id ? { ...header, [field]: value } : header
      )
    );
  };

  const addHeader = () => {
    setHeaders([...headers, { key: '', value: '', id: Date.now() }]);
  };

  const removeHeader = (id: number) => {
    setHeaders(headers.filter((header) => header.id !== id));
  };

  const handleSubmit = async () => {
    try {
      setLoading(true);
      
      // Convert headers from array to object
      const headerObject: Record<string, string> = {};
      headers.forEach((header) => {
        if (header.key.trim()) {
          headerObject[header.key.trim()] = header.value;
        }
      });

      if (isEditMode && requestData) {
        // Update existing request
        await RequestService.update({
          ...requestData,
          name,
          method,
          url,
          body,
          headers: headerObject,
        });
        
        navigate(`/integrations/${requestData.integrationId}`);
      } else if (!isEditMode && integrationId) {
        // Create new request
        await RequestService.create({
          name,
          method,
          url,
          body,
          headers: headerObject,
          integrationId: integrationId,
          order: 0, // Will be set by backend
        });
        
        navigate(`/integrations/${integrationId}`);
      }
    } catch (err) {
      setError('Failed to save request');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };


  const handleAddBearerToken = () => {
    // Check if Authorization header already exists
    const authHeader = headers.find(h => h.key.toLowerCase() === 'authorization');
    
    if (authHeader) {
      // Update existing Authorization header
      handleHeaderChange(
        authHeader.id, 
        'value', 
        'Bearer '
      );
    } else {
      // Add new Authorization header
      setHeaders([
        ...headers, 
        { key: 'Authorization', value: 'Bearer ', id: Date.now() }
      ]);
    }
  };


  return (
    <Container maxWidth="md" sx={{ mt: 4 }}>
      <Typography variant="h4" sx={{ mb: 3 }}>
        {isEditMode ? 'Edit Request' : 'New Request'}
      </Typography>
      
      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      
      <Paper elevation={2} sx={{ p: 3 }}>
        <Grid container spacing={3}>
          <Grid item xs={12}>
            <TextField
              label="Request Name"
              variant="outlined"
              fullWidth
              required
              value={name}
              onChange={(e) => setName(e.target.value)}
            />
          </Grid>
          
          <Grid item xs={12} sm={3}>
            <FormControl fullWidth>
              <InputLabel id="method-select-label">Method</InputLabel>
              <Select
                labelId="method-select-label"
                value={method}
                label="Method"
                onChange={(e) => setMethod(e.target.value as HttpMethod)}
              >
                {Object.values(HttpMethod).map((m) => (
                  <MenuItem key={m} value={m}>
                    {m}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          
          <Grid item xs={12} sm={9}>
            <TextField
              label="URL"
              variant="outlined"
              fullWidth
              required
              value={url}
              onChange={(e) => setUrl(e.target.value)}
              placeholder="https://api.example.com/users/{{userId}}"
            />
          </Grid>
          
          <Grid item xs={12}>
            <Box display="flex" justifyContent="space-between" alignItems="center" mb={1}>
              <Typography variant="h6">Headers</Typography>
              <Box>
                <Button
                  size="small"
                  onClick={handleAddBearerToken}
                  sx={{ mr: 1 }}>
                  Add Bearer Token
                </Button>

                <Button
                  size="small"
                  startIcon={<Add />}
                  onClick={addHeader}
                >
                  Add Header
                </Button>
              </Box>
            </Box>
            
            {headers.map((header, index) => (
              <Box key={header.id} sx={{ mb: 2, display: 'flex', gap: 1 }}>
                <TextField
                  label="Header Name"
                  variant="outlined"
                  size="small"
                  sx={{ flexBasis: '40%' }}
                  value={header.key}
                  onChange={(e) =>
                    handleHeaderChange(header.id, 'key', e.target.value)
                  }
                />
                <TextField
                  label="Value"
                  variant="outlined"
                  size="small"
                  sx={{ flexGrow: 1 }}
                  value={header.value}
                  onChange={(e) =>
                    handleHeaderChange(header.id, 'value', e.target.value)
                  }
                />
                <IconButton
                  color="error"
                  onClick={() => removeHeader(header.id)}
                  disabled={headers.length === 1 && index === 0}
                >
                  <Delete />
                </IconButton>
              </Box>
            ))}
          </Grid>
          
          {(method === HttpMethod.POST || 
            method === HttpMethod.PUT || 
            method === HttpMethod.PATCH) && (
            <Grid item xs={12}>
              <Typography variant="h6" sx={{ mb: 1 }}>Body</Typography>
              <TextField
                variant="outlined"
                fullWidth
                multiline
                rows={8}
                value={body}
                onChange={(e) => setBody(e.target.value)}
                placeholder='{
  "name": "{{name}}",
  "email": "{{email}}"
}'
              />
            </Grid>
          )}
        </Grid>
        
        <Divider sx={{ my: 3 }} />
        
        <Box display="flex" justifyContent="flex-end">
          <Button 
            variant="outlined" 
            onClick={() => navigate(-1)}
            sx={{ mr: 1 }}
          >
            Cancel
          </Button>
          <Button 
            variant="contained" 
            color="primary" 
            onClick={handleSubmit}
            disabled={!name || !url || loading}
          >
            {loading ? 'Saving...' : 'Save Request'}
          </Button>
        </Box>
      </Paper>
    </Container>
  );
};

export default RequestForm;
