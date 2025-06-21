using ApiPlayground.API.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO;
using Xunit;

namespace ApiPlayground.Tests.Middleware
{
    public class RequestLoggingMiddlewareTests
    {
        private readonly Mock<ILogger<RequestLoggingMiddleware>> _loggerMock;
        private readonly Mock<RequestDelegate> _nextMock;
        
        public RequestLoggingMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger<RequestLoggingMiddleware>>();
            _nextMock = new Mock<RequestDelegate>();
        }
        
        [Fact]
        public async Task InvokeAsync_WithSuccessfulRequest_LogsStartAndCompletion()
        {
            // Arrange
            var middleware = new RequestLoggingMiddleware(_nextMock.Object, _loggerMock.Object);
            var context = CreateHttpContext("/api/test", "GET");
            
            _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
                    .Returns(Task.CompletedTask)
                    .Callback<HttpContext>(ctx => ctx.Response.StatusCode = 200);
            
            // Act
            await middleware.InvokeAsync(context);
            
            // Assert
            _nextMock.Verify(next => next(context), Times.Once);
            
            // Verify logging calls
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting GET /api/test")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
                
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Completed GET /api/test with status code 200")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
        
        [Fact]
        public async Task InvokeAsync_WithException_LogsError()
        {
            // Arrange
            var middleware = new RequestLoggingMiddleware(_nextMock.Object, _loggerMock.Object);
            var context = CreateHttpContext("/api/error", "POST");
            var expectedException = new InvalidOperationException("Test exception");
            
            _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
                    .ThrowsAsync(expectedException);
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => middleware.InvokeAsync(context));
            Assert.Equal("Test exception", exception.Message);
            
            // Verify error logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error during POST /api/error")),
                    expectedException,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
        
        [Theory]
        [InlineData("/", "GET")]
        [InlineData("/api/integrations", "POST")]
        [InlineData("/api/requests/123", "PUT")]
        [InlineData("/swagger", "GET")]
        public async Task InvokeAsync_WithDifferentPaths_LogsCorrectly(string path, string method)
        {
            // Arrange
            var middleware = new RequestLoggingMiddleware(_nextMock.Object, _loggerMock.Object);
            var context = CreateHttpContext(path, method);
            
            _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
                    .Returns(Task.CompletedTask)
                    .Callback<HttpContext>(ctx => ctx.Response.StatusCode = 200);
            
            // Act
            await middleware.InvokeAsync(context);
            
            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Starting {method} {path}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
        
        [Theory]
        [InlineData(200)]
        [InlineData(404)]
        [InlineData(500)]
        [InlineData(201)]
        public async Task InvokeAsync_WithDifferentStatusCodes_LogsStatusCode(int statusCode)
        {
            // Arrange
            var middleware = new RequestLoggingMiddleware(_nextMock.Object, _loggerMock.Object);
            var context = CreateHttpContext("/api/test", "GET");
            
            _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
                    .Returns(Task.CompletedTask)
                    .Callback<HttpContext>(ctx => ctx.Response.StatusCode = statusCode);
            
            // Act
            await middleware.InvokeAsync(context);
            
            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"with status code {statusCode}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
        
        private static HttpContext CreateHttpContext(string path, string method)
        {
            var context = new DefaultHttpContext();
            context.Request.Path = path;
            context.Request.Method = method;
            context.Response.Body = new MemoryStream();
            return context;
        }
    }
} 