using ApiPlayground.Core.Models;

namespace ApiPlayground.Core.Interfaces;

public interface IIntegrationService
{
    Task<IEnumerable<Integration>> GetAllAsync();
    Task<Integration?> GetByIdAsync(string id);
    Task<Integration> CreateAsync(Integration integration);
    Task<Integration?> UpdateAsync(Integration integration);
    Task<bool> DeleteAsync(string id);
    
    // Additional methods for new features
    Task<Integration?> GetIntegrationByIdAsync(string id);
    Task<Integration> CreateIntegrationAsync(Integration integration);
    Task<Integration?> UpdateIntegrationAsync(Integration integration);
}
