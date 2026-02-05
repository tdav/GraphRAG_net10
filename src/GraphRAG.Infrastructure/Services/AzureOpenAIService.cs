using GraphRAG.Application.Configuration;
using GraphRAG.Application.Interfaces;
using GraphRAG.Infrastructure.AI.Plugins;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Planning;

#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0060

namespace GraphRAG.Infrastructure.Services;

public class AzureOpenAIService : IAIService
{
    private readonly Kernel _kernel;
    private readonly ILogger<AzureOpenAIService> _logger;
    private readonly SemanticKernelSettings _settings;

    public AzureOpenAIService(
        IOptions<SemanticKernelSettings> settings,
        GraphQueryPlugin graphPlugin,
        VectorMemoryPlugin vectorPlugin,
        MedicalTerminologyPlugin terminologyPlugin,
        ILogger<AzureOpenAIService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        var builder = Kernel.CreateBuilder();

        builder.AddAzureOpenAIChatCompletion(
            _settings.ChatDeploymentName,
            _settings.AzureOpenAIEndpoint,
            _settings.AzureOpenAIApiKey);

        builder.AddAzureOpenAITextEmbeddingGeneration(
            _settings.EmbeddingDeploymentName,
            _settings.AzureOpenAIEndpoint,
            _settings.AzureOpenAIApiKey);

        _kernel = builder.Build();

        // Register plugins
        _kernel.ImportPluginFromObject(graphPlugin, "GraphQuery");
        _kernel.ImportPluginFromObject(vectorPlugin, "VectorMemory");
        _kernel.ImportPluginFromObject(terminologyPlugin, "MedicalTerminology");
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        try
        {
            var embeddingService = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();
            var embedding = await embeddingService.GenerateEmbeddingAsync(text, kernel: null, cancellationToken: cancellationToken);
            return embedding.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding");
            // Fallback for demo/development if key is missing
            return new float[1536]; 
        }
    }

    public async Task<string> GetChatCompletionAsync(string prompt, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use Stepwise Planner for complex medical reasoning that requires tool use
            var planner = new FunctionCallingStepwisePlanner(new FunctionCallingStepwisePlannerOptions
            {
                MaxIterations = 5,
                MaxTokens = _settings.MaxTokens
            });

            var result = await planner.ExecuteAsync(_kernel, prompt, cancellationToken);
            return result.FinalAnswer ?? "I could not find a definitive answer.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat completion via planner");
            
            // Fallback to simple completion if planner fails
            try
            {
                var chatService = _kernel.GetRequiredService<IChatCompletionService>();
                var result = await chatService.GetChatMessageContentAsync(prompt, new OpenAIPromptExecutionSettings
                {
                    MaxTokens = _settings.MaxTokens,
                    Temperature = _settings.Temperature
                }, kernel: _kernel, cancellationToken: cancellationToken);

                return result.Content ?? string.Empty;
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "Error in fallback chat completion");
                return "Error: AI Service unavailable.";
            }
        }
    }

    public async Task<List<string>> ExtractEntitiesAsync(string text, CancellationToken cancellationToken = default)
    {
        var prompt = $@"
            Extract medical entities (Patient, Condition, Medication, Observation) from the following clinical text.
            Return ONLY a comma-separated list of entities.
            Text: {text}
            Entities:";

        var result = await GetChatCompletionAsync(prompt, cancellationToken);
        return result.Split(',', StringSplitOptions.RemoveEmptyEntries)
                     .Select(s => s.Trim())
                     .ToList();
    }
}
