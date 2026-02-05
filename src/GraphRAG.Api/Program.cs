using Microsoft.EntityFrameworkCore;
using GraphRAG.Application.Configuration;
using GraphRAG.Domain.Interfaces;
using GraphRAG.Domain.Services;
using GraphRAG.Infrastructure.Data;
using GraphRAG.Infrastructure.Repositories;
using GraphRAG.Infrastructure.Services;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add configuration
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("Database"));
builder.Services.Configure<SemanticKernelSettings>(builder.Configuration.GetSection("SemanticKernel"));
builder.Services.Configure<GraphRagSettings>(builder.Configuration.GetSection("GraphRag"));

// Add database context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? builder.Configuration["Database:ConnectionString"] 
    ?? "Host=localhost;Port=5432;Database=graphrag_db;Username=graphrag_user;Password=graphrag_password";

builder.Services.AddDbContext<PostgresDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Register plugins
builder.Services.AddScoped<GraphRAG.Infrastructure.AI.Plugins.GraphQueryPlugin>();
builder.Services.AddScoped<GraphRAG.Infrastructure.AI.Plugins.VectorMemoryPlugin>();
builder.Services.AddScoped<GraphRAG.Infrastructure.AI.Plugins.MedicalTerminologyPlugin>();

// Register application services`nbuilder.Services.AddScoped<GraphRAG.Application.UseCases.Interfaces.IProcessMedicalQueryUseCase, GraphRAG.Application.UseCases.ProcessMedicalQueryUseCase>();`nbuilder.Services.AddScoped<GraphRAG.Application.UseCases.Interfaces.IImportFhirDataUseCase, GraphRAG.Application.UseCases.ImportFhirDataUseCase>();
builder.Services.AddScoped<GraphRAG.Application.Interfaces.IAIService, AzureOpenAIService>();
builder.Services.AddScoped<GraphRAG.Application.Interfaces.IHybridSearchService, GraphRAG.Application.Services.HybridSearchService>();
builder.Services.AddScoped<GraphRAG.Application.Interfaces.IGraphRagService, GraphRAG.Application.Services.GraphRagService>();

// Register repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IGraphRepository>(provider =>
{
    var context = provider.GetRequiredService<PostgresDbContext>();
    return new GraphRepository(context, connectionString);
});
builder.Services.AddScoped<IVectorRepository>(provider =>
{
    var context = provider.GetRequiredService<PostgresDbContext>();
    return new VectorRepository(context, connectionString);
});
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IFhirRepository, FhirRepository>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<IMedicalTerminologyService, MedicalTerminologyService>();
builder.Services.AddScoped<GraphRAG.Application.Interfaces.IFhirMappingService, FhirMappingService>();
builder.Services.AddScoped<GraphRAG.Application.Interfaces.IFhirEtlService, FhirEtlService>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgresql");

// Add controllers
builder.Services.AddControllers();

// Add Validation
builder.Services.AddValidatorsFromAssemblyContaining<GraphRAG.Application.Validation.QueryRequestValidator>();

// Add OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Add CORS (for development)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthorization();

app.MapControllers();

// Map health check endpoint
app.MapHealthChecks("/health");

// Add a simple root endpoint
app.MapGet("/", () => new
{
    service = "GraphRAG API",
    version = "0.1.0",
    status = "running",
    endpoints = new[]
    {
        "/health - Health check",
        "/api/query - Query endpoint (POST)",
        "/api/health - Detailed health status",
        "/openapi/v1.json - OpenAPI specification"
    }
}).WithName("Root");

app.Run();


