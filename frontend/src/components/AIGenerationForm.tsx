import React, { useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  Typography,
  Box,
  FormControlLabel,
  Checkbox,
  Chip,
  IconButton,
  Alert,
  CircularProgress,
  Divider,
  Accordion,
  AccordionSummary,
  AccordionDetails,
} from '@mui/material';
import {
  Close,
  SmartToy,
  Add,
  Remove,
  ExpandMore,
  AutoAwesome,
} from '@mui/icons-material';
import { AIService } from '../services/api';
import { AIGenerationRequest, AIGenerationResponse } from '../models';

interface AIGenerationFormProps {
  open: boolean;
  onIntegrationGenerated: (response: AIGenerationResponse) => void;
  onClose: () => void;
}

const AIGenerationForm: React.FC<AIGenerationFormProps> = ({ 
  open,
  onIntegrationGenerated, 
  onClose 
}) => {
  const [description, setDescription] = useState('');
  const [targetEndpoints, setTargetEndpoints] = useState<string[]>([]);
  const [newEndpoint, setNewEndpoint] = useState('');
  const [includeAuthentication, setIncludeAuthentication] = useState(false);
  const [generatePlaceholders, setGeneratePlaceholders] = useState(true);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const exampleDescriptions = [
    "Fetch user data from GitHub API and create a new issue in a repository",
    "Get weather data from OpenWeatherMap API and send notifications",
    "Manage users in a REST API with CRUD operations",
    "Fetch posts from JSONPlaceholder API and filter by user ID"
  ];

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const request: AIGenerationRequest = {
        description,
        examples: targetEndpoints.length > 0 ? targetEndpoints : undefined,
        authenticationType: includeAuthentication ? "BearerToken" : undefined,
      };

      const response = await AIService.generateIntegration(request);
      onIntegrationGenerated(response);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to generate integration');
    } finally {
      setLoading(false);
    }
  };

  const addEndpoint = () => {
    if (newEndpoint.trim() && !targetEndpoints.includes(newEndpoint.trim())) {
      setTargetEndpoints([...targetEndpoints, newEndpoint.trim()]);
      setNewEndpoint('');
    }
  };

  const removeEndpoint = (endpoint: string) => {
    setTargetEndpoints(targetEndpoints.filter(e => e !== endpoint));
  };

  const setExample = (example: string) => {
    setDescription(example);
  };

  const handleClose = () => {
    setDescription('');
    setTargetEndpoints([]);
    setNewEndpoint('');
    setIncludeAuthentication(false);
    setGeneratePlaceholders(true);
    setError(null);
    onClose();
  };

  return (
    <Dialog 
      open={open} 
      onClose={handleClose}
      maxWidth="md"
      fullWidth
      PaperProps={{
        sx: { borderRadius: 3, maxHeight: '90vh' }
      }}
    >
      <DialogTitle sx={{ pb: 1 }}>
        <Box display="flex" alignItems="center" justifyContent="space-between">
          <Box display="flex" alignItems="center">
            <SmartToy color="secondary" sx={{ mr: 1.5, fontSize: 28 }} />
            <Typography variant="h5" component="div" fontWeight="bold">
              Generate Integration with AI
            </Typography>
          </Box>
          <IconButton onClick={handleClose} size="small">
            <Close />
          </IconButton>
        </Box>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
          Describe what your integration should do and let AI create the requests for you
        </Typography>
      </DialogTitle>

      <Divider />

      <DialogContent sx={{ pt: 2 }}>
        <Box component="form" onSubmit={handleSubmit} sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
          
          {/* Description Section */}
          <Box>
            <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center' }}>
              <AutoAwesome sx={{ mr: 1, color: 'primary.main' }} />
              Describe Your Integration
            </Typography>
            
            <TextField
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              required
              fullWidth
              multiline
              rows={4}
              variant="outlined"
              placeholder="Example: I want to fetch user data from GitHub API and create a new issue in a repository. Include endpoints for getting user profile and creating issues."
              helperText="Be specific about the APIs and operations you need. The more detail, the better the result!"
              sx={{ mb: 2 }}
            />

            {/* Example Descriptions */}
            <Accordion elevation={0} sx={{ border: '1px solid', borderColor: 'divider' }}>
              <AccordionSummary expandIcon={<ExpandMore />}>
                <Typography variant="body2" color="primary">
                  💡 See example descriptions
                </Typography>
              </AccordionSummary>
              <AccordionDetails>
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                  {exampleDescriptions.map((example, index) => (
                    <Box key={index} sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Typography variant="body2" sx={{ flex: 1 }}>
                        {example}
                      </Typography>
                                             <Button 
                         size="small" 
                         onClick={() => setExample(example)}
                         variant="outlined"
                       >
                         Use This
                       </Button>
                    </Box>
                  ))}
                </Box>
              </AccordionDetails>
            </Accordion>
          </Box>

          {/* Target Endpoints Section */}
          <Box>
            <Typography variant="h6" gutterBottom>
              Target Endpoints (Optional)
            </Typography>
            
            <Box sx={{ display: 'flex', gap: 1, mb: 2 }}>
              <TextField
                value={newEndpoint}
                onChange={(e) => setNewEndpoint(e.target.value)}
                placeholder="api.github.com/user"
                size="small"
                sx={{ flex: 1 }}
                onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addEndpoint())}
              />
              <Button 
                variant="outlined" 
                onClick={addEndpoint}
                disabled={!newEndpoint.trim()}
                startIcon={<Add />}
              >
                Add
              </Button>
            </Box>

            {targetEndpoints.length > 0 && (
              <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                {targetEndpoints.map((endpoint) => (
                  <Chip
                    key={endpoint}
                    label={endpoint}
                    onDelete={() => removeEndpoint(endpoint)}
                    deleteIcon={<Remove />}
                    variant="outlined"
                    color="primary"
                  />
                ))}
              </Box>
            )}
          </Box>

          {/* Options Section */}
          <Box>
            <Typography variant="h6" gutterBottom>
              Generation Options
            </Typography>
            
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
              <FormControlLabel
                control={
                  <Checkbox
                    checked={includeAuthentication}
                    onChange={(e) => setIncludeAuthentication(e.target.checked)}
                    color="primary"
                  />
                }
                label="Include Authentication (Bearer tokens, API keys)"
              />
              
              <FormControlLabel
                control={
                  <Checkbox
                    checked={generatePlaceholders}
                    onChange={(e) => setGeneratePlaceholders(e.target.checked)}
                    color="primary"
                  />
                }
                label="Generate Placeholders (Dynamic values like {{userId}})"
              />
            </Box>
          </Box>

          {/* Error Alert */}
          {error && (
            <Alert severity="error" onClose={() => setError(null)}>
              {error}
            </Alert>
          )}
        </Box>
      </DialogContent>

      <Divider />

      <DialogActions sx={{ p: 3, pt: 2 }}>
        <Button onClick={handleClose} size="large">
          Cancel
        </Button>
        <Button
          onClick={handleSubmit}
          variant="contained"
          size="large"
          disabled={loading || !description.trim()}
          startIcon={loading ? <CircularProgress size={20} /> : <SmartToy />}
          sx={{ minWidth: 180 }}
        >
          {loading ? 'Generating...' : 'Generate Integration'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default AIGenerationForm; 