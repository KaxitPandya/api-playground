using ApiPlayground.API.Controllers;
using ApiPlayground.Core.Interfaces;
using ApiPlayground.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ApiPlayground.Tests.Controllers
{
    public class ExecutionsControllerTests
    {
        private readonly Mock<IExecutionService> _executionServiceMock;
        private readonly Mock<ILogger<ExecutionsController>> _loggerMock;
        
        public ExecutionsControllerTests()
        {
            _executionServiceMock = new Mock<IExecutionService>();
            _loggerMock = new Mock<ILogger<ExecutionsController>>();
        }
        
        [Fact]
        public async Task ExecuteRequest_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            _executionServiceMock
                .Setup(x => x.ExecuteRequestAsync("invalid-id", It.IsAny<Dictionary<string, string>>()))
                .ThrowsAsync(new ArgumentException("Request with ID invalid-id not found"));
                
            var controller = new ExecutionsController(_executionServiceMock.Object, _loggerMock.Object);
            
            // Act
            var result = await controller.ExecuteRequest("invalid-id");
            
            // Assert
            var actionResult = Assert.IsType<ActionResult<RequestResult>>(result);
            Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        }
        
        [Fact]
        public async Task ExecuteIntegration_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            _executionServiceMock
                .Setup(x => x.ExecuteIntegrationAsync("invalid-id", It.IsAny<Dictionary<string, string>>()))
                .ThrowsAsync(new ArgumentException("Integration with ID invalid-id not found"));
                
            var controller = new ExecutionsController(_executionServiceMock.Object, _loggerMock.Object);
            
            // Act
            var result = await controller.ExecuteIntegration("invalid-id");
            
            // Assert
            var actionResult = Assert.IsType<ActionResult<List<RequestResult>>>(result);
            Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        }
        
        [Fact]
        public async Task ExecuteIntegration_WithNoRequests_ReturnsEmptyList()
        {
            // Arrange
            _executionServiceMock
                .Setup(x => x.ExecuteIntegrationAsync("empty-integration", It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(new List<RequestResult>());
                
            var controller = new ExecutionsController(_executionServiceMock.Object, _loggerMock.Object);
            
            // Act
            var result = await controller.ExecuteIntegration("empty-integration");
            
            // Assert
            var actionResult = Assert.IsType<ActionResult<List<RequestResult>>>(result);
            var returnValue = Assert.IsType<List<RequestResult>>(actionResult.Value);
            Assert.Empty(returnValue);
        }
        
        [Fact]
        public async Task ExecuteRequest_WithValidId_ReturnsResult()
        {
            // Arrange
            var expectedResult = new RequestResult
            {
                Id = "result1",
                RequestId = "request1",
                RequestName = "Test Request",
                StatusCode = 200,
                ResponseTimeMs = 100,
                Response = "{\"id\":\"123\",\"name\":\"Test Response\"}",
                ExecutedAt = DateTime.UtcNow
            };
            
            _executionServiceMock
                .Setup(x => x.ExecuteRequestAsync("request1", It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(expectedResult);
                
            var controller = new ExecutionsController(_executionServiceMock.Object, _loggerMock.Object);
            
            // Act
            var result = await controller.ExecuteRequest("request1");
            
            // Assert
            var actionResult = Assert.IsType<ActionResult<RequestResult>>(result);
            var returnValue = Assert.IsType<RequestResult>(actionResult.Value);
            
            Assert.Equal("request1", returnValue.RequestId);
            Assert.Equal(200, returnValue.StatusCode);
            Assert.NotNull(returnValue.Response);
            Assert.Equal(100, returnValue.ResponseTimeMs);
        }

        [Fact]
        public async Task ExecuteRequest_WithPlaceholders_CallsServiceWithCorrectParameters()
        {
            // Arrange
            var expectedResult = new RequestResult
            {
                Id = "result1",
                RequestId = "placeholder-request",
                RequestName = "Test Request With Placeholders",
                StatusCode = 200,
                ResponseTimeMs = 150,
                Response = "{\"success\":true}",
                ExecutedAt = DateTime.UtcNow
            };
            
            Dictionary<string, string>? capturedPlaceholders = null;
            
            _executionServiceMock
                .Setup(x => x.ExecuteRequestAsync("placeholder-request", It.IsAny<Dictionary<string, string>>()))
                .Callback<string, Dictionary<string, string>>((id, placeholders) => 
                {
                    capturedPlaceholders = placeholders;
                })
                .ReturnsAsync(expectedResult);
                
            var controller = new ExecutionsController(_executionServiceMock.Object, _loggerMock.Object);
            
            // Create placeholders map
            var placeholders = new PlaceholderMap
            {
                Values = new Dictionary<string, string>
                {
                    { "userId", "12345" },
                    { "token", "abc123xyz" }
                }
            };
            
            // Act
            var result = await controller.ExecuteRequest("placeholder-request", placeholders);
            
            // Assert
            var actionResult = Assert.IsType<ActionResult<RequestResult>>(result);
            var returnValue = Assert.IsType<RequestResult>(actionResult.Value);
            
            Assert.Equal(200, returnValue.StatusCode);
            Assert.NotNull(capturedPlaceholders);
            Assert.Equal("12345", capturedPlaceholders["userId"]);
            Assert.Equal("abc123xyz", capturedPlaceholders["token"]);
        }
    }
}