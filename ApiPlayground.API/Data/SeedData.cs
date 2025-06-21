using ApiPlayground.Core.Models;

namespace ApiPlayground.API.Data;

public static class SeedData
{
    public static void Initialize(ApplicationDbContext context)
    {
        // Skip if data already exists
        if (context.Integrations.Any())
        {
            return;
        }

        // Create GitHub integration with fixed ID
        var githubIntegration = new Integration
        {
            Id = "github-demo-integration-001",
            Name = "GitHub API",
            Description = "Collection of GitHub API calls for repository and user data",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Integrations.Add(githubIntegration);

        // Create GitHub requests with fixed IDs
        var githubRequests = new List<Request>
        {
            new Request
            {
                Id = "github-get-user-request-001",
                Name = "Get User",
                Method = HttpMethodType.GET,
                Url = "https://api.github.com/users/{{username}}",
                Headers = new Dictionary<string, string> 
                {
                    { "Accept", "application/vnd.github.v3+json" },
                    { "User-Agent", "API-Playground" }
                },
                Order = 0,
                IntegrationId = githubIntegration.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Request
            {
                Id = "github-get-repos-request-001",
                Name = "Get User Repos",
                Method = HttpMethodType.GET,
                Url = "https://api.github.com/users/{{username}}/repos",
                Headers = new Dictionary<string, string> 
                {
                    { "Accept", "application/vnd.github.v3+json" },
                    { "User-Agent", "API-Playground" }
                },
                Order = 1,
                IntegrationId = githubIntegration.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.Requests.AddRange(githubRequests);
        context.SaveChanges();
    }
}
