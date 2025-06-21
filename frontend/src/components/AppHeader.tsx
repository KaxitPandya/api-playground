import React from 'react';
import { Link as RouterLink } from 'react-router-dom';
import { 
  AppBar, 
  Toolbar, 
  Typography, 
  Button, 
  Box, 
  Container,
  useTheme
} from '@mui/material';
import ApiIcon from '@mui/icons-material/Api';
import HomeIcon from '@mui/icons-material/Home';
import AddIcon from '@mui/icons-material/Add';

const AppHeader: React.FC = () => {
  const theme = useTheme();
  
  return (
    <AppBar position="static">
      <Container maxWidth="xl">
        <Toolbar disableGutters>
          <ApiIcon sx={{ display: 'flex', mr: 1 }} />
          <Typography
            variant="h6"
            noWrap
            component={RouterLink}
            to="/"
            sx={{
              mr: 2,
              display: 'flex',
              fontFamily: 'monospace',
              fontWeight: 700,
              letterSpacing: '.2rem',
              color: 'inherit',
              textDecoration: 'none',
            }}
          >
            API PLAYGROUND
          </Typography>

          <Box sx={{ flexGrow: 1, display: 'flex' }}>
            <Button
              component={RouterLink}
              to="/"
              sx={{ my: 2, color: 'white', display: 'flex', alignItems: 'center' }}
              startIcon={<HomeIcon />}
            >
              Integrations
            </Button>
            <Button
              component={RouterLink}
              to="/?action=create"
              sx={{ my: 2, color: 'white', display: 'flex', alignItems: 'center' }}
              startIcon={<AddIcon />}
            >
              New Integration
            </Button>
          </Box>
          
          <Box sx={{ flexGrow: 0 }}>
            <Button 
              color="inherit" 
              component="a"
              href="/swagger/"
              target="_blank"
            >
              API Docs
            </Button>
          </Box>
        </Toolbar>
      </Container>
    </AppBar>
  );
};

export default AppHeader;
