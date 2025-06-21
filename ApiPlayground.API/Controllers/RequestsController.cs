using ApiPlayground.API.Data;
using ApiPlayground.Core.Interfaces;
using ApiPlayground.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiPlayground.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RequestsController : ControllerBase
{
    private readonly IRequestService _requestService;
    private readonly ILogger<RequestsController> _logger;
    private readonly ApplicationDbContext _context;

    public RequestsController(
        IRequestService requestService,
        ILogger<RequestsController> logger,
        ApplicationDbContext context)
    {
        _requestService = requestService;
        _logger = logger;
        _context = context;
    }

    [HttpGet("integration/{integrationId}")]
    public async Task<ActionResult<IEnumerable<Request>>> GetByIntegrationId(string integrationId)
    {
        _logger.LogInformation("Getting requests for integration ID: {IntegrationId}", integrationId);
        
        var requests = await _context.Requests
            .Where(r => r.IntegrationId == integrationId)
            .OrderBy(r => r.Order)
            .ToListAsync();
            
        return requests;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Request>> GetById(string id)
    {
        _logger.LogInformation("Getting request with ID: {Id}", id);
        var request = await _context.Requests.FindAsync(id);

        if (request == null)
        {
            _logger.LogWarning("Request with ID {Id} not found", id);
            return NotFound();
        }

        return request;
    }

    [HttpPost]
    public async Task<ActionResult<Request>> Create([FromBody] Request request)
    {
        if (string.IsNullOrEmpty(request.Id))
        {
            request.Id = Guid.NewGuid().ToString();
        }

        // Validate that the integration exists
        var integration = await _context.Integrations.FindAsync(request.IntegrationId);
        if (integration == null)
        {
            _logger.LogWarning("Attempt to create request for non-existent integration: {IntegrationId}", request.IntegrationId);
            return BadRequest("Invalid integration ID");
        }

        _logger.LogInformation("Creating new request: {Name} for integration: {IntegrationId}", request.Name, request.IntegrationId);
        _context.Requests.Add(request);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = request.Id }, request);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Request request)
    {
        if (id != request.Id)
        {
            _logger.LogWarning("ID mismatch in Update operation");
            return BadRequest("ID mismatch");
        }

        // Validate that the integration exists
        var integration = await _context.Integrations.FindAsync(request.IntegrationId);
        if (integration == null)
        {
            _logger.LogWarning("Attempt to update request for non-existent integration: {IntegrationId}", request.IntegrationId);
            return BadRequest("Invalid integration ID");
        }

        _logger.LogInformation("Updating request with ID: {Id}", id);
        _context.Entry(request).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await RequestExists(id))
            {
                _logger.LogWarning("Request with ID {Id} not found during update", id);
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        _logger.LogInformation("Deleting request with ID: {Id}", id);
        var request = await _context.Requests.FindAsync(id);
        
        if (request == null)
        {
            _logger.LogWarning("Request with ID {Id} not found during delete", id);
            return NotFound();
        }

        _context.Requests.Remove(request);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> RequestExists(string id)
    {
        return await _context.Requests.AnyAsync(e => e.Id == id);
    }
}
