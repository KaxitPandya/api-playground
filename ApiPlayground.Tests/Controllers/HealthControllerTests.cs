using ApiPlayground.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ApiPlayground.Tests.Controllers
{
    public class HealthControllerTests
    {
        [Fact]
        public void Get_ReturnsOkWithHealthStatus()
        {
            // Arrange
            var controller = new HealthController();
            
            // Act
            var result = controller.Get();
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var healthStatus = okResult.Value;
            
            Assert.NotNull(healthStatus);
            
            // Use reflection to check the anonymous object properties
            var statusProperty = healthStatus.GetType().GetProperty("status");
            var timestampProperty = healthStatus.GetType().GetProperty("timestamp");
            
            Assert.NotNull(statusProperty);
            Assert.NotNull(timestampProperty);
            Assert.Equal("healthy", statusProperty.GetValue(healthStatus));
            Assert.IsType<DateTime>(timestampProperty.GetValue(healthStatus));
        }
    }
} 