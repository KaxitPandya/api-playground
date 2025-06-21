using ApiPlayground.API.Data;
using ApiPlayground.API.Services;
using ApiPlayground.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ApiPlayground.Tests.Services;

public class IntegrationServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private readonly Mock<ILogger<IntegrationService>> _mockLogger;

    public IntegrationServiceTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
            
        _mockLogger = new Mock<ILogger<IntegrationService>>();
        
        // Seed database
        using var context = new ApplicationDbContext(_options);
        context.Integrations.Add(new Integration
        {
            Id = "11111111-1111-1111-1111-111111111111",
            Name = "Test Integration",
            Description = "Test Description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllIntegrations()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var service = new IntegrationService(context, _mockLogger.Object);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Test Integration", result.First().Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnIntegration()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var service = new IntegrationService(context, _mockLogger.Object);
        var id = "11111111-1111-1111-1111-111111111111";

        // Act
        var result = await service.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Integration", result.Name);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateNewIntegration()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var service = new IntegrationService(context, _mockLogger.Object);
        var integration = new Integration
        {
            Name = "New Integration",
            Description = "New Description"
        };

        // Act
        var result = await service.CreateAsync(integration);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(string.Empty, result.Id);
        Assert.Equal("New Integration", result.Name);
        
        // Verify it was added to the database
        Assert.Equal(2, await context.Integrations.CountAsync());
    }

    [Fact]
    public async Task UpdateAsync_WithValidId_ShouldUpdateIntegration()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var service = new IntegrationService(context, _mockLogger.Object);
        var integration = new Integration
        {
            Id = "11111111-1111-1111-1111-111111111111",
            Name = "Updated Integration",
            Description = "Updated Description"
        };

        // Act
        var result = await service.UpdateAsync(integration);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Integration", result.Name);
        
        // Verify it was updated in the database
        var fromDb = await context.Integrations.FindAsync(integration.Id);
        Assert.NotNull(fromDb);
        Assert.Equal("Updated Integration", fromDb.Name);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteIntegration()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var service = new IntegrationService(context, _mockLogger.Object);
        var id = "11111111-1111-1111-1111-111111111111";

        // Act
        var result = await service.DeleteAsync(id);

        // Assert
        Assert.True(result);
        
        // Verify it was deleted from the database
        Assert.Empty(await context.Integrations.ToListAsync());
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var service = new IntegrationService(context, _mockLogger.Object);

        // Act
        var result = await service.GetByIdAsync("non-existent-id");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var service = new IntegrationService(context, _mockLogger.Object);

        // Act
        var result = await service.DeleteAsync("non-existent-id");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CreateAsync_WithNullIntegration_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var service = new IntegrationService(context, _mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_WithNullIntegration_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var service = new IntegrationService(context, _mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.UpdateAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentIntegration_ShouldThrowInvalidOperationException()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var service = new IntegrationService(context, _mockLogger.Object);
        
        var nonExistentIntegration = new Integration
        {
            Id = "non-existent",
            Name = "Non-existent Integration",
            Description = "This integration doesn't exist"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(nonExistentIntegration));
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldIncludeRequests()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        
        // Add a request to the existing integration
        var request = new Request
        {
            Id = "request-1",
            IntegrationId = "11111111-1111-1111-1111-111111111111",
            Name = "Test Request",
            Method = HttpMethodType.GET,
            Url = "https://api.example.com/test",
            Headers = new Dictionary<string, string>(),
            Order = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        context.Requests.Add(request);
        await context.SaveChangesAsync();
        
        var service = new IntegrationService(context, _mockLogger.Object);

        // Act
        var result = await service.GetByIdAsync("11111111-1111-1111-1111-111111111111");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Integration", result.Name);
        Assert.NotNull(result.Requests);
        Assert.Single(result.Requests);
        Assert.Equal("Test Request", result.Requests.First().Name);
    }

    [Fact]
    public async Task CreateAsync_WithLongDescription_ShouldPreserveDescription()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var service = new IntegrationService(context, _mockLogger.Object);
        
        var longDescription = string.Join(" ", Enumerable.Repeat("This is a very long description that tests the persistence of lengthy text fields.", 10));
        var integration = new Integration
        {
            Name = "Integration with Long Description",
            Description = longDescription
        };

        // Act
        var result = await service.CreateAsync(integration);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(longDescription, result.Description);
        
        // Verify it was persisted correctly
        var fromDb = await context.Integrations.FindAsync(result.Id);
        Assert.NotNull(fromDb);
        Assert.Equal(longDescription, fromDb.Description);
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyDescription_ShouldAllowEmptyDescription()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var service = new IntegrationService(context, _mockLogger.Object);
        
        var integration = await context.Integrations.FindAsync("11111111-1111-1111-1111-111111111111");
        Assert.NotNull(integration);
        
        integration.Description = "";

        // Act
        var result = await service.UpdateAsync(integration);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("", result.Description);
        
        // Verify it was updated in database
        var fromDb = await context.Integrations.FindAsync("11111111-1111-1111-1111-111111111111");
        Assert.NotNull(fromDb);
        Assert.Equal("", fromDb.Description);
    }

    [Fact]
    public async Task UpdateAsync_WithNullDescription_ShouldAllowNullDescription()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var service = new IntegrationService(context, _mockLogger.Object);
        
        var integration = await context.Integrations.FindAsync("11111111-1111-1111-1111-111111111111");
        Assert.NotNull(integration);
        
        integration.Description = null;

        // Act
        var result = await service.UpdateAsync(integration);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Description);
        
        // Verify it was updated in database
        var fromDb = await context.Integrations.FindAsync("11111111-1111-1111-1111-111111111111");
        Assert.NotNull(fromDb);
        Assert.Null(fromDb.Description);
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleIntegrations_ShouldReturnAllOrderedByName()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var service = new IntegrationService(context, _mockLogger.Object);
        
        // Add more integrations
        var integrations = new[]
        {
            new Integration { Name = "Zebra Integration", Description = "Last alphabetically" },
            new Integration { Name = "Alpha Integration", Description = "First alphabetically" },
            new Integration { Name = "Beta Integration", Description = "Second alphabetically" }
        };
        
        foreach (var integration in integrations)
        {
            await service.CreateAsync(integration);
        }

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.Equal(4, result.Count()); // Original + 3 new ones
        
        // Should be ordered by name
        var orderedResults = result.OrderBy(i => i.Name).ToList();
        Assert.Equal("Alpha Integration", orderedResults[0].Name);
        Assert.Equal("Beta Integration", orderedResults[1].Name);
        Assert.Equal("Test Integration", orderedResults[2].Name);
        Assert.Equal("Zebra Integration", orderedResults[3].Name);
    }
}
