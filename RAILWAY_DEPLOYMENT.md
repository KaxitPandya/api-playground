# 🚂 Railway Deployment Guide

Complete step-by-step guide to deploy your API Playground to Railway.

## 📋 Prerequisites
- Git installed on your computer
- GitHub account
- Railway account (free signup)

## 🔐 Step 1: Environment Variables Setup

### What NOT to do:
❌ **NEVER commit .env files with real API keys to GitHub**

### What TO do:
✅ **Set environment variables in Railway dashboard**

### Your Environment Variables:
```env
# Required for API
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:$PORT

# Optional - for AI features (get from OpenAI)
OPENAI_API_KEY=your_actual_openai_key_here

# Frontend (Railway will auto-configure this)
REACT_APP_API_URL=https://your-api-service.railway.app
```

## 📤 Step 2: Upload to GitHub

### 2.1 Initialize Git (if not already done):
```bash
# Check if git is initialized
git status

# If not initialized, run:
git init
```

### 2.2 Stage all files:
```bash
# Add all files (environment files are already excluded by .gitignore)
git add .

# Check what will be committed
git status
```

### 2.3 Commit changes:
```bash
git commit -m "Initial commit: API Playground with advanced features

- Full-stack .NET Core + React application
- Advanced execution with retries and OAuth 2.0
- AI-powered integration generation
- OpenAPI/Swagger import functionality
- Material-UI professional interface
- Docker containerization
- Railway deployment configuration"
```

### 2.4 Create GitHub repository:
1. Go to [github.com](https://github.com)
2. Click "New repository" (green button)
3. Repository name: `api-playground` (or your preferred name)
4. Description: `Advanced HTTP API testing tool with AI generation and OAuth 2.0`
5. Make it **Public** (required for Railway free tier)
6. Don't initialize with README (you already have one)
7. Click "Create repository"

### 2.5 Connect and push to GitHub:
```bash
# Add remote origin (replace YOUR_USERNAME with your GitHub username)
git remote add origin https://github.com/YOUR_USERNAME/api-playground.git

# Set main branch
git branch -M main

# Push to GitHub
git push -u origin main
```

## 🚂 Step 3: Deploy to Railway

### 3.1 Sign up for Railway:
1. Go to [railway.app](https://railway.app)
2. Click "Sign up" 
3. Choose "Sign up with GitHub"
4. Authorize Railway to access your repositories

### 3.2 Create new project:
1. Click "New Project"
2. Select "Deploy from GitHub repo"
3. Choose your `api-playground` repository
4. Railway will automatically detect:
   - `Dockerfile.api` → Creates API service
   - `Dockerfile.frontend` → Creates Frontend service

### 3.3 Configure services:

#### API Service:
1. Click on the API service (should be auto-detected)
2. Go to "Variables" tab
3. Add these environment variables:
   ```env
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://+:$PORT
   OPENAI_API_KEY=your_actual_openai_key_here
   ```

#### Frontend Service:
1. Click on the Frontend service
2. Go to "Variables" tab  
3. Add this environment variable:
   ```env
   REACT_APP_API_URL=https://your-api-service.railway.app
   ```
   (Replace with your actual API service URL from Railway)

### 3.4 Get your API service URL:
1. Click on API service
2. Go to "Settings" tab
3. Under "Domains", you'll see something like:
   `https://api-playground-api-production-xxxx.railway.app`
4. Copy this URL

### 3.5 Update Frontend with API URL:
1. Go back to Frontend service
2. Variables tab
3. Update `REACT_APP_API_URL` with the actual API URL from step 3.4
4. Railway will automatically redeploy

### 3.6 Generate domains:
1. Both services should auto-generate Railway domains
2. Your frontend will be available at something like:
   `https://api-playground-frontend-production-xxxx.railway.app`

## ✅ Step 4: Verify Deployment

### 4.1 Check deployments:
- Both services should show "Success" status
- Check deployment logs for any errors

### 4.2 Test your application:
1. Visit your frontend URL
2. Try creating an integration
3. Test the execution features
4. Verify AI generation works (if you added OpenAI key)

### 4.3 Check API endpoints:
- Visit `https://your-api-url.railway.app/swagger`
- Should show the Swagger documentation

## 🔧 Step 5: Custom Domain (Optional)

### 5.1 Add custom domain:
1. Go to service settings
2. Under "Domains"
3. Click "Custom Domain"
4. Add your domain (requires DNS configuration)

## 🚨 Troubleshooting

### Common Issues:

#### Build Fails:
- Check Railway deployment logs
- Verify Dockerfile paths are correct
- Ensure all dependencies are in requirements

#### Frontend can't reach API:
- Verify `REACT_APP_API_URL` is correctly set
- Check CORS settings in .NET API
- Ensure API service is running

#### Environment Variables:
- Double-check variable names (case-sensitive)
- Restart services after changing variables
- Check Railway logs for environment issues

#### 502 Bad Gateway:
- API service might be failing to start
- Check API service logs
- Verify `ASPNETCORE_URLS=http://+:$PORT`

## 💰 Pricing

### Railway Pricing:
- **Free Tier**: 500 hours/month (sleeps after 1 hour of inactivity)
- **Pro Plan**: $5/month for unlimited hours
- **Usage**: Pay for what you use beyond free tier

### Recommendations:
- Start with free tier for testing
- Upgrade to Pro when ready for production use
- Monitor usage in Railway dashboard

## 🔄 Future Updates

### To update your deployed app:
```bash
# Make changes to your code
git add .
git commit -m "Your update message"
git push origin main
```

Railway will automatically detect the push and redeploy both services.

## 🎉 Success!

Your API Playground is now live! 

Share your URLs:
- **Frontend**: `https://your-frontend-url.railway.app`
- **API**: `https://your-api-url.railway.app`
- **Swagger Docs**: `https://your-api-url.railway.app/swagger`

## 📞 Support

If you run into issues:
1. Check Railway deployment logs
2. Verify environment variables
3. Test locally with Docker first
4. Check the Railway documentation
5. Create an issue in your GitHub repository

Happy deploying! 🚀 