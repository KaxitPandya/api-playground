import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link as RouterLink } from 'react-router-dom';
import {
  Button,
  Container,
  Typography,
  Paper,
  Box,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  IconButton,
  TextField,
  Divider,
  Chip,
} from '@mui/material';
import { 
  Add, 
  Delete, 
  Edit, 
  PlayArrow, 
  ArrowUpward, 
  ArrowDownward,
  Save
} from '@mui/icons-material';
import { Integration, Request } from '../models';
import { IntegrationService, RequestService } from '../services/api';

const IntegrationDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [integration, setIntegration] = useState<Integration | null>(null);
  const [requests, setRequests] = useState<Request[]>([]);
  const [isEditing, setIsEditing] = useState(false);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (id) {
      loadIntegration(id);
    }
  }, [id]);

  const loadIntegration = async (integrationId: string) => {
    try {
      setLoading(true);
      const data = await IntegrationService.getById(integrationId);
      setIntegration(data);
      setRequests(data.requests);
      setName(data.name);
      setDescription(data.description || '');
      setError(null);
    } catch (err) {
      setError('Failed to load integration');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleSave = async () => {
    if (!integration) return;
    
    try {
      await IntegrationService.update({
        ...integration,
        name,
        description
      });
      setIsEditing(false);
      loadIntegration(integration.id);
    } catch (err) {
      setError('Failed to update integration');
      console.error(err);
    }
  };

  const handleDeleteRequest = async (requestId: string) => {
    if (window.confirm('Are you sure you want to delete this request?')) {
      try {
        await RequestService.delete(requestId);
        if (id) {
          loadIntegration(id);
        }
      } catch (err) {
        setError('Failed to delete request');
        console.error(err);
      }
    }
  };

  const getMethodColor = (method: string) => {
    switch (method) {
      case 'GET': return 'success';
      case 'POST': return 'info';
      case 'PUT': return 'warning';
      case 'DELETE': return 'error';
      case 'PATCH': return 'secondary';
      default: return 'default';
    }
  };

  if (loading) {
    return (
      <Container maxWidth="md" sx={{ mt: 4 }}>
        <Typography>Loading...</Typography>
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
    <Container maxWidth="md" sx={{ mt: 4 }}>
      {error && (
        <Paper sx={{ p: 2, mb: 2, bgcolor: 'error.light' }}>
          <Typography color="error">{error}</Typography>
        </Paper>
      )}
      
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
        {isEditing ? (
          <Box flex={1}>
            <TextField
              label="Integration Name"
              variant="outlined"
              fullWidth
              value={name}
              onChange={(e) => setName(e.target.value)}
              sx={{ mb: 2 }}
            />
            <TextField
              label="Description"
              variant="outlined"
              fullWidth
              multiline
              rows={2}
              value={description}
              onChange={(e) => setDescription(e.target.value)}
            />
          </Box>
        ) : (
          <Box>
            <Typography variant="h4">{integration?.name}</Typography>
            <Typography variant="body1" color="textSecondary">
              {integration?.description || 'No description'}
            </Typography>
          </Box>
        )}
        
        <Box>
          {isEditing ? (
            <Button 
              variant="contained" 
              startIcon={<Save />}
              onClick={handleSave}
              sx={{ ml: 1 }}
            >
              Save
            </Button>
          ) : (
            <Button 
              variant="outlined"
              onClick={() => setIsEditing(true)}
              sx={{ ml: 1 }}
            >
              Edit
            </Button>
          )}
          <Button 
            variant="contained" 
            color="success"
            component={RouterLink}
            to={`/integrations/${integration?.id}/execute`}
            startIcon={<PlayArrow />}
            sx={{ ml: 1 }}
          >
            Run All
          </Button>
        </Box>
      </Box>

      <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
        <Typography variant="h5">Requests</Typography>
        <Button 
          variant="contained" 
          startIcon={<Add />}
          component={RouterLink}
          to={`/integrations/${integration?.id}/requests/new`}
        >
          Add Request
        </Button>
      </Box>
      
      <Paper elevation={2}>
        {requests.length === 0 ? (
          <Box p={3} textAlign="center">
            <Typography>No requests in this integration. Add one to get started!</Typography>
          </Box>
        ) : (
          <List>
            {requests.map((request, index) => (
              <React.Fragment key={request.id}>
                {index > 0 && <Divider />}
                <ListItem>
                  <ListItemText
                    primary={
                      <Box display="flex" alignItems="center">
                        <Chip 
                          label={request.method} 
                          size="small" 
                          color={getMethodColor(request.method) as any}
                          sx={{ mr: 1 }}
                        />
                        {request.name}
                      </Box>
                    }
                    secondary={request.url}
                  />
                  <ListItemSecondaryAction>
                    <IconButton 
                      component={RouterLink}
                      to={`/requests/${request.id}/edit`}
                      color="primary"
                    >
                      <Edit />
                    </IconButton>
                    <IconButton 
                      edge="end" 
                      color="error"
                      onClick={() => handleDeleteRequest(request.id)}
                    >
                      <Delete />
                    </IconButton>
                  </ListItemSecondaryAction>
                </ListItem>
              </React.Fragment>
            ))}
          </List>
        )}
      </Paper>
    </Container>
  );
};

export default IntegrationDetail;
