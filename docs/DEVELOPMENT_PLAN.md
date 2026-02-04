# План Разработки GraphRAG на .NET
## Detailed Development Plan for GraphRAG System with XAI

### Версия документа: 1.0
### Дата создания: 04.02.2026

---

## 1. Обзор проекта / Project Overview

### 1.1 Цель проекта
Разработка системы GraphRAG (Graph Retrieval-Augmented Generation) с объяснимым искусственным интеллектом (XAI) на базе графовых нейронных сетей (GNN) для здравоохранения.

### 1.2 Ключевые особенности системы
- **Hybrid Search**: Комбинация векторного поиска и графовых запросов
- **Explainable AI**: Визуализация путей рассуждения через GNN
- **Healthcare Focus**: Интеграция со стандартом HL7 FHIR
- **PostgreSQL 18**: Единая платформа для реляционных, графовых и векторных данных

### 1.3 Технологический стек
- **Backend**: .NET 9/10 (C#)
- **AI Framework**: Microsoft Semantic Kernel
- **Database**: PostgreSQL 18
- **Graph Engine**: Apache AGE
- **Vector Search**: pgvector
- **ML Inference**: ONNX Runtime
- **GNN**: GraphSAGE/GAT (PyTorch → ONNX)

---

## 2. Архитектура системы / System Architecture

### 2.1 Слои приложения (Clean Architecture)

#### 2.1.1 GraphRAG.Domain
**Назначение**: Ядро бизнес-логики, сущности, интерфейсы
**Компоненты**:
- Entities:
  - `Patient`, `Observation`, `MedicationRequest`
  - `GraphNode`, `GraphEdge`, `Concept`
  - `Embedding`, `Conversation`
- Interfaces:
  - `IDocumentRepository`
  - `IGraphRepository`
  - `IVectorRepository`
  - `IConversationRepository`
- Value Objects:
  - `FhirResourceId`, `ConceptCode`, `EmbeddingVector`

#### 2.1.2 GraphRAG.Application
**Назначение**: Use cases, бизнес-логика, сервисы
**Компоненты**:
- Services:
  - `GraphRagService` - основной оркестратор запросов
  - `EntityExtractionService` - NER для медицинских сущностей
  - `HybridSearchService` - комбинация векторного и графового поиска
  - `GnnInferenceService` - инференс GNN моделей
  - `ExplainabilityService` - извлечение attention weights
- DTOs:
  - `QueryRequest`, `QueryResponse`
  - `GraphContext`, `VectorContext`
  - `ExplanationResult`
- Interfaces для плагинов Semantic Kernel

#### 2.1.3 GraphRAG.Infrastructure
**Назначение**: Реализация взаимодействия с внешними системами
**Компоненты**:
- Database:
  - `PostgresDbContext`
  - `ApacheAgeClient` - клиент для Cypher запросов
  - `PgVectorClient` - векторный поиск
- Repositories:
  - `DocumentRepository`
  - `GraphRepository`
  - `VectorRepository`
  - `ConversationRepository`
- External Services:
  - `OnnxRuntimeService` - ONNX модель инференс
  - `SemanticKernelService` - интеграция с LLM
  - `FhirMappingService` - FHIR → Graph маппинг
- Security:
  - `TenantIsolationMiddleware` - RLS для мультитенантности
  - `AuditLogger` - логирование доступа

#### 2.1.4 GraphRAG.Api
**Назначение**: REST API endpoints
**Компоненты**:
- Controllers:
  - `QueryController` - обработка запросов RAG
  - `AdminController` - управление графом
  - `HealthController` - health checks
- Middleware:
  - Authentication & Authorization
  - Request/Response logging
  - Error handling
- Configuration:
  - Dependency injection setup
  - Database migrations

#### 2.1.5 GraphRAG.Tests
**Назначение**: Тестирование системы
**Компоненты**:
- Unit Tests:
  - Domain logic tests
  - Service tests
  - Mapping tests
- Integration Tests:
  - Database tests (Testcontainers)
  - API endpoint tests
  - End-to-end RAG flow tests
- Performance Tests:
  - Load testing
  - Graph traversal benchmarks

---

## 3. База данных и хранилища / Database Architecture

### 3.1 PostgreSQL 18 Schema

#### 3.1.1 Расширения (Extensions)
```sql
-- Apache AGE для графовых запросов
CREATE EXTENSION IF NOT EXISTS age;

-- pgvector для векторного поиска
CREATE EXTENSION IF NOT EXISTS vector;

-- pg_trgm для нечеткого текстового поиска
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- UUIDv7 для сортируемых идентификаторов
-- (нативно в PG18)
```

#### 3.1.2 Реляционные таблицы
```sql
-- Административные данные
CREATE TABLE tenants (
    id UUID PRIMARY KEY DEFAULT gen_uuidv7(),
    name VARCHAR(255) NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_uuidv7(),
    tenant_id UUID REFERENCES tenants(id),
    username VARCHAR(100) NOT NULL,
    role VARCHAR(50) NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- Документы и эмбеддинги
CREATE TABLE documents (
    id UUID PRIMARY KEY DEFAULT gen_uuidv7(),
    tenant_id UUID REFERENCES tenants(id),
    content TEXT NOT NULL,
    metadata JSONB,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE embeddings (
    id UUID PRIMARY KEY DEFAULT gen_uuidv7(),
    document_id UUID REFERENCES documents(id),
    vector VECTOR(1536), -- OpenAI ada-002 dimension
    chunk_index INT,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- Индексы для векторного поиска
CREATE INDEX ON embeddings USING hnsw (vector vector_cosine_ops);
```

#### 3.1.3 Графовая модель (Apache AGE)
**Узлы (Nodes)**:
- `Patient` - пациент
- `Condition` - диагноз/состояние
- `MedicationRequest` - назначение лекарства
- `Observation` - наблюдение/измерение
- `Concept` - концепт из терминологии (SNOMED CT, LOINC, RxNorm)

**Ребра (Edges)**:
- `HAS_CONDITION` - связь пациента с диагнозом
- `PRESCRIBED` - назначение лекарства
- `RELATED_TO` - связь между концептами
- `CAUSES` - причинно-следственная связь
- `CONTRAINDICATES` - противопоказание

### 3.2 Гибридный поиск (Hybrid Search)

#### 3.2.1 Функция hybrid_search_context
```sql
CREATE OR REPLACE FUNCTION hybrid_search_context(
    query_vector VECTOR(1536),
    entity_list TEXT[],
    tenant_id UUID,
    top_k INT DEFAULT 10
)
RETURNS TABLE (
    doc_id UUID,
    chunk_text TEXT,
    graph_nodes JSONB,
    relevance_score FLOAT
)
AS $$
BEGIN
    -- Step 1: Векторный поиск
    WITH vector_results AS (
        SELECT 
            e.document_id,
            d.content,
            1 - (e.vector <=> query_vector) AS vector_score
        FROM embeddings e
        JOIN documents d ON e.document_id = d.id
        WHERE d.tenant_id = tenant_id
        ORDER BY e.vector <=> query_vector
        LIMIT top_k
    ),
    -- Step 2: Графовый поиск
    graph_results AS (
        SELECT * FROM cypher('medical_graph', $$
            MATCH (p:Patient {tenant_id: $tenant_id})-[r*1..3]->(target)
            WHERE target.name IN $entity_list
            RETURN p, r, target
        $$) AS (patient agtype, relations agtype, target agtype)
    )
    -- Step 3: Слияние результатов
    -- ... fusion logic
END;
$$ LANGUAGE plpgsql;
```

---

## 4. Semantic Kernel и плагины / Semantic Kernel Integration

### 4.1 Kernel Plugins

#### 4.1.1 GraphQueryPlugin
```csharp
public class GraphQueryPlugin
{
    [KernelFunction("execute_cypher")]
    [Description("Executes Cypher query against Apache AGE graph database")]
    public async Task<string> ExecuteCypher(
        [Description("Cypher query to execute")] string query,
        [Description("Tenant ID for isolation")] string tenantId
    )
    {
        // Валидация запроса
        ValidateCypherQuery(query);
        
        // Ограничение глубины обхода (max 3 hops)
        EnforceTraversalDepth(query, maxDepth: 3);
        
        // Инъекция tenant_id
        var isolatedQuery = InjectTenantId(query, tenantId);
        
        // Выполнение запроса
        return await _ageClient.ExecuteAsync(isolatedQuery);
    }
}
```

#### 4.1.2 VectorMemoryPlugin
```csharp
public class VectorMemoryPlugin : IMemoryStore
{
    [KernelFunction("search_notes")]
    [Description("Searches medical notes using vector similarity")]
    public async Task<IEnumerable<MemoryRecord>> SearchNotes(
        [Description("Query embedding vector")] float[] queryEmbedding,
        [Description("Tenant ID")] string tenantId,
        [Description("Number of results")] int limit = 10
    )
    {
        return await _pgVectorClient.SearchAsync(
            queryEmbedding, 
            tenantId, 
            limit
        );
    }
}
```

#### 4.1.3 TerminologyPlugin
```csharp
public class TerminologyPlugin
{
    [KernelFunction("normalize_entity")]
    [Description("Normalizes medical term to standard code")]
    public async Task<ConceptCode> NormalizeEntity(
        [Description("Raw medical term")] string rawText
    )
    {
        // Поиск в локальном справочнике или внешний API
        return await _terminologyService.NormalizeAsync(rawText);
    }
}
```

### 4.2 GraphRAG Workflow

```csharp
public class GraphRagService
{
    public async Task<QueryResponse> ProcessQuery(QueryRequest request)
    {
        // 1. Извлечение сущностей (NER)
        var entities = await _entityExtraction.ExtractEntities(request.Query);
        
        // 2. Генерация подграфа
        var subgraph = await _graphRepository.ExtractSubgraph(
            entities, 
            request.TenantId,
            maxHops: 2
        );
        
        // 3. Векторное обогащение
        var queryEmbedding = await _embeddingService.CreateEmbedding(request.Query);
        var vectorContext = await _vectorRepository.Search(
            queryEmbedding, 
            request.TenantId,
            topK: 10
        );
        
        // 4. Инференс GNN
        var nodeScores = await _gnnService.ScoreNodes(subgraph);
        
        // 5. Фильтрация по весу GNN
        var filteredSubgraph = FilterByGnnScore(subgraph, nodeScores, threshold: 0.5);
        
        // 6. Формирование промпта
        var prompt = BuildPrompt(filteredSubgraph, vectorContext, request.Query);
        
        // 7. Генерация ответа через LLM
        var response = await _kernel.InvokeAsync(prompt);
        
        // 8. Извлечение объяснения (attention weights)
        var explanation = await _explainabilityService.ExtractExplanation(
            subgraph,
            nodeScores
        );
        
        return new QueryResponse
        {
            Answer = response.ToString(),
            Explanation = explanation,
            SourceNodes = filteredSubgraph.Nodes,
            SourceEdges = filteredSubgraph.Edges
        };
    }
}
```

---

## 5. GNN и ONNX Runtime / Machine Learning Component

### 5.1 Обучение модели (Python)

```python
# train_gnn.py
import torch
from torch_geometric.nn import GATConv
from torch_geometric.data import Data

class MedicalGAT(torch.nn.Module):
    def __init__(self, input_dim, hidden_dim, output_dim):
        super().__init__()
        self.conv1 = GATConv(input_dim, hidden_dim, heads=8)
        self.conv2 = GATConv(hidden_dim * 8, output_dim, heads=1)
    
    def forward(self, x, edge_index):
        x = self.conv1(x, edge_index)
        x = F.relu(x)
        x = self.conv2(x, edge_index)
        return x

# Экспорт в ONNX
model = MedicalGAT(input_dim=256, hidden_dim=128, output_dim=64)
dummy_x = torch.randn(100, 256)
dummy_edge_index = torch.randint(0, 100, (2, 500))

torch.onnx.export(
    model,
    (dummy_x, dummy_edge_index),
    "medical_gat.onnx",
    input_names=['node_features', 'edge_index'],
    output_names=['node_embeddings', 'attention_weights'],
    dynamic_axes={
        'node_features': {0: 'num_nodes'},
        'edge_index': {1: 'num_edges'},
        'node_embeddings': {0: 'num_nodes'},
        'attention_weights': {0: 'num_edges'}
    }
)
```

### 5.2 Инференс в C#

```csharp
public class GnnInferenceService
{
    private readonly InferenceSession _session;
    
    public async Task<NodeScores> ScoreNodes(GraphSubgraph subgraph)
    {
        // Подготовка тензоров
        var (nodeFeatures, edgeIndex) = PrepareInputTensors(subgraph);
        
        // Запуск инференса
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("node_features", nodeFeatures),
            NamedOnnxValue.CreateFromTensor("edge_index", edgeIndex)
        };
        
        using var results = _session.Run(inputs);
        
        var nodeEmbeddings = results
            .First(r => r.Name == "node_embeddings")
            .AsTensor<float>();
            
        var attentionWeights = results
            .First(r => r.Name == "attention_weights")
            .AsTensor<float>();
        
        return new NodeScores
        {
            Embeddings = nodeEmbeddings,
            AttentionWeights = attentionWeights
        };
    }
    
    private (DenseTensor<float>, DenseTensor<long>) PrepareInputTensors(
        GraphSubgraph subgraph)
    {
        var numNodes = subgraph.Nodes.Count;
        var numEdges = subgraph.Edges.Count;
        var featureDim = 256;
        
        // Создание тензоров
        var nodeFeatures = new DenseTensor<float>(
            new[] { numNodes, featureDim }
        );
        
        var edgeIndex = new DenseTensor<long>(
            new[] { 2, numEdges }
        );
        
        // Переиндексация глобальных ID в локальные [0, N-1]
        var nodeIdMap = subgraph.Nodes
            .Select((node, idx) => (node.Id, idx))
            .ToDictionary(x => x.Id, x => x.idx);
        
        // Заполнение node_features
        for (int i = 0; i < numNodes; i++)
        {
            var node = subgraph.Nodes[i];
            var features = ExtractNodeFeatures(node);
            
            for (int j = 0; j < featureDim; j++)
            {
                nodeFeatures[i, j] = features[j];
            }
        }
        
        // Заполнение edge_index
        for (int i = 0; i < numEdges; i++)
        {
            var edge = subgraph.Edges[i];
            edgeIndex[0, i] = nodeIdMap[edge.SourceId];
            edgeIndex[1, i] = nodeIdMap[edge.TargetId];
        }
        
        return (nodeFeatures, edgeIndex);
    }
}
```

---

## 6. Объяснимость (XAI) / Explainability

### 6.1 Извлечение Attention Weights

```csharp
public class ExplainabilityService
{
    public ExplanationResult ExtractExplanation(
        GraphSubgraph subgraph,
        NodeScores gnnScores)
    {
        // Извлечь attention weights из последнего слоя GAT
        var attentionWeights = gnnScores.AttentionWeights;
        
        // Нормировать веса
        var normalizedWeights = NormalizeWeights(attentionWeights);
        
        // Сортировать ребра по весу важности
        var rankedEdges = subgraph.Edges
            .Select((edge, idx) => new
            {
                Edge = edge,
                Weight = normalizedWeights[idx]
            })
            .OrderByDescending(x => x.Weight)
            .Take(10) // Топ-10 наиболее важных ребер
            .ToList();
        
        return new ExplanationResult
        {
            ImportantEdges = rankedEdges.Select(x => x.Edge).ToList(),
            EdgeWeights = rankedEdges.ToDictionary(
                x => x.Edge.Id, 
                x => x.Weight
            ),
            Visualization = GenerateVisualization(subgraph, rankedEdges)
        };
    }
    
    private GraphVisualization GenerateVisualization(
        GraphSubgraph subgraph,
        List<(GraphEdge Edge, float Weight)> rankedEdges)
    {
        return new GraphVisualization
        {
            Nodes = subgraph.Nodes.Select(n => new NodeViz
            {
                Id = n.Id,
                Label = n.Name,
                Type = n.Type
            }).ToList(),
            
            Edges = rankedEdges.Select(e => new EdgeViz
            {
                Source = e.Edge.SourceId,
                Target = e.Edge.TargetId,
                Weight = e.Weight,
                IsImportant = e.Weight > 0.7 // Highlight important edges
            }).ToList()
        };
    }
}
```

### 6.2 Формат JSON ответа

```json
{
  "response": "Назначение препарата X противопоказано из-за взаимодействия с текущим лечением препаратом Y через метаболический путь CYP3A4",
  "confidence": 0.89,
  "explanation": {
    "reasoning_path": [
      {
        "node_id": "node_123",
        "label": "Препарат X (Simvastatin)",
        "type": "MedicationRequest"
      },
      {
        "edge_id": "edge_456",
        "label": "METABOLIZED_BY",
        "weight": 0.92
      },
      {
        "node_id": "node_789",
        "label": "CYP3A4 Pathway",
        "type": "Concept"
      },
      {
        "edge_id": "edge_101",
        "label": "INHIBITED_BY",
        "weight": 0.87
      },
      {
        "node_id": "node_112",
        "label": "Препарат Y (Erythromycin)",
        "type": "MedicationRequest"
      }
    ],
    "graph_visualization": {
      "nodes": [...],
      "edges": [...]
    }
  },
  "sources": [
    {
      "type": "graph",
      "content": "Knowledge graph path showing drug interaction"
    },
    {
      "type": "vector",
      "content": "Clinical note from 2024-12-15 mentioning similar case",
      "relevance": 0.85
    }
  ]
}
```

---

## 7. Безопасность / Security

### 7.1 Row Level Security (RLS)

```sql
-- Включить RLS для таблиц с PII
ALTER TABLE documents ENABLE ROW LEVEL SECURITY;
ALTER TABLE embeddings ENABLE ROW LEVEL SECURITY;

-- Политика доступа на основе tenant_id
CREATE POLICY tenant_isolation_policy ON documents
    FOR ALL
    TO authenticated_user
    USING (tenant_id = current_setting('app.current_tenant_id')::UUID);

CREATE POLICY tenant_isolation_policy ON embeddings
    FOR ALL
    TO authenticated_user
    USING (
        document_id IN (
            SELECT id FROM documents 
            WHERE tenant_id = current_setting('app.current_tenant_id')::UUID
        )
    );
```

```csharp
// Middleware для инъекции tenant_id
public class TenantIsolationMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var tenantId = ExtractTenantId(context);
        
        // Установить session variable в PostgreSQL
        using var connection = _dbContext.Database.GetDbConnection();
        await connection.OpenAsync();
        
        using var command = connection.CreateCommand();
        command.CommandText = $"SET app.current_tenant_id = '{tenantId}'";
        await command.ExecuteNonQueryAsync();
        
        await next(context);
    }
}
```

### 7.2 Аудит и шифрование

```sql
-- Установка pgaudit
CREATE EXTENSION IF NOT EXISTS pgaudit;

-- Конфигурация аудита
ALTER SYSTEM SET pgaudit.log = 'read,write';
ALTER SYSTEM SET pgaudit.log_catalog = off;
ALTER SYSTEM SET pgaudit.log_parameter = on;

-- Аудит графовых запросов
CREATE TABLE cypher_audit_log (
    id UUID PRIMARY KEY DEFAULT gen_uuidv7(),
    user_id UUID,
    tenant_id UUID,
    query_text TEXT,
    execution_time_ms INT,
    timestamp TIMESTAMPTZ DEFAULT NOW()
);
```

---

## 8. ETL Pipeline для FHIR / FHIR Integration

### 8.1 Маппинг FHIR → Graph

```csharp
public class FhirToGraphMapper
{
    public async Task<GraphImportResult> ImportFhirBundle(Bundle bundle, Guid tenantId)
    {
        var cypherCommands = new List<string>();
        
        foreach (var entry in bundle.Entry)
        {
            var resource = entry.Resource;
            
            switch (resource)
            {
                case Patient patient:
                    cypherCommands.Add(CreatePatientNode(patient, tenantId));
                    break;
                    
                case Condition condition:
                    cypherCommands.Add(CreateConditionNode(condition, tenantId));
                    cypherCommands.Add(LinkConditionToPatient(condition));
                    cypherCommands.Add(LinkConditionToConcept(condition));
                    break;
                    
                case MedicationRequest medRequest:
                    cypherCommands.Add(CreateMedicationNode(medRequest, tenantId));
                    cypherCommands.Add(LinkMedicationToPatient(medRequest));
                    break;
            }
        }
        
        // Выполнение batch операций
        return await _ageClient.ExecuteBatchAsync(cypherCommands);
    }
    
    private string CreateConditionNode(Condition condition, Guid tenantId)
    {
        // MERGE для онтологических узлов (концептов)
        var conceptCode = condition.Code.Coding.FirstOrDefault()?.Code;
        
        return $@"
            MERGE (concept:Concept {{code: '{conceptCode}', system: 'SNOMED-CT'}})
            ON CREATE SET concept.display = '{condition.Code.Text}'
            
            CREATE (cond:Condition {{
                id: '{Guid.NewGuid()}',
                tenant_id: '{tenantId}',
                recorded_date: '{condition.RecordedDate}',
                clinical_status: '{condition.ClinicalStatus}'
            }})
            
            CREATE (cond)-[:HAS_CODE]->(concept)
        ";
    }
}
```

---

## 9. План реализации / Implementation Roadmap

### Фаза I: Инфраструктура (4-6 недель)
**Цель**: Развернуть базовую инфраструктуру БД и .NET проектов

#### Задачи:
1. **PostgreSQL 18 Setup**
   - [ ] Установка PostgreSQL 18
   - [ ] Компиляция Apache AGE 1.7+ из исходников
   - [ ] Установка pgvector 0.8+
   - [ ] Создание Docker образа с PG18 + AGE + pgvector
   - [ ] Тестирование совместимости расширений

2. **Базовая схема БД**
   - [ ] Создание реляционных таблиц (tenants, users, documents, embeddings)
   - [ ] Настройка графовой схемы (Apache AGE)
   - [ ] Создание индексов (HNSW, GIN, B-tree)
   - [ ] Реализация hybrid_search_context функции
   - [ ] Настройка RLS политик

3. **.NET Solution Structure**
   - [ ] Создание solution файла GraphRAG.sln
   - [ ] Проекты: Domain, Application, Infrastructure, Api, Tests
   - [ ] Настройка Dependency Injection
   - [ ] Базовая конфигурация (appsettings.json)

4. **DevOps**
   - [ ] Docker Compose для локальной разработки
   - [ ] CI/CD pipeline (GitHub Actions)
   - [ ] Настройка логирования (Serilog)

**Риски**:
- Несовместимость AGE с PG18 → Потребуется патчинг Makefile
- Нестабильность PG18 → Использовать beta/RC версии

**Критерии завершения**:
- Docker образ запускается и проходит health check
- Можно выполнить простой Cypher запрос через AGE
- Можно выполнить векторный поиск через pgvector

---

### Фаза II: Backend Core (6-8 недель)
**Цель**: Реализация основного .NET backend и Semantic Kernel плагинов

#### Задачи:
1. **Domain Layer**
   - [ ] Определение основных entities (Patient, Condition, etc.)
   - [ ] Интерфейсы репозиториев
   - [ ] Value objects и enums

2. **Infrastructure Layer**
   - [ ] NpgsqlConnection и DbContext
   - [ ] ApacheAgeClient (Cypher execution)
   - [ ] PgVectorClient (vector search)
   - [ ] Repository implementations
   - [ ] FHIR mapping service

3. **Application Layer**
   - [ ] GraphRagService - основной оркестратор
   - [ ] EntityExtractionService - NER
   - [ ] HybridSearchService
   - [ ] ExplainabilityService

4. **Semantic Kernel Integration**
   - [ ] Установка Microsoft.SemanticKernel NuGet
   - [ ] GraphQueryPlugin
   - [ ] VectorMemoryPlugin
   - [ ] TerminologyPlugin
   - [ ] Kernel configuration и planner setup

5. **FHIR Integration**
   - [ ] Установка Hl7.Fhir.R4 NuGet
   - [ ] FhirToGraphMapper
   - [ ] ETL pipeline для FHIR Bundle
   - [ ] Маппинг правила для Resource → Node
   - [ ] Терминология: SNOMED CT, LOINC, RxNorm

6. **Security**
   - [ ] TenantIsolationMiddleware
   - [ ] Автоматическая инъекция tenant_id в Cypher
   - [ ] AuditLogger для pgaudit
   - [ ] SSL/TLS конфигурация для Npgsql

**Риски**:
- Сложность маппинга FHIR → Graph → Может потребоваться упрощение модели
- Производительность Cypher запросов → Оптимизация индексов

**Критерии завершения**:
- FHIR Bundle импортируется в граф
- Можно выполнить гибридный поиск
- Semantic Kernel planner может выбирать правильные плагины

---

### Фаза III: ML & GNN (4-6 недель)
**Цель**: Обучение GNN модели и интеграция ONNX

#### Задачи:
1. **Подготовка данных для обучения (Python)**
   - [ ] Экспорт графа из PostgreSQL
   - [ ] Генерация node features (embeddings названий)
   - [ ] Создание обучающего датасета
   - [ ] Разметка релевантности ребер (если есть)

2. **Обучение GNN модели (Python)**
   - [ ] Выбор архитектуры: GraphSAGE или GAT
   - [ ] Реализация модели в PyTorch Geometric
   - [ ] Обучение на медицинских графах
   - [ ] Валидация на тестовой выборке
   - [ ] Экспорт в ONNX с dynamic axes

3. **ONNX Integration в .NET**
   - [ ] Установка Microsoft.ML.OnnxRuntime NuGet
   - [ ] GnnInferenceService
   - [ ] Преобразование AGE JSON → ONNX Tensors
   - [ ] Переиндексация node IDs (global → local)
   - [ ] Обработка dynamic batch sizes

4. **Тестирование инференса**
   - [ ] Unit тесты для tensor подготовки
   - [ ] Валидация выходов модели
   - [ ] Performance benchmarks

**Риски**:
- Проблемы с dynamic axes в ONNX → Использовать fixed batch padding
- Недостаток размеченных данных → Использовать самообучение

**Критерии завершения**:
- GNN модель экспортирована в ONNX
- Инференс работает в .NET
- Время инференса < 500ms для подграфов до 1000 узлов

---

### Фаза IV: GraphRAG & XAI (6-8 недель)
**Цель**: Интеграция полного GraphRAG pipeline с объяснимостью

#### Задачи:
1. **GraphRAG Workflow**
   - [ ] Полная имплементация ProcessQuery метода
   - [ ] Извлечение сущностей (NER) через ML.NET или ONNX
   - [ ] Генерация подграфа через Cypher
   - [ ] Векторный поиск заметок
   - [ ] Инференс GNN для ранжирования узлов
   - [ ] Фильтрация по GNN scores
   - [ ] Формирование промпта для LLM
   - [ ] Генерация ответа через Semantic Kernel

2. **Explainability (XAI)**
   - [ ] Извлечение attention weights из GAT
   - [ ] Нормализация весов
   - [ ] Ранжирование ребер по важности
   - [ ] Генерация explanation JSON
   - [ ] Визуализация reasoning path

3. **API Endpoints**
   - [ ] POST /api/query - основной RAG запрос
   - [ ] GET /api/explanation/{queryId} - детальное объяснение
   - [ ] POST /api/admin/import-fhir - импорт FHIR данных
   - [ ] GET /api/health - health check

4. **Frontend (опционально)**
   - [ ] React/Vue.js приложение
   - [ ] Визуализация графа (D3.js, vis.js)
   - [ ] Подсветка important edges
   - [ ] Интерактивный UI для запросов

5. **LLM Integration**
   - [ ] Azure OpenAI Service клиент
   - [ ] Embeddings API для векторизации
   - [ ] Chat Completion API для RAG
   - [ ] Fallback на OpenAI если Azure недоступен

**Риски**:
- Галлюцинации LLM → Fine-tuning промптов, factuality tests
- Производительность full pipeline → Кэширование, асинхронность

**Критерии завершения**:
- End-to-end query работает
- Explanation показывает reasoning path
- API возвращает корректный JSON ответ

---

### Фаза V: Оптимизация (4-6 недель)
**Цель**: Оптимизация производительности и стабильности

#### Задачи:
1. **Database Optimization**
   - [ ] Анализ медленных запросов (pg_stat_statements)
   - [ ] Оптимизация HNSW индексов (ef_construction, M)
   - [ ] Настройка GIN индексов для pg_trgm
   - [ ] Партиционирование больших таблиц
   - [ ] Connection pooling (Npgsql)

2. **Caching**
   - [ ] Кэширование часто запрашиваемых подграфов (Redis)
   - [ ] Кэширование эмбеддингов
   - [ ] Мемоизация GNN инференса
   - [ ] HTTP response caching

3. **Performance Testing**
   - [ ] Load testing (k6, JMeter)
   - [ ] Stress testing графовых обходов
   - [ ] Concurrency testing
   - [ ] Профилирование .NET (dotnet-trace)

4. **Monitoring & Observability**
   - [ ] Prometheus metrics
   - [ ] Grafana dashboards
   - [ ] Distributed tracing (OpenTelemetry)
   - [ ] Alert rules для критических метрик

5. **Production Readiness**
   - [ ] Kubernetes deployment manifests
   - [ ] Helm charts
   - [ ] Backup & restore процедуры
   - [ ] Disaster recovery plan

**Риски**:
- Производительность графа на >10M узлов → Использовать шардинг

**Критерии завершения**:
- Система выдерживает 100 RPS
- P99 latency < 2 секунд
- Все health checks проходят
- Мониторинг настроен

---

## 10. Тестирование / Testing Strategy

### 10.1 Unit Tests
```csharp
[TestClass]
public class FhirMappingTests
{
    [TestMethod]
    public void MapCondition_ShouldCreateCorrectCypherQuery()
    {
        // Arrange
        var condition = new Condition
        {
            Id = "cond-123",
            Code = new CodeableConcept("http://snomed.info/sct", "J45.9"),
            Subject = new ResourceReference("Patient/pat-456")
        };
        
        // Act
        var cypherQuery = _mapper.MapCondition(condition, tenantId);
        
        // Assert
        Assert.IsTrue(cypherQuery.Contains("MERGE (concept:Concept"));
        Assert.IsTrue(cypherQuery.Contains("code: 'J45.9'"));
    }
}
```

### 10.2 Integration Tests
```csharp
[TestClass]
public class GraphRagIntegrationTests
{
    [TestMethod]
    public async Task ProcessQuery_ShouldReturnExplanation()
    {
        // Arrange - используем Testcontainers для PostgreSQL
        await using var container = new PostgreSqlBuilder()
            .WithImage("graphrag/postgres18-age:latest")
            .Build();
        await container.StartAsync();
        
        // Act
        var response = await _graphRagService.ProcessQuery(new QueryRequest
        {
            Query = "Какие препараты противопоказаны при приеме Warfarin?",
            TenantId = _testTenantId
        });
        
        // Assert
        Assert.IsNotNull(response.Explanation);
        Assert.IsTrue(response.Explanation.ImportantEdges.Count > 0);
    }
}
```

### 10.3 Factuality Tests
```csharp
public class FactualityValidator
{
    // Используем отдельную LLM для проверки факта
    public async Task<bool> ValidateResponse(
        QueryResponse response, 
        GraphSubgraph sourceGraph)
    {
        var validationPrompt = $@"
            Given facts from knowledge graph:
            {SerializeGraph(sourceGraph)}
            
            Generated response:
            {response.Answer}
            
            Does the response contradict any facts? Answer Yes or No.
        ";
        
        var judgement = await _llmJudge.CompleteAsync(validationPrompt);
        return judgement.Contains("No");
    }
}
```

---

## 11. NuGet Packages / Dependencies

### 11.1 Core Packages
```xml
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="9.0.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
```

### 11.2 Database
```xml
<PackageReference Include="Npgsql" Version="9.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />
<PackageReference Include="Dapper" Version="2.1.0" />
```

### 11.3 AI & ML
```xml
<PackageReference Include="Microsoft.SemanticKernel" Version="1.30.0" />
<PackageReference Include="Microsoft.ML.OnnxRuntime" Version="2.0.0" />
<PackageReference Include="Microsoft.ML" Version="4.0.0" />
<PackageReference Include="Azure.AI.OpenAI" Version="2.1.0" />
```

### 11.4 FHIR
```xml
<PackageReference Include="Hl7.Fhir.R4" Version="5.10.0" />
<PackageReference Include="Hl7.Fhir.Serialization" Version="5.10.0" />
```

### 11.5 Testing
```xml
<PackageReference Include="xUnit" Version="2.9.0" />
<PackageReference Include="Moq" Version="4.20.0" />
<PackageReference Include="Testcontainers.PostgreSql" Version="4.0.0" />
<PackageReference Include="FluentAssertions" Version="7.0.0" />
```

---

## 12. Документация / Documentation

### 12.1 Техническая документация
- [ ] API Reference (Swagger/OpenAPI)
- [ ] Database Schema Documentation
- [ ] Graph Model Documentation
- [ ] Deployment Guide
- [ ] Developer Onboarding Guide

### 12.2 User Documentation
- [ ] User Manual
- [ ] Query Examples
- [ ] Interpretation Guide для Explanation
- [ ] Security & Privacy Guide

---

## 13. Метрики успеха / Success Metrics

### 13.1 Функциональные метрики
- Accuracy: >90% для известных медицинских запросов
- Factuality: <5% галлюцинаций
- Explainability: >80% пользователей понимают reasoning path

### 13.2 Производительные метрики
- Query Latency: P95 < 2s, P99 < 5s
- Throughput: >100 RPS
- Graph Traversal: <500ms для подграфов до 5000 узлов
- GNN Inference: <300ms

### 13.3 Качественные метрики
- Code Coverage: >80%
- Security Vulnerabilities: 0 critical
- Uptime: >99.9%

---

## 14. Открытые вопросы / Open Questions

### 14.1 К заказчику
1. Какой объем данных ожидается? (количество пациентов, документов)
2. Какие конкретные use cases приоритетны? (drug interaction, diagnosis support, etc.)
3. Есть ли размеченные данные для обучения GNN?
4. Какие требования по compliance? (HIPAA, GDPR, etc.)
5. Какой LLM провайдер предпочтителен? (Azure OpenAI, OpenAI, локальные модели)

### 14.2 Технические риски
1. **PostgreSQL 18**: Версия еще не вышла (релиз сентябрь 2025). Как поступить?
   - Использовать PostgreSQL 17 с ограничениями
   - Использовать PostgreSQL 18 beta/RC
   
2. **Apache AGE**: Может не поддерживать PG18 сразу после релиза
   - Подготовить патчи заранее
   - Рассмотреть альтернативы (AGE fork, другие графовые БД)

3. **GNN модель**: Нужны данные для обучения
   - Использовать синтетические данные
   - Transfer learning с публичных медицинских графов

---

## 15. Следующие шаги / Next Steps

1. **Немедленно**:
   - [ ] Согласовать план с заказчиком
   - [ ] Получить ответы на открытые вопросы
   - [ ] Определить состав команды

2. **Неделя 1-2**:
   - [ ] Начать Фазу I: Инфраструктура
   - [ ] Настроить Docker окружение
   - [ ] Создать базовую структуру .NET solution

3. **Месяц 1**:
   - [ ] Завершить Фазу I
   - [ ] Начать Фазу II: Backend Core
   - [ ] Провести технический ревью архитектуры

---

## Заключение

Данный план разработки предоставляет детальную дорожную карту для реализации системы GraphRAG с объяснимым ИИ на .NET. Проект является технически сложным и инновационным, объединяя передовые технологии (PostgreSQL 18, Apache AGE, GNN, ONNX) в единую систему для критически важной области здравоохранения.

Успешная реализация требует:
- Высокой квалификации в системном программировании
- Понимания ML/DL пайплайнов
- Экспертизы в медицинских данных и стандартах (FHIR)
- Строгого соблюдения требований безопасности

Проект рассчитан на **24-32 недели** (6-8 месяцев) при команде из 3-5 разработчиков.

---

**Версия**: 1.0  
**Дата**: 04.02.2026  
**Автор**: Development Team  
**Статус**: Draft - требуется согласование с заказчиком
