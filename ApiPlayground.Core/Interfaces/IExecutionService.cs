using ApiPlayground.Core.Models;

namespace ApiPlayground.Core.Interfaces;

public interface IExecutionService
{
    Task<List<RequestResult>> ExecuteIntegrationAsync(string integrationId, Dictionary<string, string> placeholders);
    Task<List<RequestResult>> ExecuteIntegrationWithConfigAsync(string integrationId, ExecutionConfig config);
    Task<RequestResult> ExecuteRequestAsync(string requestId, Dictionary<string, string> placeholders);
    Task<RequestResult> ExecuteRequestWithRetriesAsync(Request request, Dictionary<string, string> placeholders, RetryConfig? retryConfig = null);
    Task<List<RequestResult>> ExecuteRequestsInParallelAsync(List<Request> requests, Dictionary<string, string> placeholders, int maxParallelRequests = 5);
    Task<List<RequestResult>> ExecuteRequestsWithConditionalFlowAsync(List<Request> requests, Dictionary<string, string> placeholders);
}
