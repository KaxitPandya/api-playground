using ApiPlayground.API.Data;
using ApiPlayground.Core.Interfaces;
using ApiPlayground.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiPlayground.API.Services;

public class IntegrationService : IIntegrationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<IntegrationService> _logger;

    public IntegrationService(
        ApplicationDbContext context, 
        ILogger<IntegrationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Integration>> GetAllAsync()
    {
        _logger.LogInformation("Getting all integrations");
        return await _context.Integrations.ToListAsync();
    }

    public async Task<Integration?> GetByIdAsync(string id)
    {
        _logger.LogInformation("Getting integration with ID: {IntegrationId}", id);
        return await _context.Integrations
            .Include(i => i.Requests.OrderBy(r => r.Order))
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Integration> CreateAsync(Integration integration)
    {
        _logger.LogInformation("Creating integration: {IntegrationName}", integration.Name);
        
        integration.Id = Guid.NewGuid().ToString();
        integration.CreatedAt = DateTime.UtcNow;
        integration.UpdatedAt = DateTime.UtcNow;
        
        _context.Integrations.Add(integration);
        await _context.SaveChangesAsync();
        
        return integration;
    }

    public async Task<Integration?> UpdateAsync(Integration integration)
    {
        _logger.LogInformation("Updating integration: {IntegrationId}", integration.Id);
        
        var existingIntegration = await _context.Integrations.FindAsync(integration.Id);
        
        if (existingIntegration == null)
        {
            _logger.LogWarning("Integration not found: {IntegrationId}", integration.Id);
            return null;
        }
        
        existingIntegration.Name = integration.Name;
        existingIntegration.Description = integration.Description;
        existingIntegration.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return existingIntegration;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        _logger.LogInformation("Deleting integration: {IntegrationId}", id);
        
        var integration = await _context.Integrations.FindAsync(id);
        
        if (integration == null)
        {
            _logger.LogWarning("Integration not found: {IntegrationId}", id);
            return false;
        }
        
        _context.Integrations.Remove(integration);
        await _context.SaveChangesAsync();
        
        return true;
    }

    // Additional method implementations for new features
    public async Task<Integration?> GetIntegrationByIdAsync(string id)
    {
        return await GetByIdAsync(id);
    }

    public async Task<Integration> CreateIntegrationAsync(Integration integration)
    {
        return await CreateAsync(integration);
    }

    public async Task<Integration?> UpdateIntegrationAsync(Integration integration)
    {
        return await UpdateAsync(integration);
    }
}
