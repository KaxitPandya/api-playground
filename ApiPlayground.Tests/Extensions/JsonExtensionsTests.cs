using ApiPlayground.API.Extensions;
using Xunit;

namespace ApiPlayground.Tests.Extensions
{
    public class JsonExtensionsTests
    {
        
        [Fact]
        public void ExtractValueByJsonPath_WithValidJsonPath_ReturnsValue()
        {
            // Arrange
            var json = @"{""user"": {""id"": 123, ""name"": ""John""}, ""posts"": [{""id"": 1, ""title"": ""Test""}]}";
            
            // Act & Assert
            var userId = json.ExtractValueByJsonPath("$.user.id");
            Assert.Equal("123", userId?.ToString());
            
            var userName = json.ExtractValueByJsonPath("$.user.name");
            Assert.Equal("John", userName?.ToString());
            
            var postTitle = json.ExtractValueByJsonPath("$.posts[0].title");
            Assert.Equal("Test", postTitle?.ToString());
        }
        
        [Fact]
        public void ExtractValueByJsonPath_WithInvalidJsonPath_ReturnsNull()
        {
            // Arrange
            var json = @"{""user"": {""id"": 123}}";
            
            // Act
            var result = json.ExtractValueByJsonPath("$.nonexistent");
            
            // Assert
            Assert.Null(result);
        }
        
        [Fact]
        public void ExtractValueByJsonPath_WithInvalidJson_ReturnsNull()
        {
            // Arrange
            var invalidJson = "invalid json";
            
            // Act
            var result = invalidJson.ExtractValueByJsonPath("$.user.id");
            
            // Assert
            Assert.Null(result);
        }
        
        [Fact]
        public void ExtractValueByJsonPath_WithNullOrEmptyJson_ReturnsNull()
        {
            // Act & Assert
            Assert.Null("".ExtractValueByJsonPath("$.user.id"));
            Assert.Null("   ".ExtractValueByJsonPath("$.user.id"));
        }
        
        [Fact]
        public void ExtractValueByJsonPath_WithComplexNestedStructure_ReturnsCorrectValues()
        {
            // Arrange
            var json = @"{
                ""data"": {
                    ""users"": [
                        {
                            ""id"": 1,
                            ""profile"": {
                                ""email"": ""user1@example.com"",
                                ""preferences"": {
                                    ""theme"": ""dark""
                                }
                            }
                        },
                        {
                            ""id"": 2,
                            ""profile"": {
                                ""email"": ""user2@example.com"",
                                ""preferences"": {
                                    ""theme"": ""light""
                                }
                            }
                        }
                    ]
                }
            }";
            
            // Act & Assert
            var firstUserId = json.ExtractValueByJsonPath("$.data.users[0].id");
            Assert.Equal("1", firstUserId?.ToString());
            
            var secondUserEmail = json.ExtractValueByJsonPath("$.data.users[1].profile.email");
            Assert.Equal("user2@example.com", secondUserEmail?.ToString());
            
            var firstUserTheme = json.ExtractValueByJsonPath("$.data.users[0].profile.preferences.theme");
            Assert.Equal("dark", firstUserTheme?.ToString());
        }
    }
}
