using ApiPlayground.API.Data;
using ApiPlayground.API.Middleware;
using ApiPlayground.API.Services;
using ApiPlayground.Core.Interfaces;
using ApiPlayground.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file in development
if (builder.Environment.IsDevelopment())
{
    var envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
    if (File.Exists(envFile))
    {
        foreach (var line in File.ReadAllLines(envFile))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;

            var parts = line.Split('=', 2);
            if (parts.Length == 2)
            {
                Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
            }
        }
    }
}

// Add configuration from environment variables
builder.Configuration.AddEnvironmentVariables();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "API Playground", 
        Version = "v1",
        Description = "A web tool that lets users store and run API calls similar to Postman collections with AI generation, OpenAPI import, parallel execution, retries, conditional flows, and OAuth 2.0 support"
    });
    
    // Add XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add CORS with configurable origins
var allowedOrigins = builder.Configuration["ALLOWED_ORIGINS"]?.Split(',') 
    ?? new[] { "http://localhost:3000", "http://localhost:3001" };

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("ApiPlaygroundDb"));

// Add HttpClient with timeout configuration
var requestTimeout = int.Parse(builder.Configuration["DEFAULT_REQUEST_TIMEOUT"] ?? "30000");
builder.Services.AddHttpClient();

// Register existing application services
builder.Services.AddScoped<IIntegrationService, IntegrationService>();
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IExecutionService, ExecutionService>();

// Register new advanced services
builder.Services.AddScoped<IAIGenerationService, AIGenerationService>();
builder.Services.AddScoped<IOpenAPIImportService, OpenAPIImportService>();
builder.Services.AddScoped<IOAuthService, OAuthService>();

// Register named HttpClient for AI service
builder.Services.AddHttpClient<AIGenerationService>();

// Add logging with configurable level
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    
    var logLevel = builder.Configuration["LOG_LEVEL"] ?? "Information";
    if (Enum.TryParse<LogLevel>(logLevel, out var level))
    {
        logging.SetMinimumLevel(level);
    }
    else
    {
        logging.SetMinimumLevel(LogLevel.Information);
    }
});

var app = builder.Build();

// Seed initial data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    SeedData.Initialize(context);
}

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Playground v1");
    c.RoutePrefix = "swagger"; // Serve the Swagger UI at /swagger
});

// Add request logging middleware
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
