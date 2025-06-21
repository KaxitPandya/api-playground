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
  RadioGroup,
  Radio,
  Chip,
  IconButton,
  Alert,
  CircularProgress,
  Divider,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Checkbox,
  Paper,
  Tabs,
  Tab,
  Accordion,
  AccordionSummary,
  AccordionDetails,
} from '@mui/material';
import {
  Close,
  CloudDownload,
  Upload,
  Link as LinkIcon,
  CheckCircle,
  RadioButtonUnchecked,
  ExpandMore,
  Api,
  Refresh,
} from '@mui/icons-material';
import { OpenAPIService } from '../services/api';
import { OpenAPIImportResponse } from '../models';

interface OpenAPIImportFormProps {
  open: boolean;
  onIntegrationImported: (response: OpenAPIImportResponse) => void;
  onClose: () => void;
}

const OpenAPIImportForm: React.FC<OpenAPIImportFormProps> = ({ 
  open,
  onIntegrationImported, 
  onClose 
}) => {
  const [importType, setImportType] = useState<'url' | 'file'>('url');
  const [url, setUrl] = useState('');
  const [fileContent, setFileContent] = useState('');
  const [fileName, setFileName] = useState('');
  const [baseUrl, setBaseUrl] = useState('');
  const [availableOperations, setAvailableOperations] = useState<string[]>([]);
  const [selectedOperations, setSelectedOperations] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);
  const [loadingOperations, setLoadingOperations] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const exampleUrls = [
    "https://petstore.swagger.io/v2/swagger.json",
    "https://httpbin.org/spec.json",
    "https://api.apis.guru/v2/specs/github.com/1.1.4/openapi.json"
  ];

  const handleFileUpload = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      setFileName(file.name);
      const reader = new FileReader();
      reader.onload = (e) => {
        setFileContent(e.target?.result as string);
      };
      reader.readAsText(file);
    }
  };

  const loadAvailableOperations = async () => {
    setLoadingOperations(true);
    setError(null);
    
    try {
      const source = importType === 'url' ? url : fileContent;
      const operations = await OpenAPIService.getAvailableOperations(source, importType === 'url');
      setAvailableOperations(operations);
      setSelectedOperations(operations); // Select all by default
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load operations');
    } finally {
      setLoadingOperations(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      let response: OpenAPIImportResponse;
      
      if (importType === 'url') {
        response = await OpenAPIService.importFromUrl(
          url,
          baseUrl || undefined,
          selectedOperations.length > 0 ? selectedOperations : undefined
        );
      } else {
        response = await OpenAPIService.importFromFile(
          fileContent,
          baseUrl || undefined,
          selectedOperations.length > 0 ? selectedOperations : undefined
        );
      }

      onIntegrationImported(response);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to import integration');
    } finally {
      setLoading(false);
    }
  };

  const toggleOperation = (operation: string) => {
    setSelectedOperations(prev => 
      prev.includes(operation)
        ? prev.filter(op => op !== operation)
        : [...prev, operation]
    );
  };

  const selectAllOperations = () => {
    setSelectedOperations(availableOperations);
  };

  const clearAllOperations = () => {
    setSelectedOperations([]);
  };

  const setExampleUrl = (exampleUrl: string) => {
    setUrl(exampleUrl);
  };

  const canLoadOperations = importType === 'url' ? url.trim() : fileContent.trim();

  const handleClose = () => {
    setImportType('url');
    setUrl('');
    setFileContent('');
    setFileName('');
    setBaseUrl('');
    setAvailableOperations([]);
    setSelectedOperations([]);
    setError(null);
    onClose();
  };

  return (
    <Dialog 
      open={open} 
      onClose={handleClose}
      maxWidth="lg"
      fullWidth
      PaperProps={{
        sx: { borderRadius: 3, maxHeight: '95vh' }
      }}
    >
      <DialogTitle sx={{ pb: 1 }}>
        <Box display="flex" alignItems="center" justifyContent="space-between">
          <Box display="flex" alignItems="center">
            <Api color="info" sx={{ mr: 1.5, fontSize: 28 }} />
            <Typography variant="h5" component="div" fontWeight="bold">
              Import from OpenAPI/Swagger
            </Typography>
          </Box>
          <IconButton onClick={handleClose} size="small">
            <Close />
          </IconButton>
        </Box>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
          Import API specifications and automatically generate request collections
        </Typography>
      </DialogTitle>

      <Divider />

      <DialogContent sx={{ pt: 2 }}>
        <Box component="form" onSubmit={handleSubmit} sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
          
          {/* Import Source Selection */}
          <Box>
            <Typography variant="h6" gutterBottom>
              Import Source
            </Typography>
            
            <Tabs 
              value={importType} 
              onChange={(_, value) => setImportType(value)}
              sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}
            >
              <Tab 
                value="url" 
                label="From URL" 
                icon={<LinkIcon />} 
                iconPosition="start" 
              />
              <Tab 
                value="file" 
                label="Upload File" 
                icon={<Upload />} 
                iconPosition="start" 
              />
            </Tabs>

            {/* URL Input */}
            {importType === 'url' && (
              <Box>
                <TextField
                  value={url}
                  onChange={(e) => setUrl(e.target.value)}
                  required
                  fullWidth
                  variant="outlined"
                  placeholder="https://api.example.com/swagger.json"
                  helperText="Enter the URL to your OpenAPI/Swagger specification"
                  InputProps={{
                    startAdornment: <LinkIcon sx={{ mr: 1, color: 'text.secondary' }} />
                  }}
                />
                
                {/* Example URLs */}
                <Accordion elevation={0} sx={{ border: '1px solid', borderColor: 'divider', mt: 2 }}>
                  <AccordionSummary expandIcon={<ExpandMore />}>
                    <Typography variant="body2" color="primary">
                      💡 Try example URLs
                    </Typography>
                  </AccordionSummary>
                  <AccordionDetails>
                    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                      {exampleUrls.map((exampleUrl, index) => (
                        <Box key={index} sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <Typography variant="body2" sx={{ flex: 1, fontFamily: 'monospace' }}>
                            {exampleUrl}
                          </Typography>
                          <Button 
                            size="small" 
                            onClick={() => setExampleUrl(exampleUrl)}
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
            )}

            {/* File Upload */}
            {importType === 'file' && (
              <Box>
                <Paper
                  variant="outlined"
                  sx={{
                    p: 3,
                    textAlign: 'center',
                    border: '2px dashed',
                    borderColor: fileContent ? 'success.main' : 'divider',
                    bgcolor: fileContent ? 'success.light' : 'background.paper',
                    cursor: 'pointer',
                    '&:hover': {
                      borderColor: 'primary.main',
                      bgcolor: 'action.hover'
                    }
                  }}
                  component="label"
                >
                  <input
                    type="file"
                    hidden
                    accept=".json,.yaml,.yml"
                    onChange={handleFileUpload}
                  />
                  
                  {fileContent ? (
                    <Box>
                      <CheckCircle color="success" sx={{ fontSize: 48, mb: 1 }} />
                      <Typography variant="h6" color="success.main">
                        File Loaded: {fileName}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Click to choose a different file
                      </Typography>
                    </Box>
                  ) : (
                    <Box>
                      <Upload sx={{ fontSize: 48, mb: 1, color: 'text.secondary' }} />
                      <Typography variant="h6" gutterBottom>
                        Upload OpenAPI Specification
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Drop your JSON or YAML file here, or click to browse
                      </Typography>
                    </Box>
                  )}
                </Paper>
              </Box>
            )}
          </Box>

          {/* Base URL Override */}
          <Box>
            <Typography variant="h6" gutterBottom>
              Base URL Override (Optional)
            </Typography>
            <TextField
              value={baseUrl}
              onChange={(e) => setBaseUrl(e.target.value)}
              fullWidth
              variant="outlined"
              placeholder="https://api.example.com"
              helperText="Override the base URL from the specification file"
              InputProps={{
                startAdornment: <LinkIcon sx={{ mr: 1, color: 'text.secondary' }} />
              }}
            />
          </Box>

          {/* Load Operations Section */}
          <Box>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
              <Typography variant="h6">
                Available Operations
              </Typography>
              <Button
                variant="outlined"
                onClick={loadAvailableOperations}
                disabled={!canLoadOperations || loadingOperations}
                startIcon={loadingOperations ? <CircularProgress size={20} /> : <Refresh />}
              >
                {loadingOperations ? 'Loading...' : 'Load Operations'}
              </Button>
            </Box>

            {/* Operations List */}
            {availableOperations.length > 0 && (
              <Paper variant="outlined" sx={{ maxHeight: 300, overflow: 'auto' }}>
                <Box sx={{ p: 2, borderBottom: 1, borderColor: 'divider', bgcolor: 'background.default' }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                    <Typography variant="subtitle1" fontWeight="bold">
                      Select Operations ({selectedOperations.length} of {availableOperations.length})
                    </Typography>
                    <Box sx={{ display: 'flex', gap: 1 }}>
                      <Button size="small" onClick={selectAllOperations}>
                        Select All
                      </Button>
                      <Button size="small" onClick={clearAllOperations}>
                        Clear All
                      </Button>
                    </Box>
                  </Box>
                </Box>
                
                <List dense>
                  {availableOperations.map((operation, index) => (
                    <ListItem 
                      key={operation} 
                      button 
                      onClick={() => toggleOperation(operation)}
                      divider={index < availableOperations.length - 1}
                    >
                      <ListItemIcon>
                        <Checkbox
                          checked={selectedOperations.includes(operation)}
                          tabIndex={-1}
                          disableRipple
                          color="primary"
                        />
                      </ListItemIcon>
                      <ListItemText 
                        primary={operation}
                        primaryTypographyProps={{ fontFamily: 'monospace', fontSize: '0.9rem' }}
                      />
                    </ListItem>
                  ))}
                </List>
              </Paper>
            )}
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
          disabled={loading || !canLoadOperations}
          startIcon={loading ? <CircularProgress size={20} /> : <CloudDownload />}
          sx={{ minWidth: 180 }}
        >
          {loading ? 'Importing...' : 'Import Integration'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default OpenAPIImportForm; 