using ApiPlayground.API.Controllers;
using ApiPlayground.API.Data;
using ApiPlayground.Core.Interfaces;
using ApiPlayground.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ApiPlayground.Tests.Controllers
{
    public class IntegrationsControllerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _contextOptions;
        private readonly Mock<ILogger<IntegrationsController>> _loggerMock;
        private readonly Mock<IIntegrationService> _integrationServiceMock;
        
        public IntegrationsControllerTests()
        {
            _contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"IntegrationsDb_{Guid.NewGuid()}")
                .Options;
                
            _loggerMock = new Mock<ILogger<IntegrationsController>>();
            _integrationServiceMock = new Mock<IIntegrationService>();
            
            // Seed the test database
            using var context = new ApplicationDbContext(_contextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            
            var integrations = new List<Integration>
            {
                new Integration { Id = "1", Name = "Test Integration 1", Description = "Test Description 1" },
                new Integration { Id = "2", Name = "Test Integration 2", Description = "Test Description 2" }
            };
            
            context.Integrations.AddRange(integrations);
            context.SaveChanges();
        }
        
        [Fact]
        public async Task GetAll_ReturnsAllIntegrations()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var controller = new IntegrationsController(context, _integrationServiceMock.Object, _loggerMock.Object);
            
            // Act
            var result = await controller.GetAll();
            
            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Integration>>>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<Integration>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count());
        }
        
        [Fact]
        public async Task GetById_WithValidId_ReturnsIntegration()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var controller = new IntegrationsController(context, _integrationServiceMock.Object, _loggerMock.Object);
            
            // Act
            var result = await controller.GetById("1");
            
            // Assert
            var actionResult = Assert.IsType<ActionResult<Integration>>(result);
            var returnValue = Assert.IsType<Integration>(actionResult.Value);
            Assert.Equal("Test Integration 1", returnValue.Name);
        }
        
        [Fact]
        public async Task GetById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var controller = new IntegrationsController(context, _integrationServiceMock.Object, _loggerMock.Object);
            
            // Act
            var result = await controller.GetById("999");
            
            // Assert
            var actionResult = Assert.IsType<ActionResult<Integration>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }
        
        [Fact]
        public async Task Create_WithValidIntegration_ReturnsCreatedResponse()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var controller = new IntegrationsController(context, _integrationServiceMock.Object, _loggerMock.Object);
            var newIntegration = new Integration
            {
                Name = "New Integration",
                Description = "New Description"
            };
            
            // Act
            var result = await controller.Create(newIntegration);
            
            // Assert
            var actionResult = Assert.IsType<ActionResult<Integration>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnValue = Assert.IsType<Integration>(createdAtActionResult.Value);
            Assert.Equal("New Integration", returnValue.Name);
            
            // Verify it was saved to the database
            Assert.Equal(3, context.Integrations.Count());
        }
        
        [Fact]
        public async Task Update_WithValidIntegration_ReturnsNoContent()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var controller = new IntegrationsController(context, _integrationServiceMock.Object, _loggerMock.Object);
            var integration = await context.Integrations.FindAsync("1");
            integration!.Name = "Updated Integration";
            
            // Act
            var result = await controller.Update("1", integration);
            
            // Assert
            Assert.IsType<NoContentResult>(result);
            
            // Verify it was updated in the database
            var updatedIntegration = await context.Integrations.FindAsync("1");
            Assert.Equal("Updated Integration", updatedIntegration!.Name);
        }
        
        [Fact]
        public async Task Update_WithMismatchedId_ReturnsBadRequest()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var controller = new IntegrationsController(context, _integrationServiceMock.Object, _loggerMock.Object);
            var integration = await context.Integrations.FindAsync("1");
            
            // Act
            var result = await controller.Update("2", integration!);
            
            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
        
        [Fact]
        public async Task Delete_WithValidId_ReturnsNoContent()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var controller = new IntegrationsController(context, _integrationServiceMock.Object, _loggerMock.Object);
            
            // Act
            var result = await controller.Delete("1");
            
            // Assert
            Assert.IsType<NoContentResult>(result);
            
            // Verify it was deleted from the database
            Assert.Equal(1, context.Integrations.Count());
            Assert.Null(await context.Integrations.FindAsync("1"));
        }
        
        [Fact]
        public async Task Delete_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var controller = new IntegrationsController(context, _integrationServiceMock.Object, _loggerMock.Object);
            
            // Act
            var result = await controller.Delete("999");
            
            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}