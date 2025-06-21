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
    }
}
