using ApiPlayground.API.Data;
using ApiPlayground.API.Services;
using ApiPlayground.Core.Interfaces;
using ApiPlayground.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using Xunit;

namespace ApiPlayground.Tests.Services;

public class ExecutionServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private readonly Mock<ILogger<ExecutionService>> _mockLogger;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly Mock<IOAuthService> _mockOAuthService;

    public ExecutionServiceTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
            
        _mockLogger = new Mock<ILogger<ExecutionService>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockOAuthService = new Mock<IOAuthService>();
        
        var client = new HttpClient(_mockHttpMessageHandler.Object);
        _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);
        
        // Seed database
        using var context = new ApplicationDbContext(_options);
        var integration = new Integration
        {
            Id = "test-integration-001",
            Name = "Test Integration",
            Description = "Test Integration for Unit Tests",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        context.Integrations.Add(integration);
        var request = new Request
        {
            Id = "test-request-001",
            IntegrationId = integration.Id,
            Name = "Test Request",
            Method = HttpMethodType.GET,
            Url = "https://api.example.com/users/{{userId}}",
            Headers = new Dictionary<string, string> 
            {
                { "Accept", "application/json" }
            },
            Order = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        context.Requests.Add(request);
        context.SaveChanges();
    }

    [Fact]
    public async Task ExecuteRequestAsync_WithValidRequestId_ShouldReturnResult()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"name\":\"Test User\"}")
            });
        
        using var context = new ApplicationDbContext(_options);
        var service = new ExecutionService(context, _mockHttpClientFactory.Object, _mockLogger.Object, _mockOAuthService.Object);
        
        var placeholders = new Dictionary<string, string> { { "userId", "123" } };

        // Act
        var result = await service.ExecuteRequestAsync("test-request-001", placeholders);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Contains("Test User", result.Response);
    }

    [Fact]
    public async Task ExecuteRequestAsync_WithInvalidRequestId_ShouldThrowException()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var service = new ExecutionService(context, _mockHttpClientFactory.Object, _mockLogger.Object, _mockOAuthService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.ExecuteRequestAsync("invalid-request-id", new Dictionary<string, string>()));
    }

    [Fact]
    public async Task ExecuteIntegrationAsync_WithValidIntegration_ShouldExecuteAllRequests()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"name\":\"Test User\"}")
            });
            
        using var context = new ApplicationDbContext(_options);
        var service = new ExecutionService(context, _mockHttpClientFactory.Object, _mockLogger.Object, _mockOAuthService.Object);
        
        var placeholders = new Dictionary<string, string> { { "userId", "123" } };

        // Act
        var results = await service.ExecuteIntegrationAsync("test-integration-001", placeholders);

        // Assert
        Assert.Single(results);
        Assert.Equal(200, results.First().StatusCode);
    }

    [Fact]
    public async Task ExecuteIntegrationAsync_WithInvalidIntegrationId_ShouldThrowException()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var service = new ExecutionService(context, _mockHttpClientFactory.Object, _mockLogger.Object, _mockOAuthService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.ExecuteIntegrationAsync("invalid-integration-id", new Dictionary<string, string>()));
    }

    [Fact]
    public async Task ExecuteRequestAsync_WithPlaceholders_ShouldReplaceInUrl()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"success\":true}")
            });
        
        using var context = new ApplicationDbContext(_options);
        var service = new ExecutionService(context, _mockHttpClientFactory.Object, _mockLogger.Object, _mockOAuthService.Object);
        
        var placeholders = new Dictionary<string, string> { { "userId", "12345" } };

        // Act
        var result = await service.ExecuteRequestAsync("test-request-001", placeholders);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        
        // Verify URL had placeholder replaced
        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri != null && req.RequestUri.AbsoluteUri == "https://api.example.com/users/12345"),
                ItExpr.IsAny<CancellationToken>()
            );
    }
}
