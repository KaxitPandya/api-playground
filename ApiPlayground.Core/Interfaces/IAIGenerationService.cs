using ApiPlayground.Core.Models;

namespace ApiPlayground.Core.Interfaces
{
    public interface IAIGenerationService
    {
        Task<AIGenerationResponse> GenerateIntegrationFromDescriptionAsync(AIGenerationRequest request);
        Task<List<string>> SuggestImprovementsAsync(string integrationId);
        Task<string> ExplainIntegrationAsync(string integrationId);
    }
} 