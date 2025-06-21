using ApiPlayground.API.Data;
using ApiPlayground.Core.Interfaces;
using ApiPlayground.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiPlayground.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IntegrationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IIntegrationService _integrationService;
    private readonly ILogger<IntegrationsController> _logger;

    public IntegrationsController(
        ApplicationDbContext context,
        IIntegrationService integrationService,
        ILogger<IntegrationsController> logger)
    {
        _context = context;
        _integrationService = integrationService;
        _logger = logger;

        // Seed data if needed
        if (!_context.Integrations.Any())
        {
            SeedData();
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Integration>>> GetAll()
    {
        _logger.LogInformation("Getting all integrations");
        return await _context.Integrations
            .Include(i => i.Requests.OrderBy(r => r.Order))
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Integration>> GetById(string id)
    {
        _logger.LogInformation("Getting integration with ID: {Id}", id);
        var integration = await _context.Integrations
            .Include(i => i.Requests.OrderBy(r => r.Order))
            .FirstOrDefaultAsync(i => i.Id == id);

        if (integration == null)
        {
            _logger.LogWarning("Integration with ID {Id} not found", id);
            return NotFound();
        }

        return integration;
    }

    [HttpPost]
    public async Task<ActionResult<Integration>> Create([FromBody] Integration integration)
    {
        if (string.IsNullOrEmpty(integration.Id))
        {
            integration.Id = Guid.NewGuid().ToString();
        }

        _logger.LogInformation("Creating new integration: {Name}", integration.Name);
        _context.Integrations.Add(integration);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = integration.Id }, integration);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Integration integration)
    {
        if (id != integration.Id)
        {
            _logger.LogWarning("ID mismatch in Update operation");
            return BadRequest("ID mismatch");
        }

        _logger.LogInformation("Updating integration with ID: {Id}", id);
        _context.Entry(integration).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await IntegrationExists(id))
            {
                _logger.LogWarning("Integration with ID {Id} not found during update", id);
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        _logger.LogInformation("Deleting integration with ID: {Id}", id);
        var integration = await _context.Integrations.FindAsync(id);
        if (integration == null)
        {
            _logger.LogWarning("Integration with ID {Id} not found during delete", id);
            return NotFound();
        }

        _context.Integrations.Remove(integration);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> IntegrationExists(string id)
    {
        return await _context.Integrations.AnyAsync(e => e.Id == id);
    }

    private void SeedData()
    {
        _logger.LogInformation("Seeding initial data");
        
        var integrations = new List<Integration>
        {
            new Integration
            {
                Name = "GitHub API",
                Description = "Collection of GitHub API calls"
            },
            new Integration
            {
                Name = "Weather API",
                Description = "OpenWeatherMap API integration"
            },
            new Integration
            {
                Name = "User Service",
                Description = "User management endpoints"
            }
        };

        _context.Integrations.AddRange(integrations);
        _context.SaveChanges();

        // Add a sample request
        var sampleRequest = new Request
        {
            IntegrationId = integrations[0].Id,
            Name = "Get GitHub User",
            Method = HttpMethodType.GET,
            Url = "https://api.github.com/users/octocat",
            Order = 1,
            Headers = new Dictionary<string, string>
            {
                { "Accept", "application/vnd.github.v3+json" }
            }
        };

        _context.Requests.Add(sampleRequest);
        _context.SaveChanges();
    }
}
