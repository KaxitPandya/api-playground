using ApiPlayground.Core.Interfaces;
using ApiPlayground.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace ApiPlayground.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExecutionsController : ControllerBase
    {
        private readonly IExecutionService _executionService;
        private readonly ILogger<ExecutionsController> _logger;

        public ExecutionsController(
            IExecutionService executionService,
            ILogger<ExecutionsController> logger)
        {
            _executionService = executionService;
            _logger = logger;
        }

        /// <summary>
        /// Execute a single request by ID
        /// </summary>
        /// <param name="id">Request ID</param>
        /// <param name="placeholders">Placeholder values</param>
        /// <returns>Request execution result</returns>
        [HttpPost("request/{id}")]
        public async Task<ActionResult<RequestResult>> ExecuteRequest(string id, [FromBody] PlaceholderMap? placeholders = null)
        {
            try
            {
                _logger.LogInformation("Executing request with ID: {Id}", id);
                
                var result = await _executionService.ExecuteRequestAsync(id, placeholders?.Values ?? new Dictionary<string, string>());
                
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Request with ID {Id} not found", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing request with ID: {Id}", id);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Execute all requests in an integration (sequential by default)
        /// </summary>
        /// <param name="id">Integration ID</param>
        /// <param name="placeholders">Placeholder values</param>
        /// <returns>List of request execution results</returns>
        [HttpPost("integration/{id}")]
        public async Task<ActionResult<List<RequestResult>>> ExecuteIntegration(string id, [FromBody] PlaceholderMap? placeholders = null)
        {
            try
            {
                _logger.LogInformation("Executing integration with ID: {Id}", id);
                
                var results = await _executionService.ExecuteIntegrationAsync(id, placeholders?.Values ?? new Dictionary<string, string>());
                
                return Ok(results);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Integration with ID {Id} not found", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing integration with ID: {Id}", id);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Execute integration with advanced configuration
        /// </summary>
        /// <param name="id">Integration ID</param>
        /// <param name="config">Execution configuration</param>
        /// <returns>List of request execution results</returns>
        [HttpPost("integration/{id}/advanced")]
        public async Task<ActionResult<List<RequestResult>>> ExecuteIntegrationAdvanced(string id, [FromBody] ExecutionConfig config)
        {
            try
            {
                _logger.LogInformation("Executing integration with ID: {Id} in {Mode} mode", id, config.Mode);
                
                var results = await _executionService.ExecuteIntegrationWithConfigAsync(id, config);
                
                return Ok(results);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Integration with ID {Id} not found", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing integration with ID: {Id}", id);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Execute requests in parallel
        /// </summary>
        /// <param name="id">Integration ID</param>
        /// <param name="request">Parallel execution request</param>
        /// <returns>List of request execution results</returns>
        [HttpPost("integration/{id}/parallel")]
        public async Task<ActionResult<List<RequestResult>>> ExecuteIntegrationParallel(string id, [FromBody] ParallelExecutionRequest request)
        {
            try
            {
                _logger.LogInformation("Executing integration with ID: {Id} in parallel mode", id);
                
                var config = new ExecutionConfig
                {
                    Placeholders = request.Placeholders,
                    Mode = ExecutionMode.Parallel,
                    MaxParallelRequests = request.MaxParallelRequests,
                    TimeoutMs = request.TimeoutMs,
                    EnableRetries = request.EnableRetries
                };
                
                var results = await _executionService.ExecuteIntegrationWithConfigAsync(id, config);
                
                return Ok(results);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Integration with ID {Id} not found", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing integration in parallel with ID: {Id}", id);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Execute requests with conditional flow
        /// </summary>
        /// <param name="id">Integration ID</param>
        /// <param name="request">Conditional execution request</param>
        /// <returns>List of request execution results</returns>
        [HttpPost("integration/{id}/conditional")]
        public async Task<ActionResult<List<RequestResult>>> ExecuteIntegrationConditional(string id, [FromBody] ConditionalExecutionRequest request)
        {
            try
            {
                _logger.LogInformation("Executing integration with ID: {Id} in conditional mode", id);
                
                var config = new ExecutionConfig
                {
                    Placeholders = request.Placeholders,
                    Mode = ExecutionMode.Conditional,
                    StopOnFirstError = request.StopOnFirstError,
                    EnableRetries = request.EnableRetries
                };
                
                var results = await _executionService.ExecuteIntegrationWithConfigAsync(id, config);
                
                return Ok(results);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Integration with ID {Id} not found", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing integration conditionally with ID: {Id}", id);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

    }
}
