# Этап 2: Архитектура GraphRAG_net10

## 2.1. Обзор архитектуры

Проект GraphRAG_net10 построен на принципах **Clean Architecture** (Чистая архитектура) с четким разделением ответственности между слоями. Архитектура обеспечивает:
- Независимость бизнес-логики от инфраструктуры
- Тестируемость компонентов
- Гибкость замены технологий
- Масштабируемость системы

## 2.2. Диаграмма слоев

```
┌─────────────────────────────────────────────────────────────┐
│                    GraphRAG.Api (REST API)                  │
│  Controllers │ Middleware │ Configuration │ Health Checks   │
├─────────────────────────────────────────────────────────────┤
│                  GraphRAG.Application                        │
│     Services │ Use Cases │ DTOs │ SK Plugins │ Interfaces   │
├─────────────────────────────────────────────────────────────┤
│                   GraphRAG.Domain                            │
│   Entities │ Value Objects │ Interfaces │ Domain Services   │
└─────────────────────────────────────────────────────────────┘
                            ▲
                            │ зависит от
                            │
┌─────────────────────────────────────────────────────────────┐
│                 GraphRAG.Infrastructure                      │
│  Database │ Repositories │ External Services │ Security     │
│  PostgreSQL │ Apache AGE │ pgvector │ ONNX │ SK │ FHIR     │
└─────────────────────────────────────────────────────────────┘
```

## 2.3. Описание слоев

### 2.3.1. GraphRAG.Domain (Доменный слой)

**Назначение**: Ядро системы, содержит бизнес-логику и доменные модели

**Принципы**:
- Не зависит от других слоев
- Не содержит ссылок на фреймворки
- Содержит только чистую бизнес-логику
- Определяет контракты через интерфейсы

**Компоненты**:

#### Entities (Сущности)
```
Domain/Entities/
├── Medical/
│   ├── Patient.cs              # Пациент (ID, имя, дата рождения)
│   ├── Condition.cs            # Диагноз/состояние (код, дата, статус)
│   ├── MedicationRequest.cs    # Назначение препарата
│   └── Observation.cs          # Наблюдение/измерение
├── Graph/
│   ├── GraphNode.cs            # Узел графа (ID, метка, свойства)
│   ├── GraphEdge.cs            # Ребро графа (начало, конец, тип)
│   └── Concept.cs              # Терминологический концепт (SNOMED/LOINC)
├── AI/
│   ├── Embedding.cs            # Векторное представление
│   ├── GnnScore.cs             # Оценка узла от GNN
│   └── AttentionWeight.cs      # Вес внимания GAT
└── Core/
    ├── Tenant.cs               # Тенант (организация)
    ├── User.cs                 # Пользователь системы
    └── Conversation.cs         # Диалог с системой
```

#### Value Objects (Объекты-значения)
```
Domain/ValueObjects/
├── FhirResourceId.cs           # Идентификатор FHIR ресурса
├── ConceptCode.cs              # Код терминологии (система + код)
├── EmbeddingVector.cs          # Вектор эмбеддинга (float[])
└── TenantId.cs                 # Идентификатор тенанта (UUID)
```

#### Interfaces (Интерфейсы репозиториев)
```
Domain/Interfaces/
├── IDocumentRepository.cs      # CRUD для документов
├── IGraphRepository.cs         # Cypher запросы к графу
├── IVectorRepository.cs        # Векторный поиск
├── IConversationRepository.cs  # История диалогов
└── IFhirRepository.cs          # Работа с FHIR ресурсами
```

### 2.3.2. GraphRAG.Application (Прикладной слой)

**Назначение**: Реализация бизнес-логики и use cases

**Принципы**:
- Оркестрирует взаимодействие компонентов
- Зависит только от Domain слоя
- Реализует паттерн Use Case
- Содержит DTOs для передачи данных

**Компоненты**:

#### Services (Сервисы)
```
Application/Services/
├── GraphRagService.cs              # Основной RAG pipeline оркестратор
├── EntityExtractionService.cs     # NER для медицинских сущностей
├── HybridSearchService.cs          # Комбинация векторного + графового поиска
├── GnnInferenceService.cs          # Инференс GNN моделей (ONNX)
├── ExplainabilityService.cs        # Извлечение attention weights (XAI)
└── FhirImportService.cs            # Импорт FHIR данных в граф
```

#### DTOs (Data Transfer Objects)
```
Application/DTOs/
├── QueryRequest.cs                 # Запрос пользователя
├── QueryResponse.cs                # Ответ системы
├── GraphContext.cs                 # Контекст из графа знаний
├── VectorContext.cs                # Контекст из векторного поиска
├── ExplanationResult.cs            # Результат объяснения (XAI)
└── FhirImportResult.cs             # Результат импорта FHIR
```

#### Semantic Kernel Plugins
```
Application/Plugins/
├── GraphQueryPlugin.cs             # Плагин для Cypher запросов
│   └── [KernelFunction] ExecuteCypher(string query)
├── VectorMemoryPlugin.cs           # Плагин для векторного поиска
│   └── [KernelFunction] SearchNotes(string query, int topK)
└── TerminologyPlugin.cs            # Плагин для нормализации терминов
    └── [KernelFunction] NormalizeEntity(string entity)
```

### 2.3.3. GraphRAG.Infrastructure (Инфраструктурный слой)

**Назначение**: Реализация взаимодействия с внешними системами

**Принципы**:
- Реализует интерфейсы из Domain
- Зависит от конкретных технологий
- Содержит технические детали

**Компоненты**:

#### Database
```
Infrastructure/Database/
├── PostgreSQL/
│   ├── PostgresDbContext.cs        # EF Core контекст
│   ├── EntityConfigurations/       # Конфигурация сущностей
│   └── Migrations/                 # EF миграции
├── ApacheAge/
│   ├── ApacheAgeClient.cs          # Клиент для Cypher запросов
│   ├── CypherQueryBuilder.cs       # Построитель запросов
│   └── GraphResultParser.cs        # Парсер результатов
└── PgVector/
    ├── PgVectorClient.cs           # Клиент для векторного поиска
    └── HnswIndexManager.cs         # Управление HNSW индексами
```

#### Repositories
```
Infrastructure/Repositories/
├── DocumentRepository.cs           # Реализация IDocumentRepository
├── GraphRepository.cs              # Реализация IGraphRepository
├── VectorRepository.cs             # Реализация IVectorRepository
├── ConversationRepository.cs       # Реализация IConversationRepository
└── FhirRepository.cs               # Реализация IFhirRepository
```

#### External Services
```
Infrastructure/ExternalServices/
├── OnnxRuntime/
│   ├── OnnxRuntimeService.cs       # Инференс ONNX моделей
│   └── TensorConverter.cs          # Преобразование данных в тензоры
├── SemanticKernel/
│   ├── SemanticKernelService.cs    # Интеграция с SK
│   ├── PromptBuilder.cs            # Построение промптов
│   └── AzureOpenAIConfig.cs        # Конфигурация Azure OpenAI
└── FhirMapping/
    ├── FhirMappingService.cs       # Маппинг FHIR → Graph
    ├── PatientMapper.cs            # Маппер Patient ресурса
    ├── ConditionMapper.cs          # Маппер Condition ресурса
    └── MedicationRequestMapper.cs  # Маппер MedicationRequest
```

#### Security
```
Infrastructure/Security/
├── TenantIsolationMiddleware.cs    # Инъекция tenant_id в контекст
├── AuditLogger.cs                  # Логирование доступа
└── RowLevelSecurityPolicy.cs       # Настройка RLS политик
```

### 2.3.4. GraphRAG.Api (API слой)

**Назначение**: REST API endpoints

**Компоненты**:

#### Controllers
```
Api/Controllers/
├── QueryController.cs              # POST /api/query - RAG запросы
├── ExplanationController.cs        # GET /api/explanation/{id}
├── AdminController.cs              # POST /api/admin/import-fhir
└── HealthController.cs             # GET /api/health
```

#### Middleware
```
Api/Middleware/
├── ExceptionHandlingMiddleware.cs  # Глобальная обработка ошибок
├── RequestLoggingMiddleware.cs     # Логирование запросов
└── AuthenticationMiddleware.cs     # Аутентификация/авторизация
```

#### Configuration
```
Api/Configuration/
├── DependencyInjection.cs          # Настройка DI контейнера
├── DatabaseConfiguration.cs        # Конфигурация БД
└── SwaggerConfiguration.cs         # Настройка Swagger/OpenAPI
```

## 2.4. Взаимодействие слоев

### Поток данных GraphRAG запроса

```
1. Клиент → API Controller
   POST /api/query { query: "Можно ли назначить препарат X?" }

2. API → Application Service
   QueryController.ProcessQuery() → GraphRagService.ProcessQuery()

3. Application → Domain Interfaces
   GraphRagService использует:
   - EntityExtractionService (извлечение сущностей)
   - HybridSearchService (гибридный поиск)
   - GnnInferenceService (ранжирование узлов)

4. Domain Interfaces → Infrastructure Implementations
   - IGraphRepository → GraphRepository → ApacheAgeClient
   - IVectorRepository → VectorRepository → PgVectorClient
   - IConversationRepository → ConversationRepository → PostgresDbContext

5. Infrastructure → External Systems
   - ApacheAgeClient → PostgreSQL (Apache AGE Cypher)
   - PgVectorClient → PostgreSQL (pgvector KNN)
   - OnnxRuntimeService → GNN модель (medical_gat.onnx)
   - SemanticKernelService → Azure OpenAI API

6. Response → Client
   API возвращает QueryResponse с объяснением
```

## 2.5. Выбор технологий

### 2.5.1. База данных

**Выбор**: PostgreSQL 18 + Apache AGE + pgvector

**Обоснование**:
- **Единая платформа**: Реляционные, графовые и векторные данные в одной БД
- **Apache AGE**: Полноценный openCypher для графовых запросов
- **pgvector**: HNSW индексы для быстрого KNN поиска
- **RLS (Row Level Security)**: Встроенная изоляция мультитенантов
- **Зрелость**: PostgreSQL - надежная и проверенная СУБД

**Альтернативы рассмотрены**:
- Neo4j + PostgreSQL (отдельные БД, сложность синхронизации)
- MongoDB Atlas Vector Search (NoSQL, меньше гарантий)

### 2.5.2. Векторное хранилище

**Выбор**: pgvector с HNSW индексами

**Параметры**:
```sql
CREATE INDEX embeddings_hnsw_idx ON embeddings 
USING hnsw (embedding vector_l2_ops)
WITH (m = 16, ef_construction = 64);
```

**Обоснование**:
- Встроено в PostgreSQL (меньше инфраструктуры)
- HNSW - оптимальный алгоритм для KNN (точность + скорость)
- Поддержка фильтрации (WHERE tenant_id = ?)

### 2.5.3. Графовая БД

**Выбор**: Apache AGE (внутри PostgreSQL)

**Обоснование**:
- openCypher - стандартный язык графовых запросов
- Транзакционность PostgreSQL
- Нет необходимости в синхронизации данных

**Пример запроса**:
```cypher
MATCH (p:Patient {id: 'patient-123'})
      -[:HAS_CONDITION]->(c:Condition)
      -[:CONTRAINDICATED_WITH]->(m:Medication)
RETURN m.name, c.code
LIMIT 10
```

### 2.5.4. LLM и эмбеддинги

**Выбор**: Azure OpenAI Service

**Модели**:
- **Chat Completion**: gpt-4o или gpt-4-turbo
- **Embeddings**: text-embedding-3-large (3072 dims)

**Обоснование**:
- Enterprise SLA и безопасность
- HIPAA compliance
- Региональное размещение (EU/US)
- Предсказуемая стоимость

**Абстракция**:
```csharp
interface ILlmService {
    Task<string> GenerateResponse(string prompt);
    Task<float[]> GenerateEmbedding(string text);
}
```

### 2.5.5. GNN Framework

**Выбор**: PyTorch Geometric → ONNX Runtime (.NET)

**Обоснование**:
- PyTorch Geometric - лучший фреймворк для GNN
- ONNX - кросс-платформенный формат
- ONNX Runtime в .NET - высокая производительность
- Разделение обучения (Python) и инференса (.NET)

## 2.6. Масштабируемость и деплой

### 2.6.1. Контейнеризация

**Docker образы**:
```
graphrag/postgres18-age:1.0     # PostgreSQL + AGE + pgvector
graphrag/api:latest              # .NET API
graphrag/ml-training:latest      # Python GNN обучение (опционально)
```

**Docker Compose** (локальная разработка):
```yaml
services:
  postgres:
    image: graphrag/postgres18-age:1.0
    environment:
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    volumes:
      - pgdata:/var/lib/postgresql/data
  
  api:
    image: graphrag/api:latest
    depends_on:
      - postgres
    environment:
      ConnectionStrings__DefaultConnection: ${DB_CONN}
      AzureOpenAI__Endpoint: ${OPENAI_ENDPOINT}
```

### 2.6.2. Kubernetes деплой

**Компоненты**:
- **Deployment**: API pods (HPA 3-10 replicas)
- **StatefulSet**: PostgreSQL (master + read replicas)
- **ConfigMap**: Конфигурация приложения
- **Secret**: Credentials (DB, OpenAI API keys)
- **Service**: Load balancer для API
- **Ingress**: TLS termination

**Целевая инфраструктура**: Azure Kubernetes Service (AKS) или AWS EKS

### 2.6.3. Логирование и мониторинг

**Логирование**:
- **Serilog** - структурированное логирование
- **Seq / Elasticsearch** - агрегация логов
- **Application Insights** (Azure) - телеметрия

**Мониторинг**:
- **Prometheus** - сбор метрик
- **Grafana** - дашборды
- **OpenTelemetry** - distributed tracing

**Ключевые метрики**:
- Request latency (P50, P95, P99)
- Throughput (RPS)
- Error rate (%)
- Database query time
- GNN inference time
- LLM API latency

## 2.7. Безопасность

### 2.7.1. Аутентификация и авторизация

- **JWT tokens** для API аутентификации
- **Role-based access control** (RBAC)
- **Tenant isolation** через RLS
- **API keys** для сервисных аккаунтов

### 2.7.2. Шифрование

- **TLS 1.3** для всех соединений
- **pgcrypto** для чувствительных данных в БД
- **Azure Key Vault** для secrets management

### 2.7.3. Compliance

- **HIPAA** - защита PHI (Protected Health Information)
- **GDPR** - право на удаление данных
- **Audit logging** - pgaudit для всех операций

## 2.8. Структура Solution

```
GraphRAG_net10.sln
├── src/
│   ├── GraphRAG.Domain/            (Entities, Interfaces, Value Objects)
│   ├── GraphRAG.Application/       (Services, DTOs, Plugins)
│   ├── GraphRAG.Infrastructure/    (Repositories, External Services)
│   └── GraphRAG.Api/               (Controllers, Middleware)
└── tests/
    └── GraphRAG.Tests/             (Unit, Integration, Performance)
```

**Зависимости проектов**:
```
GraphRAG.Api 
  → GraphRAG.Application 
    → GraphRAG.Domain ← GraphRAG.Infrastructure
```

## 2.9. Критерии завершения этапа

- ✅ Архитектура спроектирована и задокументирована
- ✅ Выбраны и обоснованы технологии
- ✅ Определена структура solution и проектов
- ✅ Спроектированы основные интерфейсы и сущности
- ✅ Определен подход к масштабированию и деплою
- ✅ Зафиксированы принципы безопасности

**Статус**: ✅ Завершено  
**Дата**: 04.02.2026