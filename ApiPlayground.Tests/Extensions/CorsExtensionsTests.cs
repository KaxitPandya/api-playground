using ApiPlayground.API.Extensions;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ApiPlayground.Tests.Extensions
{
    public class CorsExtensionsTests
    {
        [Fact]
        public void AddCorsPolicy_ConfiguresCorsCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            
            // Act
            services.AddCorsPolicy(configuration);
            
            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var corsService = serviceProvider.GetService<ICorsService>();
            Assert.NotNull(corsService);
            
            // Verify CORS policy is configured
            var corsPolicyProvider = serviceProvider.GetService<ICorsPolicyProvider>();
            Assert.NotNull(corsPolicyProvider);
        }
        
        [Fact]
        public void AddCorsPolicy_AddsCorsService()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            
            // Act
            services.AddCorsPolicy(configuration);
            
            // Assert
            var corsServiceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ICorsService));
            Assert.NotNull(corsServiceDescriptor);
        }
        
        [Fact]
        public void AddCorsPolicy_AddsCorsOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            
            // Act
            services.AddCorsPolicy(configuration);
            
            // Assert
            var corsOptionsDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(CorsOptions));
            Assert.NotNull(corsOptionsDescriptor);
        }
    }
} 