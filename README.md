# 🚀 API Playground - Advanced HTTP Request Testing Tool

A powerful full-stack web application for storing, managing, and executing HTTP API requests with advanced features like AI-powered generation, OpenAPI imports, OAuth 2.0 authentication, and intelligent retry mechanisms.

## ✨ Key Features

### 🔧 Core Functionality
- **CRUD Operations**: Create, read, update, and delete API integrations and requests
- **HTTP Methods**: Support for GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS
- **Smart Placeholders**: Dynamic variable replacement with JSONPath support
- **Request Execution**: Execute individual requests or entire integration collections
- **Response Handling**: Detailed response viewing with JSON formatting

### 🤖 AI-Powered Features
- **AI Generation**: Generate complete API integrations from natural language descriptions
- **Smart Suggestions**: AI-powered endpoint and parameter recommendations
- **Intelligent Fallbacks**: Automatic sample request generation when AI is unavailable

### 📊 Advanced Integrations
- **OpenAPI/Swagger Import**: Import API specifications and convert to executable requests
- **Bulk Operations**: Select and import multiple endpoints at once
- **Specification Parsing**: Support for OpenAPI 3.0+ and Swagger 2.0

### ⚡ Advanced Execution Engine
- **Multiple Execution Modes**:
  - Sequential: Execute requests in order
  - Parallel: Run multiple requests simultaneously  
  - Conditional: Execute based on response conditions
- **Intelligent Retry System**:
  - Configurable retry attempts (1-10)
  - Exponential backoff support
  - Custom status code filtering
  - Delay configuration between retries
- **OAuth 2.0 Authentication**:
  - Automatic token flow handling
  - Popup-based authentication
  - Token management and refresh

### 🎨 User Experience
- **Material-UI Interface**: Modern, responsive design
- **Real-time Feedback**: Loading states, progress indicators, and error handling
- **Professional Workflows**: Intuitive forms with validation and helpful examples
- **Dark/Light Themes**: Customizable appearance

## 🏗️ Architecture

### Backend (.NET Core 9)
- **Three-tier Architecture**: Core, API, and Tests projects
- **Entity Framework**: In-memory database for development
- **Swagger/OpenAPI**: Automatic API documentation
- **Advanced Services**: AI generation, OAuth handling, OpenAPI parsing
- **Comprehensive Testing**: Unit tests with 80%+ coverage

### Frontend (React TypeScript)
- **Modern React**: Hooks, Context, and TypeScript
- **Material-UI**: Professional component library
- **Axios**: HTTP client with interceptors
- **Responsive Design**: Mobile-first approach

## 🚀 Deployment Options

### 🥇 Railway (Recommended)
- **Free tier**: 500 hours/month
- **Auto-deployment**: Connects to GitHub
- **Docker support**: Automatic container deployment
- **Custom domains**: Easy SSL setup

### 🥈 Render
- **Free tier**: Available with limitations
- **Blueprint deployment**: Uses `render.yaml` configuration
- **Automatic scaling**: Based on traffic

### 🥉 Vercel + Railway
- **Hybrid approach**: Frontend on Vercel, API on Railway
- **Performance optimized**: CDN for frontend
- **Serverless scaling**: Pay-per-use model

### 🏢 Azure App Service
- **Enterprise grade**: Microsoft's platform
- **Native .NET support**: Optimized for .NET Core
- **Free tier**: $200 credit for new accounts

### 📋 GitHub Pages (Limited)
- **Static sites only**: Cannot run .NET API
- **Frontend only**: Requires external API hosting
- **Free hosting**: For open source projects

## 📋 Quick Start

### Local Development
```bash
# Clone the repository
git clone https://github.com/kaxitpandya/api-playground.git
cd api-playground

# Start with Docker Compose
docker-compose up --build

# Access the application
# Frontend: http://localhost:3000
# API: http://localhost:5001
# Swagger: http://localhost:5001/swagger
```

### Production Deployment
1. **Fork this repository**
2. **Choose hosting platform** (Railway recommended)
3. **Set environment variables**:
   ```env
   ASPNETCORE_ENVIRONMENT=Production
   REACT_APP_API_URL=https://your-api-domain.com
   OPENAI_API_KEY=your_openai_key_here
   ```
4. **Deploy using platform-specific instructions**

See [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md) for detailed instructions.

## 🔧 Configuration

### Environment Variables

#### Backend (.NET Core)
```env
ASPNETCORE_ENVIRONMENT=Development|Production
ASPNETCORE_URLS=http://+:80
OPENAI_API_KEY=your_openai_api_key
```

#### Frontend (React)
```env
REACT_APP_API_URL=http://localhost:5001
REACT_APP_OPENAI_API_KEY=your_openai_key_here
```

### Features Configuration
- **AI Generation**: Requires OpenAI API key
- **OAuth 2.0**: Configure redirect URIs in your OAuth provider
- **OpenAPI Import**: Supports public and authenticated endpoints

## 🧪 Testing

### Running Tests
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Frontend tests
cd frontend
npm test
```

### Test Coverage
- **Backend**: 80%+ coverage across all services
- **Integration Tests**: API endpoint testing
- **Unit Tests**: Service layer and business logic

## 📚 API Documentation

### Available Endpoints

#### Integrations
- `GET /api/integrations` - List all integrations
- `POST /api/integrations` - Create new integration
- `GET /api/integrations/{id}` - Get integration by ID
- `PUT /api/integrations/{id}` - Update integration
- `DELETE /api/integrations/{id}` - Delete integration

#### Requests
- `GET /api/integrations/{id}/requests` - List requests
- `POST /api/integrations/{id}/requests` - Create request
- `PUT /api/requests/{id}` - Update request
- `DELETE /api/requests/{id}` - Delete request

#### Execution
- `POST /api/executions/integration/{id}` - Execute integration
- `POST /api/executions/parallel/{id}` - Parallel execution
- `POST /api/executions/conditional/{id}` - Conditional execution

#### AI & Imports
- `POST /api/ai/generate` - Generate integration with AI
- `POST /api/openapi/import-url` - Import from OpenAPI URL
- `POST /api/openapi/import-file` - Import from OpenAPI file

### Swagger Documentation
Visit `/swagger` on your API endpoint for interactive documentation.

## 🤝 Contributing

### Development Setup
1. Install .NET Core 9 SDK
2. Install Node.js 18+
3. Install Docker Desktop
4. Clone repository and run `docker-compose up`

### Code Style
- **Backend**: Follow .NET conventions
- **Frontend**: ESLint + Prettier configuration
- **Commits**: Conventional commit messages

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **OpenAI**: For AI-powered integration generation
- **Material-UI**: For the beautiful component library
- **Railway**: For easy deployment hosting
- **Microsoft**: For .NET Core and Azure services

---

## 🔗 Links

- **Live Demo**: [Deploy your own instance](DEPLOYMENT_GUIDE.md)
- **API Documentation**: `/swagger` endpoint
- **Support**: Create an issue for help
- **Roadmap**: Check the Issues tab for planned features

Built with ❤️ using .NET Core 9, React TypeScript, and Material-UI
