using ApiPlayground.Core.Models;

namespace ApiPlayground.Core.Interfaces
{
    public interface IOpenAPIImportService
    {
        Task<OpenAPIImportResponse> ImportFromUrlAsync(string url, string? baseUrl = null, List<string>? selectedOperations = null);
        Task<OpenAPIImportResponse> ImportFromFileContentAsync(string fileContent, string? baseUrl = null, List<string>? selectedOperations = null);
        Task<List<string>> GetAvailableOperationsAsync(string urlOrContent, bool isUrl = true);
        Task<Integration> ConvertOpenAPIToIntegrationAsync(string openApiContent, string? baseUrl = null, List<string>? selectedOperations = null);
    }
} 