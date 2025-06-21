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

### 📊 Advanced Integrations
- **OpenAPI/Swagger Import**: Import API specifications and convert to executable requests
- **Bulk Operations**: Select and import multiple endpoints at once

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

## 🔧 Configuration

### Environment Variables

#### Backend (.NET Core)
```Please Refer to .env_example```

## 🧪 Testing

### Running Tests
```dotnet test ApiPlayground.Tests/ApiPlayground.Tests.csproj```

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

