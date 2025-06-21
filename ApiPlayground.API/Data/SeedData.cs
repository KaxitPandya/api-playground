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

        // Create sample integration with fixed ID
        var integration = new Integration
        {
            Id = "github-demo-integration-001",
            Name = "GitHub User API Demo",
            Description = "A simple integration to fetch GitHub user data",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Integrations.Add(integration);

        // Create sample requests with fixed IDs
        var requests = new List<Request>
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
                IntegrationId = integration.Id,
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
                IntegrationId = integration.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.Requests.AddRange(requests);
        context.SaveChanges();
    }
}
