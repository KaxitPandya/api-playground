import React, { useState } from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { CssBaseline, ThemeProvider, createTheme, Container, Box } from '@mui/material';
import AppHeader from './components/AppHeader';
import IntegrationList from './components/IntegrationList';
import IntegrationDetail from './components/IntegrationDetail';
import RequestForm from './components/RequestForm';
import ExecutionView from './components/ExecutionView';
import TokenManager from './components/TokenManager';

const theme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#f50057',
    },
  },
});

function App() {
  const [token, setToken] = useState<string | null>(localStorage.getItem('authToken'));

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Router>
        <AppHeader />
        <Container maxWidth="xl" sx={{ mt: 3, mb: 4 }}>
          <TokenManager onTokenChange={setToken} />
          <Box>
            <Routes>
              <Route path="/" element={<IntegrationList />} />
              <Route path="/integrations/:id" element={<IntegrationDetail />} />
              <Route path="/integrations/:id/requests/new" element={<RequestForm />} />
              <Route path="/requests/:id/edit" element={<RequestForm />} />
              <Route path="/integrations/:id/execute" element={<ExecutionView />} />
            </Routes>
          </Box>
        </Container>
      </Router>
    </ThemeProvider>
  );
}

export default App;
