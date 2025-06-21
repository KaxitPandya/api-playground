using ApiPlayground.API.Data;
using ApiPlayground.Core.Interfaces;
using ApiPlayground.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiPlayground.API.Services;

public class RequestService : IRequestService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RequestService> _logger;

    public RequestService(
        ApplicationDbContext context, 
        ILogger<RequestService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Request>> GetByIntegrationIdAsync(string integrationId)
    {
        _logger.LogInformation("Getting requests for integration: {IntegrationId}", integrationId);
        return await _context.Requests
            .Where(r => r.IntegrationId == integrationId)
            .OrderBy(r => r.Order)
            .ToListAsync();
    }

    public async Task<Request?> GetByIdAsync(string id)
    {
        _logger.LogInformation("Getting request: {RequestId}", id);
        return await _context.Requests.FindAsync(id);
    }

    public async Task<Request> CreateAsync(Request request)
    {
        _logger.LogInformation("Creating request: {RequestName} for integration: {IntegrationId}", 
            request.Name, request.IntegrationId);
        
        request.Id = Guid.NewGuid().ToString();
        request.CreatedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;
        
        // Ensure we have the right order if not specified
        if (request.Order == 0)
        {
            var maxOrder = await _context.Requests
                .Where(r => r.IntegrationId == request.IntegrationId)
                .MaxAsync(r => (int?)r.Order) ?? -1;
            
            request.Order = maxOrder + 1;
        }
        
        _context.Requests.Add(request);
        await _context.SaveChangesAsync();
        
        return request;
    }

    public async Task<Request?> UpdateAsync(Request request)
    {
        _logger.LogInformation("Updating request: {RequestId}", request.Id);
        
        var existingRequest = await _context.Requests.FindAsync(request.Id);
        
        if (existingRequest == null)
        {
            _logger.LogWarning("Request not found: {RequestId}", request.Id);
            return null;
        }
        
        existingRequest.Name = request.Name;
        existingRequest.Method = request.Method;
        existingRequest.Url = request.Url;
        existingRequest.Headers = request.Headers;
        existingRequest.Body = request.Body;
        existingRequest.Order = request.Order;
        existingRequest.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return existingRequest;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        _logger.LogInformation("Deleting request: {RequestId}", id);
        
        var request = await _context.Requests.FindAsync(id);
        
        if (request == null)
        {
            _logger.LogWarning("Request not found: {RequestId}", id);
            return false;
        }
        
        _context.Requests.Remove(request);
        await _context.SaveChangesAsync();
        
        return true;
    }
}
