using ApiPlayground.API.Data;
using ApiPlayground.Core.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ApiPlayground.Tests.Data
{
    public class SeedDataTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _contextOptions;
        
        public SeedDataTests()
        {
            _contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"SeedDataDb_{Guid.NewGuid()}")
                .Options;
        }
        
        [Fact]
        public void Initialize_WithEmptyDatabase_SeedsInitialData()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            
            // Act
            SeedData.Initialize(context);
            
            // Assert
            var integrations = context.Integrations.Include(i => i.Requests).ToList();
            Assert.Single(integrations);
            
            var integration = integrations.First();
            Assert.Equal("GitHub User API Demo", integration.Name);
            Assert.Equal("A simple integration to fetch GitHub user data", integration.Description);
            Assert.Equal(2, integration.Requests.Count);
            
            // Verify the requests
            var requests = integration.Requests.OrderBy(r => r.Order).ToList();
            
            // First request
            Assert.Equal("Get User", requests[0].Name);
            Assert.Equal(HttpMethodType.GET, requests[0].Method);
            Assert.Equal("https://api.github.com/users/{{username}}", requests[0].Url);
            Assert.Equal(0, requests[0].Order);
            Assert.Contains("Accept", requests[0].Headers.Keys);
            Assert.Equal("application/vnd.github.v3+json", requests[0].Headers["Accept"]);
            
            // Second request
            Assert.Equal("Get User Repos", requests[1].Name);
            Assert.Equal(HttpMethodType.GET, requests[1].Method);
            Assert.Equal("https://api.github.com/users/{{username}}/repos", requests[1].Url);
            Assert.Equal(1, requests[1].Order);
            Assert.Contains("Accept", requests[1].Headers.Keys);
            Assert.Equal("application/vnd.github.v3+json", requests[1].Headers["Accept"]);
        }
        
        [Fact]
        public void Initialize_WithExistingData_DoesNotDuplicate()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            
            // Add some existing data
            var existingIntegration = new Integration
            {
                Id = "existing-integration",
                Name = "Existing Integration",
                Description = "An existing integration"
            };
            context.Integrations.Add(existingIntegration);
            context.SaveChanges();
            
            // Act
            SeedData.Initialize(context);
            
            // Assert
            var integrations = context.Integrations.Include(i => i.Requests).ToList();
            Assert.Single(integrations); // Only the existing one, no seeded data
            
            // Verify only the existing integration exists (seeding skipped)
            Assert.Contains(integrations, i => i.Name == "Existing Integration");
        }
        
        [Fact]
        public void Initialize_CalledMultipleTimes_DoesNotDuplicateData()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            
            // Act
            SeedData.Initialize(context);
            SeedData.Initialize(context); // Call twice
            
            // Assert
            var integrations = context.Integrations.Include(i => i.Requests).ToList();
            Assert.Single(integrations); // Should still be only one
            
            var integration = integrations.First();
            Assert.Equal(2, integration.Requests.Count); // Should still be 2 requests
        }
    }
} 