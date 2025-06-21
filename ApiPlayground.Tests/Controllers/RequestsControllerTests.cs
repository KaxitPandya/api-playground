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
    public class RequestsControllerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _contextOptions;
        private readonly Mock<ILogger<RequestsController>> _loggerMock;
        private readonly Mock<IRequestService> _requestServiceMock;
        
        public RequestsControllerTests()
        {
            _contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"RequestsDb_{Guid.NewGuid()}")
                .Options;
                
            _loggerMock = new Mock<ILogger<RequestsController>>();
            _requestServiceMock = new Mock<IRequestService>();
            
            // Seed the test database
            using var context = new ApplicationDbContext(_contextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            
            var integration = new Integration 
            { 
                Id = "test-integration-1", 
                Name = "Test Integration", 
                Description = "Test Description" 
            };
            
            var requests = new List<Request>
            {
                new Request 
                { 
                    Id = "test-request-1", 
                    IntegrationId = "test-integration-1", 
                    Name = "Test Request 1", 
                    Method = HttpMethodType.GET,
                    Url = "https://api.example.com/test1",
                    Headers = new Dictionary<string, string>(),
                    Order = 0
                },
                new Request 
                { 
                    Id = "test-request-2", 
                    IntegrationId = "test-integration-1", 
                    Name = "Test Request 2", 
                    Method = HttpMethodType.POST,
                    Url = "https://api.example.com/test2",
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                    Body = "{\"test\": \"data\"}",
                    Order = 1
                }
            };
            
            context.Integrations.Add(integration);
            context.Requests.AddRange(requests);
            context.SaveChanges();
        }
        
        [Fact]
        public async Task GetByIntegrationId_WithValidId_ReturnsRequests()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var controller = new RequestsController(_requestServiceMock.Object, _loggerMock.Object, context);
            
            // Act
            var result = await controller.GetByIntegrationId("test-integration-1");
            
            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Request>>>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<Request>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count());
            Assert.Equal("Test Request 1", returnValue.First().Name);
        }
        
        [Fact]
        public async Task GetByIntegrationId_WithInvalidId_ReturnsEmptyList()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var controller = new RequestsController(_requestServiceMock.Object, _loggerMock.Object, context);
            
            // Act
            var result = await controller.GetByIntegrationId("non-existent-id");
            
            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Request>>>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<Request>>(actionResult.Value);
            Assert.Empty(returnValue);
        }
        
        [Fact]
        public async Task GetById_WithValidId_ReturnsRequest()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var controller = new RequestsController(_requestServiceMock.Object, _loggerMock.Object, context);
            
            // Act
            var result = await controller.GetById("test-request-1");
            
            // Assert
            var actionResult = Assert.IsType<ActionResult<Request>>(result);
            var returnValue = Assert.IsType<Request>(actionResult.Value);
            Assert.Equal("Test Request 1", returnValue.Name);
            Assert.Equal(HttpMethodType.GET, returnValue.Method);
        }
        
        [Fact]
        public async Task GetById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var controller = new RequestsController(_requestServiceMock.Object, _loggerMock.Object, context);
            
            // Act
            var result = await controller.GetById("non-existent-id");
            
            // Assert
            var actionResult = Assert.IsType<ActionResult<Request>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }
        
        [Fact]
        public async Task Create_WithValidRequest_ReturnsCreatedResponse()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var controller = new RequestsController(_requestServiceMock.Object, _loggerMock.Object, context);
            var newRequest = new Request
            {
                IntegrationId = "test-integration-1",
                Name = "New Test Request",
                Method = HttpMethodType.POST,
                Url = "https://api.example.com/new",
                Headers = new Dictionary<string, string>(),
                Body = "{\"new\": \"data\"}"
            };
            
            // Act
            var result = await controller.Create(newRequest);
            
            // Assert
            var actionResult = Assert.IsType<ActionResult<Request>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnValue = Assert.IsType<Request>(createdAtActionResult.Value);
            Assert.Equal("New Test Request", returnValue.Name);
            Assert.NotEmpty(returnValue.Id);
            
            // Verify it was saved to the database
            Assert.Equal(3, context.Requests.Count());
        }
        
        [Fact]
        public async Task Create_WithInvalidIntegrationId_ReturnsBadRequest()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var controller = new RequestsController(_requestServiceMock.Object, _loggerMock.Object, context);
            var newRequest = new Request
            {
                IntegrationId = "non-existent-integration",
                Name = "New Test Request",
                Method = HttpMethodType.POST,
                Url = "https://api.example.com/new"
            };
            
            // Act
            var result = await controller.Create(newRequest);
            
            // Assert
            var actionResult = Assert.IsType<ActionResult<Request>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal("Invalid integration ID", badRequestResult.Value);
        }
        
        [Fact]
        public async Task Update_WithValidRequest_ReturnsNoContent()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var controller = new RequestsController(_requestServiceMock.Object, _loggerMock.Object, context);
            var request = await context.Requests.FindAsync("test-request-1");
            request!.Name = "Updated Request Name";
            
            // Act
            var result = await controller.Update("test-request-1", request);
            
            // Assert
            Assert.IsType<NoContentResult>(result);
            
            // Verify it was updated in the database
            var updatedRequest = await context.Requests.FindAsync("test-request-1");
            Assert.Equal("Updated Request Name", updatedRequest!.Name);
        }
        
        [Fact]
        public async Task Update_WithMismatchedId_ReturnsBadRequest()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var controller = new RequestsController(_requestServiceMock.Object, _loggerMock.Object, context);
            var request = await context.Requests.FindAsync("test-request-1");
            
            // Act
            var result = await controller.Update("different-id", request!);
            
            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("ID mismatch", badRequestResult.Value);
        }
        
        [Fact]
        public async Task Update_WithInvalidIntegrationId_ReturnsBadRequest()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var controller = new RequestsController(_requestServiceMock.Object, _loggerMock.Object, context);
            var request = await context.Requests.FindAsync("test-request-1");
            request!.IntegrationId = "non-existent-integration";
            
            // Act
            var result = await controller.Update("test-request-1", request);
            
            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid integration ID", badRequestResult.Value);
        }
        
        [Fact]
        public async Task Update_WithNonExistentRequest_ReturnsNotFound()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var controller = new RequestsController(_requestServiceMock.Object, _loggerMock.Object, context);
            var request = new Request
            {
                Id = "non-existent-request",
                IntegrationId = "test-integration-1",
                Name = "Non-existent Request",
                Method = HttpMethodType.GET,
                Url = "https://api.example.com/test"
            };
            
            // Act
            var result = await controller.Update("non-existent-request", request);
            
            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        
        [Fact]
        public async Task Delete_WithValidId_ReturnsNoContent()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var controller = new RequestsController(_requestServiceMock.Object, _loggerMock.Object, context);
            
            // Act
            var result = await controller.Delete("test-request-1");
            
            // Assert
            Assert.IsType<NoContentResult>(result);
            
            // Verify it was deleted from the database
            Assert.Equal(1, context.Requests.Count());
            Assert.Null(await context.Requests.FindAsync("test-request-1"));
        }
        
        [Fact]
        public async Task Delete_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var controller = new RequestsController(_requestServiceMock.Object, _loggerMock.Object, context);
            
            // Act
            var result = await controller.Delete("non-existent-id");
            
            // Assert
            Assert.IsType<NotFoundResult>(result);
            
            // Verify nothing was deleted
            Assert.Equal(2, context.Requests.Count());
        }
    }
} 