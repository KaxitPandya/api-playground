using ApiPlayground.Core.Models;

namespace ApiPlayground.Core.Interfaces;

public interface IRequestService
{
    Task<IEnumerable<Request>> GetByIntegrationIdAsync(string integrationId);
    Task<Request?> GetByIdAsync(string id);
    Task<Request> CreateAsync(Request request);
    Task<Request?> UpdateAsync(Request request);
    Task<bool> DeleteAsync(string id);
}
