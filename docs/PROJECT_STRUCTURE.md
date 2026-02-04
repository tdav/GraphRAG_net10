# GraphRAG .NET Project Structure

## Обзор структуры проекта / Project Structure Overview

Проект организован по принципам Clean Architecture с четким разделением слоев и ответственностей.

```
GraphRAG_net10/
├── GraphRAG.slnx                          # Solution file (.NET 10 format)
├── README.md                               # Main project readme
├── .gitignore                              # Git ignore rules
├── docs/                                   # Documentation
│   ├── DEVELOPMENT_PLAN.md                 # Detailed development plan
│   ├── PROJECT_STRUCTURE.md                # This file
│   ├── stages/                             # Development stages
│   │   ├── stage-01-analysis.md
│   │   └── stage-02-architecture.md
│   └── quests/                             # Development quests
│       ├── quest-01-analysis.md
│       └── quest-02-architecture.md
├── src/                                    # Source code
│   ├── GraphRAG.Domain/                    # Domain layer
│   │   ├── Entities/                       # Domain entities
│   │   ├── ValueObjects/                   # Value objects
│   │   ├── Interfaces/                     # Domain interfaces
│   │   └── Enums/                          # Enumerations
│   ├── GraphRAG.Application/               # Application layer
│   │   ├── Services/                       # Business services
│   │   ├── DTOs/                           # Data transfer objects
│   │   ├── Interfaces/                     # Application interfaces
│   │   └── Plugins/                        # Semantic Kernel plugins
│   ├── GraphRAG.Infrastructure/            # Infrastructure layer
│   │   ├── Database/                       # Database implementations
│   │   │   ├── PostgreSQL/                 # PostgreSQL specific
│   │   │   ├── ApacheAge/                  # Apache AGE client
│   │   │   └── PgVector/                   # pgvector client
│   │   ├── Repositories/                   # Repository implementations
│   │   ├── ExternalServices/               # External service clients
│   │   │   ├── OnnxRuntime/                # ONNX model inference
│   │   │   ├── SemanticKernel/             # SK integration
│   │   │   └── FhirMapping/                # FHIR mapping
│   │   └── Security/                       # Security implementations
│   └── GraphRAG.Api/                       # API layer
│       ├── Controllers/                    # API controllers
│       ├── Middleware/                     # Custom middleware
│       ├── Models/                         # API models
│       └── Configuration/                  # App configuration
├── tests/                                  # Tests
│   └── GraphRAG.Tests/                     # Test project
│       ├── Unit/                           # Unit tests
│       ├── Integration/                    # Integration tests
│       └── Performance/                    # Performance tests
└── Техническое Задание- GraphRAG на .NET (1).pdf  # Technical specification
```

---

## Описание слоев / Layer Description

### 1. GraphRAG.Domain
**Назначение**: Ядро системы, содержащее бизнес-логику и доменные сущности.

**Принципы**:
- Независим от внешних зависимостей
- Содержит только чистую бизнес-логику
- Не зависит от фреймворков и технологий

**Основные компоненты**:

#### Entities (Сущности)
- `Patient` - Пациент
- `Condition` - Диагноз/состояние
- `MedicationRequest` - Назначение лекарства
- `Observation` - Наблюдение/измерение
- `Concept` - Терминологический концепт
- `GraphNode` - Узел графа знаний
- `GraphEdge` - Ребро графа знаний
- `Embedding` - Векторное представление
- `Conversation` - Беседа/диалог

#### ValueObjects (Объекты-значения)
- `FhirResourceId` - Идентификатор FHIR ресурса
- `ConceptCode` - Код терминологии (SNOMED CT, LOINC, RxNorm)
- `EmbeddingVector` - Вектор эмбеддинга
- `TenantId` - Идентификатор тенанта

#### Interfaces (Интерфейсы)
- `IDocumentRepository` - Репозиторий документов
- `IGraphRepository` - Репозиторий графа
- `IVectorRepository` - Репозиторий векторов
- `IConversationRepository` - Репозиторий бесед

---

### 2. GraphRAG.Application
**Назначение**: Реализация use cases и бизнес-логики приложения.

**Принципы**:
- Оркестрирует взаимодействие между слоями
- Реализует use cases
- Зависит только от Domain слоя

**Основные компоненты**:

#### Services (Сервисы)
- `GraphRagService` - Основной сервис RAG pipeline
- `EntityExtractionService` - Извлечение медицинских сущностей (NER)
- `HybridSearchService` - Гибридный поиск (векторный + графовый)
- `GnnInferenceService` - Инференс GNN моделей
- `ExplainabilityService` - Извлечение объяснений (XAI)

#### DTOs (Data Transfer Objects)
- `QueryRequest` - Запрос пользователя
- `QueryResponse` - Ответ системы
- `GraphContext` - Контекст из графа знаний
- `VectorContext` - Контекст из векторного поиска
- `ExplanationResult` - Результат объяснения

#### Plugins (Плагины Semantic Kernel)
- `GraphQueryPlugin` - Плагин для Cypher запросов
- `VectorMemoryPlugin` - Плагин для векторного поиска
- `TerminologyPlugin` - Плагин для нормализации терминов

---

### 3. GraphRAG.Infrastructure
**Назначение**: Реализация взаимодействия с внешними системами и технологиями.

**Принципы**:
- Реализует интерфейсы из Domain
- Содержит технические детали
- Зависит от конкретных технологий

**Основные компоненты**:

#### Database
- `PostgresDbContext` - EF Core контекст
- `ApacheAgeClient` - Клиент для Apache AGE (Cypher)
- `PgVectorClient` - Клиент для pgvector

#### Repositories
- `DocumentRepository` - Реализация IDocumentRepository
- `GraphRepository` - Реализация IGraphRepository
- `VectorRepository` - Реализация IVectorRepository

#### External Services
- `OnnxRuntimeService` - Инференс ONNX моделей
- `SemanticKernelService` - Интеграция с Microsoft Semantic Kernel
- `FhirMappingService` - Маппинг FHIR → Graph
- `AzureOpenAIService` - Клиент Azure OpenAI

#### Security
- `TenantIsolationMiddleware` - RLS для мультитенантности
- `AuditLogger` - Логирование доступа к данным

---

### 4. GraphRAG.Api
**Назначение**: REST API для взаимодействия с системой.

**Принципы**:
- Тонкий слой над Application
- Только маршрутизация и валидация
- Swagger/OpenAPI документация

**Основные компоненты**:

#### Controllers
- `QueryController` - Обработка RAG запросов
  - `POST /api/query` - Основной endpoint
  - `GET /api/explanation/{queryId}` - Детальное объяснение
- `AdminController` - Административные операции
  - `POST /api/admin/import-fhir` - Импорт FHIR данных
  - `GET /api/admin/graph-stats` - Статистика графа
- `HealthController` - Health checks
  - `GET /api/health` - Статус системы

#### Middleware
- `ExceptionHandlingMiddleware` - Обработка ошибок
- `RequestLoggingMiddleware` - Логирование запросов
- `AuthenticationMiddleware` - Аутентификация

---

### 5. GraphRAG.Tests
**Назначение**: Тестирование всех компонентов системы.

**Основные компоненты**:

#### Unit Tests
- `FhirMappingTests` - Тесты маппинга FHIR
- `GnnInferenceTests` - Тесты GNN инференса
- `HybridSearchTests` - Тесты гибридного поиска

#### Integration Tests
- `GraphRagIntegrationTests` - E2E тесты RAG pipeline
- `DatabaseIntegrationTests` - Тесты работы с БД
- `ApiIntegrationTests` - Тесты API endpoints

#### Performance Tests
- `LoadTests` - Нагрузочное тестирование
- `GraphTraversalBenchmarks` - Бенчмарки графовых обходов

---

## Зависимости между проектами / Project Dependencies

```
GraphRAG.Api
    ├── GraphRAG.Application
    │   └── GraphRAG.Domain
    └── GraphRAG.Infrastructure
        ├── GraphRAG.Application
        └── GraphRAG.Domain

GraphRAG.Tests
    ├── GraphRAG.Domain
    ├── GraphRAG.Application
    └── GraphRAG.Infrastructure
```

**Правила зависимостей**:
1. Domain не зависит ни от кого
2. Application зависит только от Domain
3. Infrastructure зависит от Domain и Application
4. Api зависит от всех слоев
5. Tests может зависеть от всех слоев

---

## Конфигурация / Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=graphrag;Username=graphrag_user;Password=***"
  },
  "AzureOpenAI": {
    "Endpoint": "https://your-openai-resource.openai.azure.com/",
    "ApiKey": "***",
    "DeploymentName": "gpt-4",
    "EmbeddingDeploymentName": "text-embedding-ada-002"
  },
  "GNN": {
    "ModelPath": "./models/medical_gat.onnx",
    "ScoreThreshold": 0.5
  },
  "Graph": {
    "MaxTraversalDepth": 3,
    "BatchSize": 1000
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

---

## Команды для работы / Working Commands

### Сборка проекта
```bash
# Сборка всего решения
dotnet build

# Сборка конкретного проекта
dotnet build src/GraphRAG.Api/GraphRAG.Api.csproj

# Восстановление зависимостей
dotnet restore
```

### Запуск приложения
```bash
# Запуск API
dotnet run --project src/GraphRAG.Api/GraphRAG.Api.csproj

# Запуск с hot reload
dotnet watch --project src/GraphRAG.Api/GraphRAG.Api.csproj
```

### Тестирование
```bash
# Запуск всех тестов
dotnet test

# Запуск с покрытием кода
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=opencover

# Запуск конкретного теста
dotnet test --filter "FullyQualifiedName~FhirMappingTests"
```

### Миграции БД (Entity Framework)
```bash
# Создание миграции
dotnet ef migrations add InitialCreate --project src/GraphRAG.Infrastructure --startup-project src/GraphRAG.Api

# Применение миграций
dotnet ef database update --project src/GraphRAG.Infrastructure --startup-project src/GraphRAG.Api
```

---

## Стандарты кодирования / Coding Standards

### Naming Conventions
- **Classes**: PascalCase (e.g., `GraphRagService`)
- **Interfaces**: IPascalCase (e.g., `IGraphRepository`)
- **Methods**: PascalCase (e.g., `ProcessQuery`)
- **Properties**: PascalCase (e.g., `QueryText`)
- **Parameters**: camelCase (e.g., `queryRequest`)
- **Private fields**: _camelCase (e.g., `_graphRepository`)

### Code Organization
```csharp
// 1. Using statements
using System;
using GraphRAG.Domain.Entities;

// 2. Namespace
namespace GraphRAG.Application.Services;

// 3. Class documentation
/// <summary>
/// Main service for GraphRAG query processing.
/// </summary>
public class GraphRagService
{
    // 4. Private fields
    private readonly IGraphRepository _graphRepository;
    private readonly ILogger<GraphRagService> _logger;
    
    // 5. Constructor
    public GraphRagService(
        IGraphRepository graphRepository,
        ILogger<GraphRagService> logger)
    {
        _graphRepository = graphRepository;
        _logger = logger;
    }
    
    // 6. Public methods
    public async Task<QueryResponse> ProcessQuery(QueryRequest request)
    {
        // Implementation
    }
    
    // 7. Private methods
    private string BuildPrompt(GraphContext context)
    {
        // Implementation
    }
}
```

### Async/Await Guidelines
- Все I/O операции должны быть асинхронными
- Использовать `ConfigureAwait(false)` в библиотеках
- Избегать `async void` (кроме event handlers)

### Exception Handling
```csharp
public async Task<QueryResponse> ProcessQuery(QueryRequest request)
{
    try
    {
        // Validate input
        if (request == null)
            throw new ArgumentNullException(nameof(request));
            
        // Process query
        var result = await _graphService.ExecuteAsync(request);
        return result;
    }
    catch (GraphQueryException ex)
    {
        _logger.LogError(ex, "Graph query failed for tenant {TenantId}", request.TenantId);
        throw new ApplicationException("Failed to process query", ex);
    }
}
```

---

## Следующие шаги / Next Steps

1. **Реализовать базовые сущности в Domain слое**
   - Patient, Condition, MedicationRequest, etc.
   
2. **Настроить PostgreSQL 18 с расширениями**
   - Apache AGE
   - pgvector
   - Создать базовую схему БД

3. **Реализовать Infrastructure слой**
   - ApacheAgeClient
   - PgVectorClient
   - Repository implementations

4. **Создать Semantic Kernel плагины**
   - GraphQueryPlugin
   - VectorMemoryPlugin
   - TerminologyPlugin

5. **Реализовать GraphRAG pipeline**
   - EntityExtractionService
   - HybridSearchService
   - GnnInferenceService
   - ExplainabilityService

---

**Версия**: 1.0  
**Дата**: 04.02.2026  
**Статус**: Initial Structure Created
