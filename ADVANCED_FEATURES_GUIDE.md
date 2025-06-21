# 🚀 Advanced Features Guide - API Playground

This guide covers the two advanced features for creating integrations quickly and efficiently.

## 🤖 AI Generation Feature

### **What it does:**
- Converts natural language descriptions into complete API integrations
- Automatically generates multiple HTTP requests with proper configurations
- Creates placeholders for dynamic values
- Includes authentication setup when requested

### **How to use:**

#### 1. **Access the Feature**
- Go to http://localhost:3000
- Click **"Create Integration"** dropdown
- Select **"🤖 AI Generate Integration"**

#### 2. **Describe Your Integration**
Write a detailed description of what you want to achieve. Examples:

**✅ Good descriptions:**
```
I want to fetch user data from GitHub API and create a new issue in a repository. 
Include endpoints for getting user profile, listing repositories, and creating issues.
Use Bearer token authentication.
```

**✅ More examples:**
```
Create a weather monitoring integration that:
1. Gets current weather from OpenWeatherMap API
2. Sends alerts via Slack webhook if temperature exceeds 30°C
3. Logs data to a custom analytics endpoint
```

```
Build a user management system with:
- GET all users from /api/users
- POST new user to /api/users with name, email, role
- PUT update user by ID
- DELETE user by ID
Include Bearer token authentication for all requests.
```

#### 3. **Optional: Add Target Endpoints**
- Click **"Add"** to specify exact API domains
- Examples: `api.github.com`, `api.openweathermap.org`
- This helps AI focus on specific APIs

#### 4. **Configure Options**
- **Include Authentication**: Adds Bearer token headers
- **Generate Placeholders**: Creates `{{variableName}}` for dynamic values

#### 5. **Generate and Review**
- Click **"Generate Integration"**
- AI will create complete integration with multiple requests
- Review generated requests and edit as needed

---

## 📊 OpenAPI/Swagger Import Feature

### **What it does:**
- Imports existing API specifications (OpenAPI 3.0, Swagger 2.0)
- Automatically creates requests for all endpoints
- Preserves method types, parameters, headers
- Supports both URL and file upload sources

### **How to use:**

#### 1. **Access the Feature**
- Go to http://localhost:3000
- Click **"Create Integration"** dropdown
- Select **"📊 Import from OpenAPI"**

#### 2. **Choose Import Source**

**Option A: From URL**
- Select **"From URL"** tab
- Enter OpenAPI specification URL
- Try example URLs:
  - `https://petstore.swagger.io/v2/swagger.json` (Swagger Petstore)
  - `https://httpbin.org/spec.json` (HTTPBin API)

**Option B: Upload File**
- Select **"Upload File"** tab
- Drag & drop or click to browse
- Supports `.json`, `.yaml`, `.yml` files

#### 3. **Optional: Override Base URL**
- Enter custom base URL if different from specification
- Example: `https://my-api.company.com` instead of default

#### 4. **Load and Select Operations**
- Click **"Load Operations"** button
- Review all available API endpoints
- Select/deselect specific operations to import
- Use **"Select All"** or **"Clear All"** for bulk actions

#### 5. **Import Integration**
- Click **"Import Integration"**
- All selected operations become individual requests
- Ready to execute with proper configurations

---

## 🎯 **Practical Examples**

### **Example 1: GitHub API Integration (AI Generated)**

**Description to use:**
```
Create a GitHub integration that:
1. Gets authenticated user profile
2. Lists user's repositories
3. Creates a new issue in a specific repository
4. Gets issue details by number
Use Bearer token authentication for all requests.
```

**Expected Result:**
- 4 HTTP requests created automatically
- Bearer token placeholder: `{{github_token}}`
- Repository name placeholder: `{{repo_name}}`
- Issue number placeholder: `{{issue_number}}`

### **Example 2: Pet Store API (OpenAPI Import)**

**Steps:**
1. Use URL: `https://petstore.swagger.io/v2/swagger.json`
2. Load operations (shows ~20 endpoints)
3. Select operations you need (pets, orders, users)
4. Import integration

**Result:**
- Complete pet store API collection
- All CRUD operations for pets, orders, users
- Proper HTTP methods (GET, POST, PUT, DELETE)
- Parameter placeholders ready for use

---

## 🔧 **Pro Tips**

### **For AI Generation:**
1. **Be Specific**: Include exact API names and operations
2. **Mention Authentication**: Specify token types needed
3. **Include Relationships**: Describe how requests connect (use output from request A in request B)
4. **Use Examples**: Reference specific endpoints when possible

### **For OpenAPI Import:**
1. **Check Base URL**: Verify the base URL matches your target environment
2. **Select Wisely**: Import only operations you'll actually use
3. **Preview First**: Use "Load Operations" to see what's available
4. **File Format**: Ensure JSON/YAML files are valid OpenAPI specs

### **After Import/Generation:**
1. **Test Immediately**: Run requests to verify they work
2. **Add Authentication**: Set up Bearer tokens in Token Manager
3. **Configure Placeholders**: Set values for `{{variables}}`
4. **Order Requests**: Arrange requests in logical execution order

---

## 🚀 **Integration Workflow**

### **Typical Usage Pattern:**
1. **Generate/Import** → Get basic structure quickly
2. **Authenticate** → Set up tokens in Token Manager  
3. **Configure** → Set placeholder values
4. **Test** → Execute individual requests
5. **Refine** → Edit requests as needed
6. **Execute** → Run full integration

### **Best Practices:**
- Start with AI generation for custom workflows
- Use OpenAPI import for existing, documented APIs
- Combine both: Import base API, then use AI to create custom workflows
- Always test with real APIs before production use

---

## 📋 **Feature Comparison**

| Feature | AI Generation | OpenAPI Import |
|---------|---------------|----------------|
| **Input** | Natural language | API specification |
| **Speed** | Medium (AI processing) | Fast (direct parsing) |
| **Accuracy** | Good (may need refinement) | Excellent (exact spec) |
| **Customization** | High (describes exactly what you want) | Medium (limited to spec) |
| **Authentication** | Automatically included | May need manual setup |
| **Placeholders** | Smart placeholder generation | Basic parameter mapping |
| **Best for** | Custom workflows, complex logic | Existing APIs, comprehensive coverage |

---

## 🎨 **UI Improvements Made**

### **Enhanced AI Generation Form:**
- ✅ Material-UI design matching app theme
- ✅ Example descriptions with "Use This" buttons
- ✅ Chip-based endpoint management (add/remove)
- ✅ Expandable examples section
- ✅ Better error handling and loading states
- ✅ Icon-enhanced sections

### **Enhanced OpenAPI Import Form:**
- ✅ Tabbed interface (URL vs File Upload)
- ✅ Drag-and-drop file upload with visual feedback
- ✅ Example URLs with quick selection
- ✅ Scrollable operations list with search-like functionality
- ✅ Bulk select/deselect operations
- ✅ Visual file upload status
- ✅ Better operation preview

Both forms now provide a much more intuitive and professional user experience! 🎉

---

## 🌐 **Access Your Enhanced Features**

**Frontend:** http://localhost:3000  
**API Documentation:** http://localhost:5001/swagger  
**Status:** ✅ Running and ready to use

Try both features now with the improved UI! 