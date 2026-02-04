# Phase II: Backend Core - Детальный План
## Недели 7-14 (8 недель разработки)

**Дата создания**: 04.02.2026  
**Версия**: 1.0  
**Статус**: Запланировано

---

## 🎯 Цель фазы

Реализовать полнофункциональный backend с основными сервисами, интеграциями с внешними системами (Azure OpenAI, FHIR), и Semantic Kernel плагинами для GraphRAG pipeline.

---

## 📋 Общая структура Phase II

```
Phase II (8 недель)
├── Этап 1: Domain Layer Extensions (2 недели)
├── Этап 2: Infrastructure - Database (2 недели)
├── Этап 3: Infrastructure - External Services (1 неделя)
├── Этап 4: Application Layer (1 неделя)
├── Этап 5: Semantic Kernel Plugins (1 неделя)
└── Этап 6: FHIR ETL Pipeline (2 недели)
```

---

## 📅 Этап 1: Domain Layer Extensions (Недели 7-8)

### Задачи

#### 1.1 Value Objects (3 дня)

**Value Objects** - неизменяемые объекты для представления типов предметной области.

##### FhirResourceId
```csharp
public record FhirResourceId
{
    public string ResourceType { get; init; }  // "Patient", "Condition", etc.
    public string Id { get; init; }            // "patient-123"
    
    public string ToReference() => $"{ResourceType}/{Id}";
    public static FhirResourceId Parse(string reference);
}
```

##### ConceptCode
```csharp
public record ConceptCode
{
    public string System { get; init; }    // "http://snomed.info/sct"
    public string Code { get; init; }      // "73211009"
    public string Display { get; init; }   // "Diabetes mellitus"
    
    public bool IsSnomedCt() => System.Contains("snomed");
    public bool IsLoinc() => System.Contains("loinc");
    public bool IsRxNorm() => System.Contains("rxnorm");
}
```

##### EmbeddingVector
```csharp
public record EmbeddingVector
{
    public float[] Values { get; init; }
    public int Dimensions => Values.Length;
    
    public float CosineSimilarity(EmbeddingVector other);
    public static EmbeddingVector FromJson(string json);
}
```

**Критерии приёмки**:
- ✅ Все Value Objects immutable (record types)
- ✅ Валидация в конструкторах
- ✅ Unit тесты (100% coverage)

---

#### 1.2 Domain Events (2 дня)

**Domain Events** - события для асинхронной обработки изменений.

##### PatientImportedEvent
```csharp
public record PatientImportedEvent : IDomainEvent
{
    public Guid PatientId { get; init; }
    public Guid TenantId { get; init; }
    public string FhirPatientId { get; init; }
    public DateTime ImportedAt { get; init; }
}
```

##### GraphNodeCreatedEvent
```csharp
public record GraphNodeCreatedEvent : IDomainEvent
{
    public Guid NodeId { get; init; }
    public Guid TenantId { get; init; }
    public string NodeLabel { get; init; }
    public long AgeVertexId { get; init; }
    public DateTime CreatedAt { get; init; }
}
```

##### QueryCompletedEvent
```csharp
public record QueryCompletedEvent : IDomainEvent
{
    public Guid ConversationId { get; init; }
    public Guid UserId { get; init; }
    public string Query { get; init; }
    public string Response { get; init; }
    public TimeSpan ProcessingTime { get; init; }
    public int NodesRetrieved { get; init; }
    public DateTime CompletedAt { get; init; }
}
```

**Критерии приёмки**:
- ✅ Events являются immutable records
- ✅ MediatR integration для обработки
- ✅ Event handlers для каждого события

---

#### 1.3 Domain Services (2 дня)

##### ValidationService
```csharp
public interface IValidationService
{
    Task<ValidationResult> ValidatePatient(Patient patient);
    Task<ValidationResult> ValidateFhirBundle(Bundle bundle);
    Task<ValidationResult> ValidateQuery(QueryRequest request);
}
```

##### MedicalTerminologyService
```csharp
public interface IMedicalTerminologyService
{
    Task<ConceptCode?> NormalizeToSnomedCt(string conceptName);
    Task<string> ExpandAcronym(string acronym);
    Task<IEnumerable<string>> GetSynonyms(ConceptCode code);
}
```

**Критерии приёмки**:
- ✅ Interfaces в Domain layer
- ✅ Implementations в Infrastructure layer
- ✅ Unit тесты для бизнес-логики

---

## 📅 Этап 2: Infrastructure - Database (Недели 8-10)

### Задачи

#### 2.1 Repository Implementations (5 дней)

##### PostgresRepository<T>
Базовая реализация для всех entities:
```csharp
public class PostgresRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly PostgresDbContext _context;
    private readonly DbSet<T> _dbSet;
    
    public async Task<T?> GetByIdAsync(Guid id, Guid tenantId);
    public async Task<IEnumerable<T>> GetAllAsync(Guid tenantId);
    public async Task<T> CreateAsync(T entity);
    public async Task<T> UpdateAsync(T entity);
    public async Task DeleteAsync(Guid id, Guid tenantId);
}
```

##### GraphRepository
Работа с Apache AGE:
```csharp
public class GraphRepository : IGraphRepository
{
    private readonly NpgsqlConnection _connection;
    private readonly string _graphName = "medical_graph";
    
    public async Task<IEnumerable<GraphNode>> ExecuteCypherQuery(
        string query, 
        object parameters
    );
    
    public async Task<Graph> GetSubgraph(
        Guid nodeId, 
        int depth, 
        Guid tenantId
    );
    
    public async Task<long> CreateNode(
        string label, 
        Dictionary<string, object> properties
    );
    
    public async Task<long> CreateEdge(
        long fromVertexId, 
        long toVertexId, 
        string edgeType,
        Dictionary<string, object> properties
    );
}
```

##### VectorRepository
Работа с pgvector:
```csharp
public class VectorRepository : IVectorRepository
{
    private readonly PostgresDbContext _context;
    
    public async Task<IEnumerable<Embedding>> SearchSimilar(
        float[] queryVector,
        int topK,
        Guid tenantId,
        double threshold = 0.7
    );
    
    public async Task<Guid> CreateEmbedding(
        string content,
        float[] vector,
        string entityType,
        Guid entityId,
        Guid tenantId
    );
}
```

##### ConversationRepository
```csharp
public class ConversationRepository : IConversationRepository
{
    public async Task<Conversation?> GetByIdAsync(Guid id, Guid tenantId);
    public async Task<IEnumerable<Conversation>> GetUserConversations(
        Guid userId, 
        Guid tenantId
    );
    public async Task AddMessage(
        Guid conversationId,
        string role,
        string content
    );
}
```

##### FhirRepository
```csharp
public class FhirRepository : IFhirRepository
{
    public async Task<ImportResult> ImportFhirBundle(
        Bundle bundle, 
        Guid tenantId
    );
    
    public async Task<Patient?> GetPatientByFhirId(
        string fhirId, 
        Guid tenantId
    );
}
```

**Критерии приёмки**:
- ✅ Все repository methods реализованы
- ✅ RLS автоматически применяется через tenant_id
- ✅ Транзакционная поддержка
- ✅ Integration тесты с Testcontainers

---

#### 2.2 Database Optimizations (3 дня)

##### Batch Operations
```csharp
public class BatchOperationService
{
    public async Task<int> BulkInsertNodes(
        IEnumerable<GraphNode> nodes
    );
    
    public async Task<int> BulkInsertEmbeddings(
        IEnumerable<Embedding> embeddings
    );
}
```

##### Unit of Work Pattern
```csharp
public interface IUnitOfWork : IDisposable
{
    IRepository<Patient> Patients { get; }
    IRepository<Condition> Conditions { get; }
    IGraphRepository Graph { get; }
    IVectorRepository Vector { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

##### Query Optimization
- Eager loading для связанных entities
- Compiled queries для часто используемых запросов
- Index hints для сложных запросов

**Критерии приёмки**:
- ✅ Batch insert >1000 records/sec
- ✅ Транзакции работают корректно
- ✅ Нет N+1 query проблем

---

#### 2.3 EF Core Migrations (2 дня)

```bash
# Создание миграции
dotnet ef migrations add InitialCreate \
  --project src/GraphRAG.Infrastructure \
  --startup-project src/GraphRAG.Api

# Применение миграции
dotnet ef database update \
  --project src/GraphRAG.Infrastructure \
  --startup-project src/GraphRAG.Api
```

**Seed данные**:
```csharp
public class DatabaseSeeder
{
    public async Task SeedDevelopmentData()
    {
        // Создать тестового tenant
        // Создать тестового пользователя
        // Загрузить медицинские концепции (SNOMED CT sample)
        // Создать sample граф
    }
}
```

**Критерии приёмки**:
- ✅ Миграция создаёт все таблицы
- ✅ Seed данные загружаются
- ✅ Rollback миграций работает

---

## 📅 Этап 3: Infrastructure - External Services (Недели 10-11)

### Задачи

#### 3.1 AI Services (3 дня)

##### AzureOpenAIService
```csharp
public interface IAzureOpenAIService
{
    Task<float[]> GenerateEmbeddingAsync(string text);
    Task<string> GenerateChatCompletionAsync(
        string systemPrompt,
        string userMessage,
        List<ChatMessage> history = null
    );
}

public class AzureOpenAIService : IAzureOpenAIService
{
    // Использует Azure.AI.OpenAI SDK
    // Модели: text-embedding-ada-002, gpt-4
}
```

##### EntityExtractionService
```csharp
public interface IEntityExtractionService
{
    Task<IEnumerable<MedicalEntity>> ExtractEntities(string text);
}

public record MedicalEntity
{
    public string Text { get; init; }
    public string Type { get; init; }  // "Condition", "Medication", "Procedure"
    public double Confidence { get; init; }
    public ConceptCode? Code { get; init; }
}
```

Подходы:
- **Вариант 1**: Regex + словари медицинских терминов
- **Вариант 2**: ML.NET + предобученная NER модель
- **Вариант 3**: LLM-based extraction через GPT-4

##### SemanticKernelConfiguration
```csharp
public class SemanticKernelConfiguration
{
    public static IKernel ConfigureKernel(IServiceProvider services)
    {
        var kernel = new KernelBuilder()
            .WithAzureOpenAIChatCompletion(...)
            .WithAzureOpenAITextEmbedding(...)
            .Build();
            
        // Register plugins
        kernel.ImportFunctions(services.GetService<GraphQueryPlugin>());
        kernel.ImportFunctions(services.GetService<VectorMemoryPlugin>());
        kernel.ImportFunctions(services.GetService<TerminologyPlugin>());
        
        return kernel;
    }
}
```

**Критерии приёмки**:
- ✅ Embeddings генерируются корректно (1536 dimensions)
- ✅ Chat completion работает
- ✅ Entity extraction точность >80%
- ✅ Semantic Kernel инициализируется

---

#### 3.2 FHIR Services (2 дня)

##### FhirMappingService
```csharp
public interface IFhirMappingService
{
    Task<Patient> MapPatient(Hl7.Fhir.Model.Patient fhirPatient, Guid tenantId);
    Task<Condition> MapCondition(Hl7.Fhir.Model.Condition fhirCondition, Guid tenantId);
    Task<MedicationRequest> MapMedicationRequest(
        Hl7.Fhir.Model.MedicationRequest fhirMedRequest, 
        Guid tenantId
    );
    Task<Observation> MapObservation(
        Hl7.Fhir.Model.Observation fhirObservation, 
        Guid tenantId
    );
}
```

**Маппинг правила**:

```
FHIR Patient
├── → Patient entity (id, name, birthDate, gender)
└── → GraphNode (label: "Patient", properties: {...})

FHIR Condition
├── → Condition entity (id, code, onsetDate, severity)
├── → GraphNode (label: "Condition", properties: {...})
└── → GraphEdge (type: "HAS_CONDITION", from: Patient, to: Condition)

FHIR MedicationRequest
├── → MedicationRequest entity (id, medication, dosage, status)
├── → GraphNode (label: "Medication", properties: {...})
└── → GraphEdge (type: "PRESCRIBED", from: Patient, to: Medication)

FHIR Observation
├── → Observation entity (id, code, value, effectiveDate)
├── → GraphNode (label: "Observation", properties: {...})
└── → GraphEdge (type: "HAS_OBSERVATION", from: Patient, to: Observation)
```

##### FhirValidationService
```csharp
public interface IFhirValidationService
{
    Task<ValidationResult> ValidateBundle(Bundle bundle);
    Task<bool> IsValidResource<T>(T resource) where T : Resource;
}
```

**Критерии приёмки**:
- ✅ Маппинг всех 4 типов ресурсов
- ✅ Reference resolution работает
- ✅ Валидация по FHIR спецификации

---

#### 3.3 Graph Services (2 дня)

##### GraphTraversalService
```csharp
public interface IGraphTraversalService
{
    Task<IEnumerable<GraphPath>> FindPaths(
        Guid fromNodeId,
        Guid toNodeId,
        int maxDepth = 3
    );
    
    Task<IEnumerable<GraphNode>> GetNeighbors(
        Guid nodeId,
        string edgeType = null,
        int hops = 1
    );
}
```

##### SubgraphExtractionService
```csharp
public interface ISubgraphExtractionService
{
    Task<Graph> ExtractRelevantSubgraph(
        IEnumerable<string> entityNames,
        Guid tenantId,
        int maxNodes = 100
    );
}
```

**Критерии приёмки**:
- ✅ Поиск путей работает
- ✅ Подграфы извлекаются корректно
- ✅ Производительность <500ms для графов <1000 узлов

---

## 📅 Этап 4: Application Layer (Недели 11-12)

### Задачи

#### 4.1 Core Services (4 дня)

##### GraphRagService
```csharp
public interface IGraphRagService
{
    Task<QueryResponse> ProcessQueryAsync(
        QueryRequest request,
        Guid userId,
        Guid tenantId
    );
}

public class GraphRagService : IGraphRagService
{
    private readonly IEntityExtractionService _entityExtractor;
    private readonly IHybridSearchService _hybridSearch;
    private readonly IKernel _semanticKernel;
    
    public async Task<QueryResponse> ProcessQueryAsync(
        QueryRequest request,
        Guid userId,
        Guid tenantId
    )
    {
        // 1. Extract medical entities from query
        var entities = await _entityExtractor.ExtractEntities(request.Query);
        
        // 2. Perform hybrid search (vector + graph)
        var searchContext = await _hybridSearch.HybridSearch(
            request.Query, 
            entities, 
            tenantId
        );
        
        // 3. Assemble context for LLM
        var context = AssembleContext(searchContext);
        
        // 4. Generate response using Semantic Kernel
        var response = await _semanticKernel.InvokeAsync(
            "GenerateMedicalResponse",
            new KernelArguments
            {
                ["context"] = context,
                ["query"] = request.Query
            }
        );
        
        // 5. Save to conversation history
        await SaveToConversation(userId, request, response);
        
        return new QueryResponse
        {
            Answer = response.ToString(),
            RelevantNodes = searchContext.GraphNodes,
            Sources = searchContext.Documents
        };
    }
}
```

##### HybridSearchService
```csharp
public interface IHybridSearchService
{
    Task<SearchContext> HybridSearch(
        string query,
        IEnumerable<MedicalEntity> entities,
        Guid tenantId
    );
}

public class HybridSearchService : IHybridSearchService
{
    private readonly IVectorRepository _vectorRepo;
    private readonly IGraphRepository _graphRepo;
    private readonly IAzureOpenAIService _openAI;
    
    public async Task<SearchContext> HybridSearch(
        string query,
        IEnumerable<MedicalEntity> entities,
        Guid tenantId
    )
    {
        // 1. Vector search
        var queryEmbedding = await _openAI.GenerateEmbeddingAsync(query);
        var similarDocuments = await _vectorRepo.SearchSimilar(
            queryEmbedding, 
            topK: 10, 
            tenantId
        );
        
        // 2. Graph search
        var graphNodes = await _graphRepo.FindNodesByEntities(
            entities.Select(e => e.Text),
            tenantId
        );
        
        var subgraph = await _graphRepo.GetSubgraph(
            graphNodes.First().Id,
            depth: 2,
            tenantId
        );
        
        // 3. Fusion and ranking
        var rankedResults = FuseAndRank(similarDocuments, subgraph);
        
        return new SearchContext
        {
            Documents = similarDocuments,
            GraphNodes = graphNodes,
            Subgraph = subgraph,
            RankedResults = rankedResults
        };
    }
}
```

**Критерии приёмки**:
- ✅ End-to-end query processing работает
- ✅ Hybrid search возвращает релевантные результаты
- ✅ Ответы сохраняются в историю

---

#### 4.2 DTOs и Validation (2 дня)

```csharp
public record QueryRequest
{
    public string Query { get; init; }
    public Guid? PatientId { get; init; }
    public Dictionary<string, object>? Context { get; init; }
}

public record QueryResponse
{
    public string Answer { get; init; }
    public List<GraphNode> RelevantNodes { get; init; }
    public List<Document> Sources { get; init; }
    public TimeSpan ProcessingTime { get; init; }
}

public record SearchContext
{
    public IEnumerable<Embedding> Documents { get; init; }
    public IEnumerable<GraphNode> GraphNodes { get; init; }
    public Graph Subgraph { get; init; }
    public IEnumerable<RankedResult> RankedResults { get; init; }
}
```

**FluentValidation**:
```csharp
public class QueryRequestValidator : AbstractValidator<QueryRequest>
{
    public QueryRequestValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(1000);
    }
}
```

**Критерии приёмки**:
- ✅ Все DTOs immutable (records)
- ✅ Валидация на всех входных данных

---

#### 4.3 Use Cases (1 день)

```csharp
public class ProcessMedicalQueryUseCase
{
    public async Task<QueryResponse> ExecuteAsync(
        QueryRequest request,
        Guid userId,
        Guid tenantId
    );
}

public class ImportFhirDataUseCase
{
    public async Task<ImportResult> ExecuteAsync(
        Bundle fhirBundle,
        Guid tenantId
    );
}

public class ExplainReasoningUseCase
{
    public async Task<ExplanationResult> ExecuteAsync(
        Guid queryId
    );
}
```

**Критерии приёмки**:
- ✅ Use cases отделены от controllers
- ✅ Следуют Single Responsibility Principle

---

## 📅 Этап 5: Semantic Kernel Plugins (Недели 12-13)

### Задачи

#### 5.1 Graph Plugin (2 дня)

```csharp
public class GraphQueryPlugin
{
    private readonly IGraphRepository _graphRepo;
    
    [KernelFunction]
    [Description("Execute a Cypher query on the medical knowledge graph")]
    public async Task<string> ExecuteCypherQuery(
        [Description("The Cypher query to execute")] string query
    )
    {
        var results = await _graphRepo.ExecuteCypherQuery(query, null);
        return JsonSerializer.Serialize(results);
    }
    
    [KernelFunction]
    [Description("Get a subgraph around a specific medical entity")]
    public async Task<string> GetSubgraph(
        [Description("The name of the entity")] string entityName,
        [Description("The depth of the subgraph (default: 2)")] int depth = 2
    )
    {
        // Find node by name
        // Extract subgraph
        // Return as JSON
    }
    
    [KernelFunction]
    [Description("Find paths between two medical concepts")]
    public async Task<string> FindPaths(
        [Description("Source concept")] string from,
        [Description("Target concept")] string to,
        [Description("Maximum path length")] int maxDepth = 3
    )
    {
        // Find paths in graph
        // Return as JSON
    }
}
```

---

#### 5.2 Vector Memory Plugin (1 день)

```csharp
public class VectorMemoryPlugin
{
    private readonly IVectorRepository _vectorRepo;
    private readonly IAzureOpenAIService _openAI;
    
    [KernelFunction]
    [Description("Search for similar clinical documents")]
    public async Task<string> SearchDocuments(
        [Description("The search query")] string query,
        [Description("Number of results (default: 10)")] int topK = 10
    )
    {
        var embedding = await _openAI.GenerateEmbeddingAsync(query);
        var results = await _vectorRepo.SearchSimilar(embedding, topK, tenantId);
        return JsonSerializer.Serialize(results);
    }
    
    [KernelFunction]
    [Description("Find similar medical concepts")]
    public async Task<string> GetSimilarConcepts(
        [Description("The concept name")] string conceptName
    )
    {
        // Generate embedding
        // Search similar concepts
        // Return as JSON
    }
}
```

---

#### 5.3 Terminology Plugin (2 дня)

```csharp
public class TerminologyPlugin
{
    private readonly IMedicalTerminologyService _terminology;
    
    [KernelFunction]
    [Description("Normalize a medical term to SNOMED CT")]
    public async Task<string> NormalizeEntityName(
        [Description("The medical term to normalize")] string term
    )
    {
        var code = await _terminology.NormalizeToSnomedCt(term);
        return code?.Display ?? term;
    }
    
    [KernelFunction]
    [Description("Map a term to a standard medical code")]
    public async Task<string> MapToStandardCode(
        [Description("The medical term")] string term,
        [Description("Code system (SNOMED, LOINC, RxNorm)")] string system = "SNOMED"
    )
    {
        // Map to appropriate code
        // Return code as JSON
    }
    
    [KernelFunction]
    [Description("Expand medical acronyms and abbreviations")]
    public async Task<string> ExpandAcronyms(
        [Description("Text containing acronyms")] string text
    )
    {
        return await _terminology.ExpandAcronym(text);
    }
}
```

---

#### 5.4 Planner Configuration (1 день)

```csharp
public class SemanticKernelPlanner
{
    public static async Task<string> ExecutePlan(
        IKernel kernel,
        string goal
    )
    {
        var planner = new SequentialPlanner(kernel);
        var plan = await planner.CreatePlanAsync(goal);
        
        var result = await kernel.RunAsync(plan);
        return result.ToString();
    }
}
```

**Example plan**:
```
Goal: "What medications are contraindicated for a patient with diabetes?"

Plan:
1. NormalizeEntityName("diabetes") → "Diabetes mellitus (disorder)"
2. GetSubgraph("Diabetes mellitus", depth: 2)
3. ExecuteCypherQuery("MATCH (d:Condition {name: 'Diabetes mellitus'})-[:CONTRAINDICATED_WITH]->(m:Medication) RETURN m")
4. Return results
```

**Критерии приёмки**:
- ✅ Все 3 plugin работают
- ✅ Planner выбирает правильные функции
- ✅ Integration тесты для планов

---

## 📅 Этап 6: FHIR ETL Pipeline (Недели 13-14)

### Задачи

#### 6.1 FHIR Bundle Processing (3 дня)

```csharp
public class FhirEtlPipeline
{
    public async Task<ImportResult> ImportBundleAsync(
        Bundle bundle,
        Guid tenantId
    )
    {
        // 1. Validate bundle
        var validation = await _validator.ValidateBundle(bundle);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);
        
        // 2. Extract resources
        var patients = bundle.Entry
            .Where(e => e.Resource is Hl7.Fhir.Model.Patient)
            .Select(e => e.Resource as Hl7.Fhir.Model.Patient);
        
        var conditions = bundle.Entry
            .Where(e => e.Resource is Hl7.Fhir.Model.Condition)
            .Select(e => e.Resource as Hl7.Fhir.Model.Condition);
        
        // ... аналогично для других ресурсов
        
        // 3. Process in transaction
        await using var transaction = await _uow.BeginTransactionAsync();
        try
        {
            var result = new ImportResult();
            
            // Process patients
            foreach (var fhirPatient in patients)
            {
                var patient = await _mapper.MapPatient(fhirPatient, tenantId);
                await _patientRepo.CreateAsync(patient);
                result.PatientsImported++;
            }
            
            // Process conditions
            foreach (var fhirCondition in conditions)
            {
                var condition = await _mapper.MapCondition(fhirCondition, tenantId);
                await _conditionRepo.CreateAsync(condition);
                result.ConditionsImported++;
            }
            
            // ... аналогично для других ресурсов
            
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

**Критерии приёмки**:
- ✅ Парсинг FHIR JSON работает
- ✅ Валидация по FHIR спецификации
- ✅ Batch операции транзакционны

---

#### 6.2 Resource Mapping (3 дня)

**Детальный маппинг Patient**:
```csharp
public async Task<Patient> MapPatient(
    Hl7.Fhir.Model.Patient fhirPatient,
    Guid tenantId
)
{
    var patient = new Patient
    {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        FhirPatientId = fhirPatient.Id,
        GivenName = fhirPatient.Name?.FirstOrDefault()?.Given?.FirstOrDefault(),
        FamilyName = fhirPatient.Name?.FirstOrDefault()?.Family,
        BirthDate = fhirPatient.BirthDate != null 
            ? DateTime.Parse(fhirPatient.BirthDate) 
            : null,
        Gender = fhirPatient.Gender?.ToString(),
        FhirDataJson = fhirPatient.ToJson()
    };
    
    // Create graph node
    var nodeId = await _graphRepo.CreateNode(
        label: "Patient",
        properties: new Dictionary<string, object>
        {
            ["fhir_id"] = patient.FhirPatientId,
            ["name"] = $"{patient.GivenName} {patient.FamilyName}",
            ["birth_date"] = patient.BirthDate,
            ["tenant_id"] = tenantId.ToString()
        }
    );
    
    patient.AgeVertexId = nodeId;
    
    return patient;
}
```

**Reference Resolution**:
```csharp
public async Task ResolveReferences(Bundle bundle, Guid tenantId)
{
    var referenceMap = new Dictionary<string, Guid>();
    
    // First pass: create all entities, collect IDs
    // Second pass: resolve references, create edges
    
    foreach (var condition in conditions)
    {
        // Find patient by FHIR reference
        var patientRef = condition.Subject.Reference; // "Patient/123"
        var patientId = referenceMap[patientRef];
        
        // Create edge in graph
        await _graphRepo.CreateEdge(
            fromVertexId: patientNode.AgeVertexId,
            toVertexId: conditionNode.AgeVertexId,
            edgeType: "HAS_CONDITION",
            properties: new { severity = condition.Severity }
        );
    }
}
```

**Критерии приёмки**:
- ✅ Маппинг всех 4 ресурсов
- ✅ Reference resolution корректен
- ✅ Граф корректно строится

---

#### 6.3 Terminology Processing (2 дня)

```csharp
public async Task ProcessTerminologies(Bundle bundle, Guid tenantId)
{
    var concepts = new HashSet<ConceptCode>();
    
    // Extract all concept codes from bundle
    foreach (var condition in conditions)
    {
        foreach (var coding in condition.Code.Coding)
        {
            concepts.Add(new ConceptCode
            {
                System = coding.System,
                Code = coding.Code,
                Display = coding.Display
            });
        }
    }
    
    // Create Concept entities
    foreach (var code in concepts)
    {
        var concept = new Concept
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            System = code.System,
            Code = code.Code,
            Display = code.Display
        };
        
        await _conceptRepo.CreateAsync(concept);
        
        // Create graph node
        await _graphRepo.CreateNode(
            label: "Concept",
            properties: new Dictionary<string, object>
            {
                ["system"] = concept.System,
                ["code"] = concept.Code,
                ["display"] = concept.Display
            }
        );
    }
}
```

**Критерии приёмки**:
- ✅ SNOMED CT коды обрабатываются
- ✅ LOINC коды обрабатываются
- ✅ RxNorm коды обрабатываются
- ✅ Concept graph строится

---

#### 6.4 Performance Optimization (2 дня)

**Batch Insert**:
```csharp
public async Task<int> BulkInsertPatients(IEnumerable<Patient> patients)
{
    const int batchSize = 1000;
    var batches = patients.Chunk(batchSize);
    
    var totalInserted = 0;
    foreach (var batch in batches)
    {
        await _context.Patients.AddRangeAsync(batch);
        totalInserted += await _context.SaveChangesAsync();
    }
    
    return totalInserted;
}
```

**Parallel Processing**:
```csharp
public async Task<ImportResult> ImportBundleParallel(
    Bundle bundle,
    Guid tenantId
)
{
    var tasks = new[]
    {
        Task.Run(() => ProcessPatients(bundle, tenantId)),
        Task.Run(() => ProcessConditions(bundle, tenantId)),
        Task.Run(() => ProcessMedications(bundle, tenantId)),
        Task.Run(() => ProcessObservations(bundle, tenantId))
    };
    
    await Task.WhenAll(tasks);
    
    // Resolve references after all entities created
    await ResolveReferences(bundle, tenantId);
}
```

**Критерии приёмки**:
- ✅ Throughput >1000 records/sec
- ✅ Параллельная обработка работает
- ✅ No deadlocks или race conditions

---

## ✅ Критерии завершения Phase II

### Функциональные критерии
- ✅ FHIR Bundle успешно импортируется в граф
- ✅ Можно выполнить запрос: "What medications are prescribed for diabetes patients?"
- ✅ Гибридный поиск возвращает релевантные результаты (vector + graph)
- ✅ Semantic Kernel planner выбирает правильные плагины
- ✅ Entity extraction извлекает медицинские сущности с точностью >80%
- ✅ Все CRUD операции работают через repositories

### Технические критерии
- ✅ Integration тесты покрытие >80%
- ✅ Нет critical security уязвимостей
- ✅ Build проходит без warnings
- ✅ Документация API в Swagger

### Производительные критерии
- ✅ FHIR import throughput >1000 records/sec
- ✅ Hybrid search latency <3s (без GNN)
- ✅ Graph traversal <500ms для подграфов до 1000 узлов
- ✅ No memory leaks (проверить с dotMemory)

---

## 📊 Метрики Phase II

### Deliverables
- [ ] 5 Repository implementations (с unit of work)
- [ ] 3 External service integrations (Azure OpenAI, FHIR, Graph)
- [ ] 3 Semantic Kernel plugins
- [ ] 2 Core services (GraphRagService, HybridSearchService)
- [ ] FHIR ETL pipeline (полный цикл)
- [ ] 20+ Integration тесты
- [ ] API endpoints с Swagger docs

### Статистика кода (ожидается)
- **C# классов**: ~50
- **Строк кода**: ~5,000-7,000
- **Тестов**: ~100+
- **API endpoints**: ~10

---

## 📚 Ресурсы и зависимости

### NuGet пакеты (добавить в Phase II)
```xml
<!-- AI/ML -->
<PackageReference Include="Microsoft.SemanticKernel" Version="1.30.0" />
<PackageReference Include="Azure.AI.OpenAI" Version="2.0.0" />

<!-- Database -->
<PackageReference Include="Npgsql" Version="9.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />
<PackageReference Include="Pgvector" Version="0.2.0" />

<!-- FHIR -->
<PackageReference Include="Hl7.Fhir.R4" Version="5.10.0" />

<!-- Validation -->
<PackageReference Include="FluentValidation" Version="11.10.0" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />

<!-- Testing -->
<PackageReference Include="Testcontainers.PostgreSql" Version="3.10.0" />
<PackageReference Include="xUnit" Version="2.9.0" />
```

### Внешние сервисы
- **Azure OpenAI**: API key и endpoint (получить от Azure)
- **PostgreSQL**: Docker container или cloud instance
- **Apache AGE**: Включено в PostgreSQL container

---

## 🎯 Следующие шаги после Phase II

После завершения Phase II можно начинать:
- **Phase III**: Обучение GNN модели (PyTorch GAT)
- **Phase IV**: Интеграция XAI и полный GraphRAG pipeline

---

**Статус**: 📋 Запланировано  
**Дата создания плана**: 04.02.2026  
**Ожидаемое начало**: После завершения Phase I (оставшиеся 30%)  
**Длительность**: 8 недель  
**Команда**: 2-3 backend разработчика
