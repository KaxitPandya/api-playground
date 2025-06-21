using ApiPlayground.Core.Interfaces;
using ApiPlayground.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ApiPlayground.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OpenAPIController : ControllerBase
    {
        private readonly IOpenAPIImportService _openApiImportService;
        private readonly ILogger<OpenAPIController> _logger;

        public OpenAPIController(IOpenAPIImportService openApiImportService, ILogger<OpenAPIController> logger)
        {
            _openApiImportService = openApiImportService;
            _logger = logger;
        }

        /// <summary>
        /// Import integration from OpenAPI specification URL
        /// </summary>
        /// <param name="request">OpenAPI import request</param>
        /// <returns>Imported integration details</returns>
        [HttpPost("import-url")]
        [HttpPost("import/url")] // Alternative route for frontend compatibility
        public async Task<ActionResult<OpenAPIImportResponse>> ImportFromUrl([FromBody] OpenAPIImportRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Url))
                {
                    return BadRequest("URL is required");
                }

                _logger.LogInformation("Importing OpenAPI from URL: {Url}", request.Url);

                var result = await _openApiImportService.ImportFromUrlAsync(
                    request.Url, 
                    request.BaseUrl, 
                    request.SelectedOperations);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing OpenAPI from URL: {Url}", request.Url);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Import integration from OpenAPI specification file content
        /// </summary>
        /// <param name="request">OpenAPI import request with file content</param>
        /// <returns>Imported integration details</returns>
        [HttpPost("import-file")]
        [HttpPost("import/file")] // Alternative route for frontend compatibility
        public async Task<ActionResult<OpenAPIImportResponse>> ImportFromFile([FromBody] OpenAPIImportRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.FileContent))
                {
                    return BadRequest("File content is required");
                }

                _logger.LogInformation("Importing OpenAPI from file content");

                var result = await _openApiImportService.ImportFromFileContentAsync(
                    request.FileContent, 
                    request.BaseUrl, 
                    request.SelectedOperations);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing OpenAPI from file content");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Get available operations from OpenAPI specification URL
        /// </summary>
        /// <param name="url">OpenAPI specification URL</param>
        /// <returns>List of available operations</returns>
        [HttpGet("operations")]
        public async Task<ActionResult<List<string>>> GetAvailableOperationsFromUrl([FromQuery] string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    return BadRequest("URL is required");
                }

                _logger.LogInformation("Getting available operations from URL: {Url}", url);

                var operations = await _openApiImportService.GetAvailableOperationsAsync(url, true);
                
                return Ok(operations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting operations from URL: {Url}", url);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Get available operations from OpenAPI specification file content
        /// </summary>
        /// <param name="request">Request containing file content</param>
        /// <returns>List of available operations</returns>
        [HttpPost("operations")]
        public async Task<ActionResult<List<string>>> GetAvailableOperationsFromFile([FromBody] JsonElement request)
        {
            try
            {
                string? fileContent = null;
                
                // Try to parse the JSON to get fileContent
                if (request.ValueKind == JsonValueKind.Object)
                {
                    if (request.TryGetProperty("fileContent", out JsonElement fileContentElement))
                    {
                        fileContent = fileContentElement.GetString();
                    }
                    else if (request.TryGetProperty("content", out JsonElement contentElement))
                    {
                        fileContent = contentElement.GetString();
                    }
                }
                
                if (string.IsNullOrEmpty(fileContent))
                {
                    return BadRequest("File content is required");
                }

                _logger.LogInformation("Getting available operations from file content");

                var operations = await _openApiImportService.GetAvailableOperationsAsync(fileContent, false);
                
                return Ok(operations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting operations from file content");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Upload OpenAPI file and get preview of available operations
        /// </summary>
        /// <param name="file">OpenAPI specification file</param>
        /// <returns>List of available operations</returns>
        [HttpPost("upload")]
        public async Task<ActionResult<object>> UploadOpenAPIFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("File is required");
                }

                if (!file.ContentType.Contains("json") && !file.ContentType.Contains("yaml") && !file.ContentType.Contains("yml"))
                {
                    return BadRequest("Only JSON and YAML files are supported");
                }

                _logger.LogInformation("Processing uploaded OpenAPI file: {FileName}", file.FileName);

                using var reader = new StreamReader(file.OpenReadStream());
                var content = await reader.ReadToEndAsync();

                var operations = await _openApiImportService.GetAvailableOperationsAsync(content, false);
                
                return Ok(new { 
                    fileName = file.FileName,
                    size = file.Length,
                    operations = operations,
                    content = content
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing uploaded OpenAPI file");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
    }
} 