using Microsoft.EntityFrameworkCore;
using GraphRAG.Infrastructure.Data;
using GraphRAG.Application.Configuration;
using GraphRAG.Domain.Interfaces;
using GraphRAG.Infrastructure.Repositories;

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

// Register application services
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

// Add health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgresql");

// Add controllers
builder.Services.AddControllers();

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
