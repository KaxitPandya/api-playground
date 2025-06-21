import React, { useState, useEffect } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import {
  Button,
  Container,
  Typography,
  Paper,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Divider,
  Box,
  Menu,
  MenuItem,
  Alert,
  Snackbar,
} from '@mui/material';
import { Add, Delete, Edit, PlayArrow, SmartToy, CloudDownload, MoreVert } from '@mui/icons-material';
import { IntegrationService } from '../services/api';
import { Integration, AIGenerationResponse, OpenAPIImportResponse } from '../models';
import AIGenerationForm from './AIGenerationForm';
import OpenAPIImportForm from './OpenAPIImportForm';

const IntegrationList: React.FC = () => {
  const [integrations, setIntegrations] = useState<Integration[]>([]);
  const [open, setOpen] = useState(false);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  // New feature states
  const [showAIForm, setShowAIForm] = useState(false);
  const [showOpenAPIForm, setShowOpenAPIForm] = useState(false);
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  useEffect(() => {
    loadIntegrations();
  }, []);

  const loadIntegrations = async () => {
    try {
      setLoading(true);
      const data = await IntegrationService.getAll();
      setIntegrations(data);
      setError(null);
    } catch (err) {
      setError('Failed to load integrations');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleOpen = () => {
    setOpen(true);
    setAnchorEl(null);
  };

  const handleClose = () => {
    setOpen(false);
    setName('');
    setDescription('');
  };

  const handleSubmit = async () => {
    try {
      await IntegrationService.create({
        name,
        description,
      });
      handleClose();
      loadIntegrations();
    } catch (err) {
      setError('Failed to create integration');
      console.error(err);
    }
  };

  const handleDelete = async (id: string) => {
    if (window.confirm('Are you sure you want to delete this integration?')) {
      try {
        await IntegrationService.delete(id);
        loadIntegrations();
      } catch (err) {
        setError('Failed to delete integration');
        console.error(err);
      }
    }
  };

  const handleMenuClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
  };

  const handleAIGenerated = async (response: AIGenerationResponse) => {
    try {
      // The integration is already created by the AI service
      setShowAIForm(false);
      setSuccessMessage(`Successfully generated integration "${response.integration.name}" with ${response.integration.requests.length} requests!`);
      
      // Add a small delay to ensure the integration is fully saved before refreshing
      setTimeout(() => {
        loadIntegrations();
      }, 500);
    } catch (err) {
      setError('Failed to save AI-generated integration');
      console.error(err);
    }
  };

  const handleOpenAPIImported = async (response: OpenAPIImportResponse) => {
    try {
      // The integration is already created by the import service
      setShowOpenAPIForm(false);
      
      const operationsCount = response.importedCount || response.integration.requests.length || 0;
      setSuccessMessage(`Successfully imported integration "${response.integration.name}" with ${operationsCount} operations!`);
      
      // Add a small delay to ensure the integration is fully saved before refreshing
      setTimeout(() => {
        loadIntegrations();
      }, 500);
    } catch (err) {
      setError('Failed to save imported integration');
      console.error(err);
    }
  };

  return (
    <Container maxWidth="md" sx={{ mt: 4 }}>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
        <Typography variant="h4">API Integrations</Typography>
        <Box>
          <Button
            variant="contained"
            color="primary"
            endIcon={<MoreVert />}
            onClick={handleMenuClick}
          >
            Create Integration
          </Button>
          <Menu
            anchorEl={anchorEl}
            open={Boolean(anchorEl)}
            onClose={handleMenuClose}
          >
            <MenuItem onClick={handleOpen}>
              <Add sx={{ mr: 1 }} />
              Manual Integration
            </MenuItem>
            <MenuItem onClick={() => { setShowAIForm(true); handleMenuClose(); }}>
              <SmartToy sx={{ mr: 1 }} />
              AI Generate Integration
            </MenuItem>
            <MenuItem onClick={() => { setShowOpenAPIForm(true); handleMenuClose(); }}>
              <CloudDownload sx={{ mr: 1 }} />
              Import from OpenAPI
            </MenuItem>
          </Menu>
        </Box>
      </Box>

      {error && (
        <Paper sx={{ p: 2, mb: 2, bgcolor: 'error.light' }}>
          <Typography color="error">{error}</Typography>
        </Paper>
      )}

      <Paper elevation={2}>
        {loading ? (
          <Box p={3} textAlign="center">
            <Typography>Loading...</Typography>
          </Box>
        ) : integrations.length === 0 ? (
          <Box p={3} textAlign="center">
            <Typography variant="h6" color="textSecondary" gutterBottom>
              No integrations found
            </Typography>
            <Typography variant="body2" color="textSecondary" paragraph>
              Get started by creating your first integration:
            </Typography>
            <Box sx={{ mt: 2, display: 'flex', gap: 1, justifyContent: 'center', flexWrap: 'wrap' }}>
              <Button
                variant="outlined"
                size="small"
                startIcon={<Add />}
                onClick={handleOpen}
              >
                Manual
              </Button>
              <Button
                variant="outlined"
                size="small"
                startIcon={<SmartToy />}
                onClick={() => setShowAIForm(true)}
                color="secondary"
              >
                AI Generate
              </Button>
              <Button
                variant="outlined"
                size="small"
                startIcon={<CloudDownload />}
                onClick={() => setShowOpenAPIForm(true)}
                color="info"
              >
                Import OpenAPI
              </Button>
            </Box>
          </Box>
        ) : (
          <List>
            {integrations.map((integration, index) => (
              <React.Fragment key={integration.id}>
                {index > 0 && <Divider />}
                <ListItem>
                  <ListItemText
                    primary={integration.name}
                    secondary={
                      <Box>
                        <Typography variant="body2" color="textSecondary">
                          {integration.description || 'No description'}
                        </Typography>
                        <Typography variant="caption" color="textSecondary">
                          {integration.requests.length} request{integration.requests.length !== 1 ? 's' : ''} • 
                          Created {new Date(integration.createdAt).toLocaleDateString()}
                        </Typography>
                      </Box>
                    }
                  />
                  <ListItemSecondaryAction>
                    <IconButton 
                      component={RouterLink} 
                      to={`/integrations/${integration.id}/execute`}
                      color="success"
                      title="Execute Integration"
                    >
                      <PlayArrow />
                    </IconButton>
                    <IconButton 
                      component={RouterLink} 
                      to={`/integrations/${integration.id}`}
                      color="primary"
                      title="Edit Integration"
                    >
                      <Edit />
                    </IconButton>
                    <IconButton 
                      edge="end" 
                      color="error"
                      onClick={() => handleDelete(integration.id)}
                      title="Delete Integration"
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

      {/* Manual Creation Dialog */}
      <Dialog open={open} onClose={handleClose}>
        <DialogTitle>New Integration</DialogTitle>
        <DialogContent>
          <TextField
            autoFocus
            margin="dense"
            label="Integration Name"
            fullWidth
            variant="outlined"
            value={name}
            onChange={(e) => setName(e.target.value)}
          />
          <TextField
            margin="dense"
            label="Description (Optional)"
            fullWidth
            variant="outlined"
            multiline
            rows={3}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClose}>Cancel</Button>
          <Button 
            onClick={handleSubmit} 
            variant="contained" 
            color="primary"
            disabled={!name.trim()}
          >
            Create
          </Button>
        </DialogActions>
      </Dialog>

      {/* AI Generation Form */}
      <AIGenerationForm
        open={showAIForm}
        onIntegrationGenerated={handleAIGenerated}
        onClose={() => setShowAIForm(false)}
      />

      {/* OpenAPI Import Form */}
      <OpenAPIImportForm
        open={showOpenAPIForm}
        onIntegrationImported={handleOpenAPIImported}
        onClose={() => setShowOpenAPIForm(false)}
      />

      {/* Success Snackbar */}
      <Snackbar
        open={Boolean(successMessage)}
        autoHideDuration={6000}
        onClose={() => setSuccessMessage(null)}
      >
        <Alert 
          onClose={() => setSuccessMessage(null)} 
          severity="success" 
          sx={{ width: '100%' }}
        >
          {successMessage}
        </Alert>
      </Snackbar>
    </Container>
  );
};

export default IntegrationList;
