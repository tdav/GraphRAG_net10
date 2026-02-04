using Microsoft.AspNetCore.Mvc;
using GraphRAG.Application.DTOs;

namespace GraphRAG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QueryController : ControllerBase
{
    private readonly ILogger<QueryController> _logger;
    private readonly GraphRAG.Application.Interfaces.IGraphRagService _graphRagService;

    public QueryController(
        ILogger<QueryController> logger,
        GraphRAG.Application.Interfaces.IGraphRagService graphRagService)
    {
        _logger = logger;
        _graphRagService = graphRagService;
    }

    /// <summary>
    /// Process a GraphRAG query
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<QueryResponse>> Query([FromBody] QueryRequest request)
    {
        try
        {
            _logger.LogInformation("Processing query: {Query}", request.Query);

            if (request.Context == null
                || !request.Context.TryGetValue("tenantId", out var tenantValue)
                || !Guid.TryParse(tenantValue?.ToString(), out var tenantId))
            {
                return BadRequest(new
                {
                    error = "Missing or invalid tenantId in request context"
                });
            }

            var response = await _graphRagService.ProcessQueryAsync(
                request,
                tenantId,
                HttpContext.RequestAborted);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing query");
            return StatusCode(500, new
            {
                error = "An error occurred processing your query",
                details = ex.Message
            });
        }
    }

    /// <summary>
    /// Get query history for a conversation
    /// </summary>
    [HttpGet("conversation/{conversationId}")]
    public async Task<IActionResult> GetConversationHistory(Guid conversationId)
    {
        try
        {
            // TODO: Implement conversation history retrieval
            _logger.LogInformation("Getting conversation history for: {ConversationId}", conversationId);

            return Ok(new
            {
                conversationId,
                messages = new List<object>()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversation history");
            return StatusCode(500, new
            {
                error = "An error occurred retrieving conversation history",
                details = ex.Message
            });
        }
    }
}
