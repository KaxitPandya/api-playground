import React, { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { 
  Box, 
  Paper, 
  Typography, 
  Button, 
  LinearProgress,
  Alert,
  Container,
  Card,
  CardContent,
  Chip
} from '@mui/material';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import SecurityIcon from '@mui/icons-material/Security';
import GitHubIcon from '@mui/icons-material/GitHub';

const OAuthDemo: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [step, setStep] = useState<'loading' | 'authorize' | 'success'>('loading');
  const [progress, setProgress] = useState(0);

  const state = searchParams.get('state');
  const integrationId = searchParams.get('integration_id');

  useEffect(() => {
    // Simulate loading
    const timer = setTimeout(() => {
      setStep('authorize');
    }, 1000);

    // Simulate progress
    const progressTimer = setInterval(() => {
      setProgress((oldProgress) => {
        if (oldProgress === 100) {
          clearInterval(progressTimer);
          return 100;
        }
        const diff = Math.random() * 10;
        return Math.min(oldProgress + diff, 100);
      });
    }, 100);

    return () => {
      clearTimeout(timer);
      clearInterval(progressTimer);
    };
  }, []);

  const handleAuthorize = () => {
    setStep('success');
    setTimeout(() => {
      // Simulate successful OAuth flow
      const mockCode = 'demo_auth_code_' + Math.random().toString(36).substring(7);
      navigate(`/oauth/callback?code=${mockCode}&state=${state}&integration_id=${integrationId}`);
    }, 2000);
  };

  const handleCancel = () => {
    navigate('/');
  };

  return (
    <Container maxWidth="sm" sx={{ mt: 4 }}>
      <Card elevation={3}>
        <CardContent sx={{ p: 4 }}>
          <Box display="flex" alignItems="center" justifyContent="center" mb={3}>
            <GitHubIcon sx={{ fontSize: 40, mr: 2, color: '#333' }} />
            <Typography variant="h4" component="h1" fontWeight="bold">
              API Playground
            </Typography>
          </Box>

          {step === 'loading' && (
            <Box>
              <Typography variant="h6" gutterBottom align="center">
                Connecting to OAuth Provider...
              </Typography>
              <LinearProgress variant="determinate" value={progress} sx={{ mt: 2, mb: 2 }} />
              <Typography variant="body2" color="text.secondary" align="center">
                Setting up secure connection
              </Typography>
            </Box>
          )}

          {step === 'authorize' && (
            <Box>
              <Box display="flex" alignItems="center" justifyContent="center" mb={3}>
                <SecurityIcon sx={{ fontSize: 32, mr: 1, color: 'primary.main' }} />
                <Typography variant="h5" component="h2">
                  OAuth 2.0 Authorization
                </Typography>
              </Box>

              <Alert severity="info" sx={{ mb: 3 }}>
                <Typography variant="body2">
                  This is a demo OAuth flow. In a real implementation, this would redirect to the actual OAuth provider.
                </Typography>
              </Alert>

              <Paper sx={{ p: 3, mb: 3, backgroundColor: 'grey.50' }}>
                <Typography variant="h6" gutterBottom>
                  API Playground wants to access:
                </Typography>
                
                <Box sx={{ mt: 2 }}>
                  <Chip 
                    icon={<CheckCircleIcon />} 
                    label="Read repository information" 
                    color="primary" 
                    variant="outlined"
                    sx={{ mb: 1, mr: 1 }}
                  />
                  <Chip 
                    icon={<CheckCircleIcon />} 
                    label="Read user profile" 
                    color="primary" 
                    variant="outlined"
                    sx={{ mb: 1, mr: 1 }}
                  />
                  <Chip 
                    icon={<CheckCircleIcon />} 
                    label="Execute API requests" 
                    color="primary" 
                    variant="outlined"
                    sx={{ mb: 1, mr: 1 }}
                  />
                </Box>

                <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
                  Integration ID: <strong>{integrationId}</strong>
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  State: <strong>{state}</strong>
                </Typography>
              </Paper>

              <Box display="flex" gap={2} justifyContent="center">
                <Button 
                  variant="outlined" 
                  onClick={handleCancel}
                  size="large"
                >
                  Cancel
                </Button>
                <Button 
                  variant="contained" 
                  onClick={handleAuthorize}
                  size="large"
                  startIcon={<SecurityIcon />}
                >
                  Authorize Application
                </Button>
              </Box>
            </Box>
          )}

          {step === 'success' && (
            <Box textAlign="center">
              <CheckCircleIcon sx={{ fontSize: 64, color: 'success.main', mb: 2 }} />
              <Typography variant="h5" gutterBottom color="success.main">
                Authorization Successful!
              </Typography>
              <Typography variant="body1" color="text.secondary">
                Redirecting back to API Playground...
              </Typography>
              <LinearProgress sx={{ mt: 3 }} />
            </Box>
          )}
        </CardContent>
      </Card>
    </Container>
  );
};

export default OAuthDemo; 