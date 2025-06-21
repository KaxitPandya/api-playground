# 🚀 API Playground Deployment Guide

This guide covers multiple hosting options for your full-stack API Playground application.

## 📋 Prerequisites
- Git repository on GitHub
- Docker images built and tested locally
- API endpoints working correctly

---

## 🥇 Option 1: Railway (Recommended - Free & Easy)

### Why Railway?
- ✅ Free tier with 500 hours/month
- ✅ Automatic Docker deployment
- ✅ Built-in PostgreSQL if needed
- ✅ Easy domain management
- ✅ Great for .NET Core apps

### Steps:
1. **Sign up**: Go to [railway.app](https://railway.app) and sign up with GitHub
2. **Create new project**: Click "New Project" → "Deploy from GitHub repo"
3. **Select repository**: Choose your API Playground repository
4. **Configure services**:
   - Railway will auto-detect your Docker setup
   - It will create separate services for API and Frontend
5. **Set environment variables**:
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://+:$PORT
   ```
6. **Deploy**: Railway automatically builds and deploys
7. **Custom domain** (optional): Add your own domain in Railway dashboard

**Cost**: Free tier (500 hours/month) → $5/month for unlimited

---

## 🥈 Option 2: Render (Free Tier Available)

### Why Render?
- ✅ Free tier available
- ✅ Docker support
- ✅ Auto-deployment from GitHub
- ✅ SSL certificates included

### Steps:
1. **Sign up**: Go to [render.com](https://render.com) and connect GitHub
2. **Use provided config**: The `render.yaml` file is already configured
3. **Create Blueprint**: 
   - Go to Render Dashboard
   - Click "New" → "Blueprint"
   - Connect your GitHub repo
   - Render will read the `render.yaml` file
4. **Deploy**: Both API and Frontend will deploy automatically
5. **Free tier limitations**: 
   - Services sleep after 15 minutes of inactivity
   - Slower cold starts

**Cost**: Free tier available → $7/month per service for always-on

---

## 🥉 Option 3: Vercel (Frontend) + Railway (Backend)

### Hybrid Approach
Split hosting for optimal performance:

### Frontend on Vercel:
1. **Create separate frontend repo** or use subfolder deployment
2. **Deploy to Vercel**:
   ```bash
   npx vercel --prod
   ```
3. **Set environment variables**:
   ```
   REACT_APP_API_URL=https://your-api-url.railway.app
   ```

### Backend on Railway:
1. **Deploy only the API** following Railway steps above
2. **Configure CORS** in your .NET API to allow Vercel domain

**Cost**: Vercel free tier + Railway $5/month

---

## 🏢 Option 4: Azure (Microsoft's Platform)

### Why Azure?
- ✅ Native .NET Core support
- ✅ Enterprise-grade
- ✅ Generous free tier
- ✅ Integration with GitHub Actions

### Steps:
1. **Create Azure account**: Get $200 free credit
2. **Create App Service**:
   ```bash
   az webapp create --resource-group myResourceGroup --plan myAppServicePlan --name myUniqueAppName --deployment-container-image-name your-registry/api-playground:latest
   ```
3. **Set up GitHub Actions**: Use Azure's deployment templates
4. **Configure environment variables** in Azure portal

**Cost**: Free tier (60 CPU minutes/day) → $13+/month

---

## 🔧 Option 5: GitHub Pages (Frontend Only) + External API

### Limitations:
- ⚠️ Only frontend can be hosted on GitHub Pages
- ⚠️ Backend must be hosted elsewhere
- ⚠️ CORS configuration required

### Steps:
1. **Build static frontend**:
   ```bash
   cd frontend
   npm run build
   ```
2. **Deploy to GitHub Pages**:
   - Go to repository Settings → Pages
   - Set source to "GitHub Actions"
   - Create workflow to deploy `frontend/build` folder
3. **Host backend separately** (Railway, Render, etc.)
4. **Update API URL** in frontend environment variables

---

## 🎯 Quick Start - Railway Deployment

Since Railway is the easiest and most cost-effective, here's a quick start:

### 1. Push to GitHub:
```bash
git add .
git commit -m "Add deployment configurations"
git push origin main
```

### 2. Deploy to Railway:
1. Go to [railway.app](https://railway.app)
2. Sign up with GitHub
3. Click "New Project" → "Deploy from GitHub repo"
4. Select your repository
5. Railway auto-detects Docker and deploys both services
6. Get your live URLs from the Railway dashboard

### 3. Test your live application!

---

## 🔒 Environment Variables for Production

Make sure to set these in your hosting platform:

```env
# API
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:$PORT
OPENAI_API_KEY=your_openai_key_here

# Frontend
REACT_APP_API_URL=https://your-api-domain.com
```

---

## 📊 Cost Comparison

| Platform | Free Tier | Paid Tier | Best For |
|----------|-----------|-----------|----------|
| **Railway** | 500 hrs/month | $5/month unlimited | Full-stack apps |
| **Render** | Free with sleep | $7/month per service | Docker apps |
| **Vercel** | Generous free | $20/month | Frontend + serverless |
| **Azure** | $200 credit | $13+/month | Enterprise |
| **GitHub Pages** | Free | Free | Static sites only |

## 🏆 Recommendation

**Start with Railway** - it's the easiest, most cost-effective, and handles your Docker setup perfectly. You can always migrate later if needed.

---

## 🆘 Need Help?

If you run into issues:
1. Check the deployment logs in your hosting platform
2. Verify environment variables are set correctly
3. Test Docker images locally first
4. Check CORS configuration if frontend can't reach API

Good luck with your deployment! 🚀 