import React, { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { 
  Box, 
  Typography, 
  LinearProgress,
  Alert,
  Container,
  Card,
  CardContent
} from '@mui/material';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import ErrorIcon from '@mui/icons-material/Error';

const OAuthCallback: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [status, setStatus] = useState<'processing' | 'success' | 'error'>('processing');
  const [message, setMessage] = useState('Processing OAuth callback...');

  const code = searchParams.get('code');
  const state = searchParams.get('state');
  const integrationId = searchParams.get('integration_id');

  useEffect(() => {
    if (!code || !integrationId) {
      setStatus('error');
      setMessage('Missing required OAuth parameters');
      return;
    }

    // Simulate OAuth token exchange
    const processOAuth = async () => {
      try {
        setMessage('Exchanging authorization code for access token...');
        
        // Simulate API call delay
        await new Promise(resolve => setTimeout(resolve, 2000));
        
        // For demo purposes, always succeed
        setStatus('success');
        setMessage('OAuth authentication completed successfully!');
        
        // Redirect back to the integration after a short delay
        setTimeout(() => {
          navigate(`/integrations/${integrationId}`);
        }, 2000);
        
      } catch (error) {
        setStatus('error');
        setMessage('Failed to complete OAuth authentication');
      }
    };

    processOAuth();
  }, [code, integrationId, navigate]);

  return (
    <Container maxWidth="sm" sx={{ mt: 4 }}>
      <Card elevation={3}>
        <CardContent sx={{ p: 4, textAlign: 'center' }}>
          <Box mb={3}>
            {status === 'processing' && (
              <>
                <Typography variant="h5" gutterBottom>
                  OAuth Authentication
                </Typography>
                <LinearProgress sx={{ mt: 2, mb: 2 }} />
              </>
            )}
            
            {status === 'success' && (
              <CheckCircleIcon sx={{ fontSize: 64, color: 'success.main', mb: 2 }} />
            )}
            
            {status === 'error' && (
              <ErrorIcon sx={{ fontSize: 64, color: 'error.main', mb: 2 }} />
            )}
          </Box>

          <Alert 
            severity={status === 'error' ? 'error' : status === 'success' ? 'success' : 'info'}
            sx={{ mb: 3 }}
          >
            {message}
          </Alert>

          {status === 'processing' && (
            <Typography variant="body2" color="text.secondary">
              Please wait while we complete the authentication process...
            </Typography>
          )}

          {status === 'success' && (
            <Typography variant="body2" color="text.secondary">
              Redirecting back to your integration...
            </Typography>
          )}

          {code && (
            <Box mt={2}>
              <Typography variant="caption" color="text.secondary">
                Authorization Code: {code.substring(0, 20)}...
              </Typography>
            </Box>
          )}
        </CardContent>
      </Card>
    </Container>
  );
};

export default OAuthCallback; 