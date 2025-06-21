using ApiPlayground.API.Data;
using ApiPlayground.API.Services;
using ApiPlayground.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ApiPlayground.Tests.Services;

public class RequestServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private readonly Mock<ILogger<RequestService>> _mockLogger;

    public RequestServiceTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
            
        _mockLogger = new Mock<ILogger<RequestService>>();
        
        // Seed database
        using var context = new ApplicationDbContext(_options);
        var integration = new Integration
        {
            Id = "11111111-1111-1111-1111-111111111111",
            Name = "Test Integration",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        context.Integrations.Add(integration);
        var request = new Request
        {
            Id = "22222222-2222-2222-2222-222222222222",
            IntegrationId = integration.Id,
            Name = "Test Request",
            Method = HttpMethodType.GET,
            Url = "https://api.example.com/users/123",
            Headers = new Dictionary<string, string>(),
            Order = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        context.Requests.Add(request);
        context.SaveChanges();
    }

    [Fact]
    public async Task GetByIntegrationIdAsync_ShouldReturnRequestsForIntegration()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var service = new RequestService(context, _mockLogger.Object);
        var integrationId = "11111111-1111-1111-1111-111111111111";

        // Act
        var results = await service.GetByIntegrationIdAsync(integrationId);

        // Assert
        Assert.Single(results);
        Assert.Equal("Test Request", results.First().Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnRequest()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var service = new RequestService(context, _mockLogger.Object);
        var id = "22222222-2222-2222-2222-222222222222";

        // Act
        var result = await service.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Request", result.Name);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateNewRequest()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var service = new RequestService(context, _mockLogger.Object);
        var request = new Request
        {
            IntegrationId = "11111111-1111-1111-1111-111111111111",
            Name = "New Request",
            Method = HttpMethodType.POST,
            Url = "https://api.example.com/users",
            Headers = new Dictionary<string, string>()
        };

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(string.Empty, result.Id);
        Assert.Equal("New Request", result.Name);
        
        // Verify it was added to the database
        Assert.Equal(2, await context.Requests.CountAsync());
        
        // Verify order was set automatically
        Assert.Equal(1, result.Order);
    }

    [Fact]
    public async Task UpdateAsync_WithValidId_ShouldUpdateRequest()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var service = new RequestService(context, _mockLogger.Object);
        var request = new Request
        {
            Id = "22222222-2222-2222-2222-222222222222",
            IntegrationId = "11111111-1111-1111-1111-111111111111",
            Name = "Updated Request",
            Method = HttpMethodType.PUT,
            Url = "https://api.example.com/users/123",
            Headers = new Dictionary<string, string>(),
            Order = 0,
        };

        // Act
        var result = await service.UpdateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Request", result.Name);
        Assert.Equal(HttpMethodType.PUT, result.Method);
        
        // Verify it was updated in the database
        var fromDb = await context.Requests.FindAsync(request.Id);
        Assert.NotNull(fromDb);
        Assert.Equal("Updated Request", fromDb.Name);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteRequest()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var service = new RequestService(context, _mockLogger.Object);
        var id = "22222222-2222-2222-2222-222222222222";

        // Act
        var result = await service.DeleteAsync(id);

        // Assert
        Assert.True(result);
        
        // Verify it was deleted from the database
        Assert.Empty(await context.Requests.ToListAsync());
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var service = new RequestService(context, _mockLogger.Object);

        // Act
        var result = await service.GetByIdAsync("non-existent-id");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIntegrationIdAsync_WithNonExistentIntegrationId_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var service = new RequestService(context, _mockLogger.Object);

        // Act
        var result = await service.GetByIntegrationIdAsync("non-existent-integration");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var service = new RequestService(context, _mockLogger.Object);

        // Act
        var result = await service.DeleteAsync("non-existent-id");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CreateAsync_WithAllHttpMethods_ShouldCreateCorrectly()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var service = new RequestService(context, _mockLogger.Object);
        
        var methods = Enum.GetValues<HttpMethodType>();
        var requests = new List<Request>();
        
        foreach (var method in methods)
        {
            var request = new Request
            {
                IntegrationId = "11111111-1111-1111-1111-111111111111",
                Name = $"Test {method} Request",
                Method = method,
                Url = "https://api.example.com/test",
                Headers = new Dictionary<string, string>(),
                Body = method == HttpMethodType.POST || method == HttpMethodType.PUT || method == HttpMethodType.PATCH 
                    ? "{\"test\": \"data\"}" : null
            };
            
            // Act
            var result = await service.CreateAsync(request);
            requests.Add(result);
        }

        // Assert
        Assert.Equal(methods.Length, requests.Count);
        Assert.All(requests, r => Assert.NotEmpty(r.Id));
        
        // Verify all were saved to database
        var dbRequests = await context.Requests.CountAsync();
        Assert.Equal(methods.Length + 1, dbRequests); // +1 for seeded request
    }

    [Fact]
    public async Task CreateAsync_WithComplexHeaders_ShouldPreserveHeaders()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var service = new RequestService(context, _mockLogger.Object);
        
        var request = new Request
        {
            IntegrationId = "11111111-1111-1111-1111-111111111111",
            Name = "Request with Headers",
            Method = HttpMethodType.GET,
            Url = "https://api.example.com/test",
            Headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer token123" },
                { "Content-Type", "application/json" },
                { "X-Custom-Header", "custom-value" },
                { "Accept", "application/json, text/plain" }
            }
        };

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Headers.Count);
        Assert.Equal("Bearer token123", result.Headers["Authorization"]);
        Assert.Equal("application/json", result.Headers["Content-Type"]);
        Assert.Equal("custom-value", result.Headers["X-Custom-Header"]);
        Assert.Equal("application/json, text/plain", result.Headers["Accept"]);
        
        // Verify headers were persisted correctly
        var fromDb = await context.Requests.FindAsync(result.Id);
        Assert.NotNull(fromDb);
        Assert.Equal(4, fromDb.Headers.Count);
    }

    [Fact]
    public async Task UpdateAsync_WithModifiedHeaders_ShouldUpdateHeaders()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var service = new RequestService(context, _mockLogger.Object);
        
        var existingRequest = await context.Requests.FindAsync("22222222-2222-2222-2222-222222222222");
        Assert.NotNull(existingRequest);
        
        existingRequest.Headers = new Dictionary<string, string>
        {
            { "Authorization", "Bearer new-token" },
            { "Content-Type", "application/xml" }
        };

        // Act
        var result = await service.UpdateAsync(existingRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("Bearer new-token", result.Headers["Authorization"]);
        Assert.Equal("application/xml", result.Headers["Content-Type"]);
        
        // Verify headers were updated in database
        var fromDb = await context.Requests.FindAsync(existingRequest.Id);
        Assert.NotNull(fromDb);
        Assert.Equal(2, fromDb.Headers.Count);
    }
}
