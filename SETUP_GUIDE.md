# 🔧 Environment Setup Guide

This guide will help you configure the API Playground with real API keys and tokens for full functionality.

## 📋 Required Environment Variables

### 1. Copy Environment Template
```bash
# Copy the template file
cp .env.example .env

# Edit the .env file with your real values
notepad .env  # Windows
nano .env     # Linux/Mac
```

### 2. OpenAI Configuration (Required for AI Features)
```bash
# Get your API key from: https://platform.openai.com/api-keys
OPENAI_API_KEY=sk-your-actual-openai-api-key-here
```

### 3. GitHub Configuration (Required for GitHub API & OAuth)
```bash
# Create a GitHub OAuth App: https://github.com/settings/applications/new
GITHUB_CLIENT_ID=your-github-client-id-here
GITHUB_CLIENT_SECRET=your-github-client-secret-here

# Create a Personal Access Token: https://github.com/settings/tokens/new
# Required scopes: repo, user, read:org
GITHUB_PERSONAL_ACCESS_TOKEN=ghp_your-github-personal-access-token-here
```

### 4. Complete .env File Template
```bash
# API Configuration
API_PORT=5001
FRONTEND_PORT=3000

# OpenAI Configuration (for AI-powered features)
OPENAI_API_KEY=sk-your-openai-api-key-here

# GitHub Configuration (for OAuth and API access)
GITHUB_CLIENT_ID=your-github-client-id-here
GITHUB_CLIENT_SECRET=your-github-client-secret-here
GITHUB_PERSONAL_ACCESS_TOKEN=ghp_your-github-personal-access-token-here

# Database Configuration
DATABASE_CONNECTION_STRING=InMemory
ASPNETCORE_ENVIRONMENT=Development

# CORS Configuration
ALLOWED_ORIGINS=http://localhost:3000,http://localhost:3001

# Logging Configuration
LOG_LEVEL=Information

# Security Configuration
JWT_SECRET_KEY=your-super-secret-jwt-key-at-least-32-characters-long
JWT_ISSUER=ApiPlayground
JWT_AUDIENCE=ApiPlaygroundUsers

# External API Configuration
DEFAULT_REQUEST_TIMEOUT=30000
MAX_CONCURRENT_REQUESTS=10

# OAuth Configuration
OAUTH_REDIRECT_URI=http://localhost:3000/oauth/callback
OAUTH_STATE_SECRET=your-oauth-state-secret-here

# Rate Limiting
RATE_LIMIT_REQUESTS_PER_MINUTE=100
RATE_LIMIT_REQUESTS_PER_HOUR=1000
```

## 🔑 How to Get API Keys

### OpenAI API Key
1. Go to [OpenAI Platform](https://platform.openai.com/api-keys)
2. Sign in or create an account
3. Click "Create new secret key"
4. Copy the key (starts with `sk-`)
5. Add billing information (required for API usage)

### GitHub OAuth App
1. Go to [GitHub Developer Settings](https://github.com/settings/applications/new)
2. Fill in the application details:
   - **Application name**: API Playground
   - **Homepage URL**: http://localhost:3000
   - **Authorization callback URL**: http://localhost:3000/oauth/callback
3. Click "Register application"
4. Copy the Client ID and generate a Client Secret

### GitHub Personal Access Token
1. Go to [GitHub Token Settings](https://github.com/settings/tokens/new)
2. Select scopes:
   - `repo` - Full control of private repositories
   - `user` - Read user profile data
   - `read:org` - Read organization membership
3. Click "Generate token"
4. Copy the token (starts with `ghp_`)

## 🚀 Running with Environment Variables

### Method 1: Docker (Recommended)
```bash
# Make sure .env file exists with your values
docker-compose up --build
```

### Method 2: Local Development
```bash
# Backend (.NET)
cd ApiPlayground.API
dotnet run

# Frontend (React) - in another terminal
cd frontend
npm install
npm start
```

## ✅ Verifying Setup

### 1. Check Environment Loading
After starting the application, check the logs for:
```
Successfully loaded environment variables from .env file
OpenAI API Key: sk-****...
GitHub Client ID: ****...
```

### 2. Test AI Features
1. Go to http://localhost:3000
2. Click "AI Generate"
3. Enter a description like "I want to fetch GitHub user data"
4. If successful, you'll see a generated integration

### 3. Test GitHub Integration
1. Use the pre-loaded "GitHub User API Demo"
2. Run with username "octocat"
3. Should return GitHub user data successfully

## 🔒 Security Best Practices

### Environment Files
- ✅ Never commit `.env` files to version control
- ✅ Use `.env.example` as templates
- ✅ Rotate API keys regularly
- ✅ Use different keys for development/production

### API Key Management
- ✅ Store keys in environment variables
- ✅ Use key restrictions when available
- ✅ Monitor API usage and costs
- ✅ Revoke unused or compromised keys

## 🐛 Troubleshooting

### "OpenAI API key is required" Error
- Check that `OPENAI_API_KEY` is set in `.env`
- Verify the key starts with `sk-`
- Ensure you have billing set up on OpenAI account

### "GitHub API rate limit exceeded"
- Use `GITHUB_PERSONAL_ACCESS_TOKEN` for higher rate limits
- Check GitHub API rate limit status in headers

### Docker Environment Issues
- Run `docker-compose down && docker-compose up --build`
- Check that `.env` file is in the same directory as `docker-compose.yml`

### CORS Errors
- Verify `ALLOWED_ORIGINS` includes your frontend URL
- Check that ports match your configuration

## 📚 Additional Resources

- [OpenAI API Documentation](https://platform.openai.com/docs)
- [GitHub API Documentation](https://docs.github.com/en/rest)
- [GitHub OAuth Apps](https://docs.github.com/en/developers/apps/building-oauth-apps)
- [Docker Compose Environment Variables](https://docs.docker.com/compose/environment-variables/) 