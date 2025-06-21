using ApiPlayground.Core.Interfaces;
using ApiPlayground.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace ApiPlayground.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly IAIGenerationService _aiGenerationService;
        private readonly ILogger<AIController> _logger;

        public AIController(IAIGenerationService aiGenerationService, ILogger<AIController> logger)
        {
            _aiGenerationService = aiGenerationService;
            _logger = logger;
        }

        /// <summary>
        /// Generate an integration from plain English description using AI
        /// </summary>
        /// <param name="request">AI generation request</param>
        /// <returns>Generated integration with explanation and suggestions</returns>
        [HttpPost("generate")]
        public async Task<ActionResult<AIGenerationResponse>> GenerateIntegration([FromBody] AIGenerationRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Description))
                {
                    return BadRequest("Description is required");
                }

                _logger.LogInformation("Generating integration from description: {Description}", request.Description);

                var result = await _aiGenerationService.GenerateIntegrationFromDescriptionAsync(request);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating integration from description");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Get AI-powered suggestions for improving an existing integration
        /// </summary>
        /// <param name="integrationId">Integration ID</param>
        /// <returns>List of improvement suggestions</returns>
        [HttpGet("suggest/{integrationId}")]
        public async Task<ActionResult<List<string>>> SuggestImprovements(string integrationId)
        {
            try
            {
                _logger.LogInformation("Getting AI suggestions for integration: {IntegrationId}", integrationId);

                var suggestions = await _aiGenerationService.SuggestImprovementsAsync(integrationId);
                
                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting suggestions for integration: {IntegrationId}", integrationId);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Get AI-powered explanation of how an integration works
        /// </summary>
        /// <param name="integrationId">Integration ID</param>
        /// <returns>Plain English explanation of the integration</returns>
        [HttpGet("explain/{integrationId}")]
        public async Task<ActionResult<string>> ExplainIntegration(string integrationId)
        {
            try
            {
                _logger.LogInformation("Getting AI explanation for integration: {IntegrationId}", integrationId);

                var explanation = await _aiGenerationService.ExplainIntegrationAsync(integrationId);
                
                return Ok(new { explanation });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting explanation for integration: {IntegrationId}", integrationId);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
    }
} 