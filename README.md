# GraphRAG на .NET 10

Система GraphRAG (Graph Retrieval-Augmented Generation) с объяснимым искусственным интеллектом (XAI) на базе графовых нейронных сетей (GNN) для здравоохранения.

## 📋 Обзор проекта

Проект реализует продвинутую систему поддержки принятия врачебных решений (CDSS), которая объединяет:
- **Graph Knowledge Base** - графовая база знаний на Apache AGE
- **Vector Search** - векторный поиск через pgvector
- **GNN (Graph Neural Networks)** - графовые нейронные сети для ранжирования
- **Explainable AI** - визуализация путей рассуждения модели
- **FHIR Integration** - интеграция со стандартом HL7 FHIR

## 🏗️ Архитектура

Проект следует принципам **Clean Architecture** с четким разделением слоев:

```
┌─────────────────────────────────────┐
│         GraphRAG.Api (REST API)     │
├─────────────────────────────────────┤
│     GraphRAG.Infrastructure         │
│  (PostgreSQL, AGE, ONNX, SK)        │
├─────────────────────────────────────┤
│     GraphRAG.Application            │
│  (Services, Use Cases, Plugins)     │
├─────────────────────────────────────┤
│        GraphRAG.Domain              │
│  (Entities, Interfaces, Rules)      │
└─────────────────────────────────────┘
```

## 🔧 Технологический стек

- **Backend**: .NET 10 (C#)
- **AI Framework**: Microsoft Semantic Kernel
- **Database**: PostgreSQL 18
- **Graph Engine**: Apache AGE (openCypher)
- **Vector Search**: pgvector (HNSW indices)
- **ML Inference**: ONNX Runtime
- **GNN**: GraphSAGE/GAT (PyTorch → ONNX)
- **FHIR**: Hl7.Fhir.R4

## 📁 Структура проекта

```
GraphRAG_net10/
├── src/
│   ├── GraphRAG.Domain/          # Доменные сущности и интерфейсы
│   ├── GraphRAG.Application/     # Бизнес-логика и use cases
│   ├── GraphRAG.Infrastructure/  # Реализации репозиториев и клиентов
│   └── GraphRAG.Api/             # REST API endpoints
├── tests/
│   └── GraphRAG.Tests/           # Unit, Integration, Performance tests
├── docs/
│   ├── DEVELOPMENT_PLAN.md       # Детальный план разработки
│   ├── PROJECT_STRUCTURE.md      # Описание структуры проекта
│   ├── stages/                   # Этапы разработки
│   └── quests/                   # Задачи для разработки
└── Техническое Задание- GraphRAG на .NET (1).pdf
```

## 🚀 Быстрый старт

### Требования
- .NET 10 SDK
- PostgreSQL 18 с расширениями Apache AGE и pgvector
- Docker (опционально, для локальной разработки)

### Сборка проекта
```bash
# Клонировать репозиторий
git clone https://github.com/tdav/GraphRAG_net10.git
cd GraphRAG_net10

# Восстановить зависимости
dotnet restore

# Собрать проект
dotnet build

# Запустить тесты
dotnet test

# Запустить API
dotnet run --project src/GraphRAG.Api/GraphRAG.Api.csproj
```

### Docker Compose (в разработке)
```bash
# Запустить PostgreSQL с AGE и pgvector
docker-compose up -d

# Применить миграции БД
dotnet ef database update --project src/GraphRAG.Infrastructure --startup-project src/GraphRAG.Api
```

## 📚 Документация

- **[🎯 Статус реализации](IMPLEMENTATION_STATUS.md)** - **НОВОЕ**: Детальный отчет о выполненной работе и план оставшихся задач
- **[📋 Phase II: Детальный план](docs/PHASE_II_DETAILED_PLAN.md)** - **НОВОЕ**: Полный план Phase 2 (8 недель, 6 этапов)
- **[План разработки](docs/DEVELOPMENT_PLAN.md)** - Подробный план реализации (фазы I-V, 6-8 месяцев)
- **[Структура проекта](docs/PROJECT_STRUCTURE.md)** - Описание архитектуры и организации кода
- **[Дорожная карта](docs/ROADMAP.md)** - Временная шкала и этапы разработки
- **[Этап 1: Анализ требований](docs/stages/stage-01-analysis.md)** - Анализ технических требований
- **[Этап 2: Архитектура](docs/stages/stage-02-architecture.md)** - Проектирование архитектуры системы
- **[Техническое задание](Техническое%20Задание-%20GraphRAG%20на%20.NET%20(1).pdf)** - Исходное ТЗ проекта

## 🎯 Текущий статус

**Общий прогресс проекта**: ~12% (4 из 32 недель)  
**Текущая фаза**: Phase I - Infrastructure Setup (70% завершено)  
**📊 Подробный статус**: См. [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)

### ✅ Завершено (Phase I - 70%)
- [x] **Документация** (100%)
  - [x] Анализ технического задания (stage-01-analysis.md)
  - [x] Проектирование архитектуры (stage-02-architecture.md)
  - [x] План разработки и дорожная карта
  - [x] Отчет о статусе реализации (IMPLEMENTATION_STATUS.md)
  
- [x] **Domain Layer** (100%)
  - [x] 14 доменных сущностей (Core, Medical, Graph, AI)
  - [x] 5 интерфейсов репозиториев
  - [x] Clean Architecture структура
  
- [x] **Database Infrastructure** (100%)
  - [x] PostgreSQL 17 + Apache AGE 1.5.0 + pgvector 0.8.0
  - [x] Полная схема БД (10 таблиц)
  - [x] 15+ индексов (HNSW, GIN, B-tree)
  - [x] Row Level Security политики
  - [x] Docker Compose окружение
  
- [x] **CI/CD Pipeline** (100%)
  - [x] GitHub Actions workflow
  - [x] Автоматизированная сборка и тесты
  - [x] Security scanning (CodeQL)
  
- [x] **.NET Solution** (100%)
  - [x] Структура проектов создана
  - [x] Сборка проходит успешно

### 🚧 В процессе (Phase I - оставшиеся 30%)
- [ ] **Infrastructure Implementation**
  - [ ] NuGet пакеты (Npgsql, EF Core, Semantic Kernel, ONNX)
  - [ ] PostgresDbContext и EF Core миграции
  - [ ] ApacheAgeClient для Cypher запросов
  - [ ] PgVectorClient для векторного поиска
  - [ ] Реализация репозиториев
  
- [ ] **Application & API Setup**
  - [ ] Базовые DTOs и сервисы
  - [ ] Health checks
  - [ ] Dependency injection
  - [ ] Swagger документация
  
- [ ] **Testing Infrastructure**
  - [ ] Unit тесты для domain entities
  - [ ] Integration тесты с Testcontainers

### 📋 Запланировано

#### **Phase II: Backend Core** (Недели 7-14, 0%)

**Цель фазы**: Реализовать основной backend функционал и внешние интеграции

##### Этап 1: Domain Layer Extensions (Недели 7-8)
- [ ] Value Objects для типов предметной области
  - [ ] FhirResourceId - для FHIR ссылок
  - [ ] ConceptCode - для медицинских кодов (SNOMED CT, LOINC, RxNorm)
  - [ ] EmbeddingVector - для векторных представлений
- [ ] Domain Events для асинхронной обработки
  - [ ] PatientImported - событие импорта пациента из FHIR
  - [ ] GraphNodeCreated - событие создания узла
  - [ ] QueryCompleted - событие завершения запроса
- [ ] Domain Services
  - [ ] ValidationService - валидация бизнес-правил
  - [ ] MedicalTerminologyService - работа с медицинскими терминами

##### Этап 2: Infrastructure - Database (Недели 8-10)
- [ ] Repository Implementations
  - [ ] PostgresRepository<T> - базовый репозиторий с EF Core
  - [ ] GraphRepository - выполнение Cypher запросов через Apache AGE
  - [ ] VectorRepository - KNN поиск через pgvector
  - [ ] ConversationRepository - управление историей диалогов
  - [ ] FhirRepository - импорт FHIR Bundle
- [ ] Database Optimizations
  - [ ] Batch операции для импорта данных
  - [ ] Транзакционная поддержка с Unit of Work
  - [ ] Query оптимизация и профилирование
- [ ] EF Core Migrations
  - [ ] InitialCreate миграция
  - [ ] Seed данные для разработки/тестирования

##### Этап 3: Infrastructure - External Services (Недели 10-11)
- [ ] AI Services
  - [ ] AzureOpenAIService - embeddings generation и chat completion
  - [ ] EntityExtractionService - извлечение медицинских сущностей из текста
  - [ ] SemanticKernelConfiguration - настройка SK плагинов и планировщика
- [ ] FHIR Services
  - [ ] FhirMappingService - маппинг FHIR ресурсов в граф
    - [ ] Patient → GraphNode + Patient entity
    - [ ] Condition → GraphNode + GraphEdge (hasCondition)
    - [ ] MedicationRequest → GraphNode + GraphEdge (prescribedMedication)
    - [ ] Observation → GraphNode + GraphEdge (hasObservation)
  - [ ] FhirValidationService - валидация FHIR Bundle
- [ ] Graph Services
  - [ ] GraphTraversalService - обход графа по паттернам
  - [ ] SubgraphExtractionService - извлечение релевантных подграфов

##### Этап 4: Application Layer (Недели 11-12)
- [ ] Core Services
  - [ ] GraphRagService - главный оркестратор RAG pipeline
    - [ ] ProcessQuery(QueryRequest) - обработка запроса пользователя
    - [ ] Entity extraction → Hybrid search → Context assembly → LLM generation
  - [ ] HybridSearchService - комбинированный поиск
    - [ ] VectorSearch - семантический поиск в документах
    - [ ] GraphSearch - поиск связей в графе знаний
    - [ ] ResultsFusion - слияние и ранжирование результатов
- [ ] DTOs и Validation
  - [ ] QueryRequest/QueryResponse
  - [ ] SearchContext
  - [ ] GraphContext
  - [ ] FluentValidation rules
- [ ] Use Cases
  - [ ] ProcessMedicalQueryUseCase
  - [ ] ImportFhirDataUseCase
  - [ ] ExplainReasoningUseCase

##### Этап 5: Semantic Kernel Plugins (Недели 12-13)
- [ ] Graph Plugin
  - [ ] ExecuteCypherQuery - выполнение Cypher запросов
  - [ ] GetSubgraph - получение подграфа вокруг узла
  - [ ] FindPaths - поиск путей между узлами
- [ ] Vector Memory Plugin
  - [ ] SearchDocuments - семантический поиск документов
  - [ ] SearchNotes - поиск клинических заметок
  - [ ] GetSimilarConcepts - поиск похожих концепций
- [ ] Terminology Plugin
  - [ ] NormalizeEntityName - нормализация названий
  - [ ] MapToStandardCode - маппинг на стандартные коды (SNOMED CT)
  - [ ] ExpandAcronyms - раскрытие медицинских аббревиатур
- [ ] Planner Configuration
  - [ ] SequentialPlanner настройка
  - [ ] Prompt templates для медицинского домена

##### Этап 6: FHIR ETL Pipeline (Недели 13-14)
- [ ] FHIR Bundle Processing
  - [ ] JSON parsing и десериализация (Hl7.Fhir.R4)
  - [ ] Валидация структуры Bundle
  - [ ] Обработка batch операций
- [ ] Resource Mapping
  - [ ] Patient resource → Patient entity + GraphNode
  - [ ] Condition resource → Condition entity + GraphNode + Edge
  - [ ] MedicationRequest → MedicationRequest entity + GraphNode + Edge
  - [ ] Observation → Observation entity + GraphNode + Edge
  - [ ] Reference resolution (ссылки между ресурсами)
- [ ] Terminology Processing
  - [ ] SNOMED CT codes для диагнозов
  - [ ] LOINC codes для наблюдений
  - [ ] RxNorm codes для препаратов
  - [ ] Mapping на Concept entities
- [ ] Performance Optimization
  - [ ] Batch insert операции (>1000 records/sec)
  - [ ] Параллельная обработка Bundle entries
  - [ ] Deduplication проверки

##### Критерии завершения Phase II:
- ✅ FHIR Bundle успешно импортируется в граф (>1000 records/sec)
- ✅ Гибридный поиск работает (vector + graph fusion)
- ✅ Semantic Kernel planner корректно выбирает плагины
- ✅ Entity extraction корректно извлекает медицинские сущности (>80% accuracy)
- ✅ Integration тесты с реальной БД проходят (>80% coverage)
- ✅ API endpoints документированы в Swagger
- ✅ Латентность query processing <3s (без GNN)

---

#### **Phase III: ML & GNN** (Недели 15-20, 0%)
- [ ] GNN модель обучение (PyTorch GAT)
- [ ] ONNX экспорт и интеграция
  
#### **Phase IV: GraphRAG & XAI** (Недели 21-28, 0%)
- [ ] Полный RAG pipeline
- [ ] Explainable AI визуализация
  
#### **Phase V: Production Readiness** (Недели 29-32, 0%)
- [ ] Performance оптимизация
- [ ] HIPAA compliance
- [ ] Production deployment

## 🔑 Ключевые особенности

### 1. Гибридный поиск (Hybrid Search)
Комбинация векторного поиска (pgvector HNSW) и графового поиска (Apache AGE Cypher):
```sql
SELECT * FROM hybrid_search_context(
    query_vector := '[0.1, 0.2, ...]',
    entity_list := ARRAY['Препарат X', 'Диагноз Y'],
    tenant_id := 'uuid',
    top_k := 10
);
```

### 2. Графовые нейронные сети (GNN)
Ранжирование узлов и ребер графа с помощью GAT (Graph Attention Network):
```csharp
var nodeScores = await _gnnService.ScoreNodes(subgraph);
var filteredSubgraph = FilterByGnnScore(subgraph, nodeScores, threshold: 0.5);
```

### 3. Объяснимый ИИ (XAI)
Визуализация reasoning path через attention weights:
```json
{
  "response": "Назначение препарата X противопоказано...",
  "explanation": {
    "reasoning_path": [...],
    "important_edges": [...]
  }
}
```

### 4. FHIR Integration
Автоматический маппинг FHIR Resources → Graph:
```csharp
var result = await _fhirMapper.ImportFhirBundle(bundle, tenantId);
// Patient → Node, Condition → Node + Edge, MedicationRequest → Node + Edge
```

### 5. Multi-tenancy & Security
Row Level Security (RLS) для изоляции данных:
```sql
ALTER TABLE documents ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation_policy ON documents
    USING (tenant_id = current_setting('app.current_tenant_id')::UUID);
```

## 🤝 Вклад в проект

Проект находится в активной разработке. Если вы хотите внести вклад:
1. Fork репозитория
2. Создайте feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit изменения (`git commit -m 'Add some AmazingFeature'`)
4. Push в branch (`git push origin feature/AmazingFeature`)
5. Откройте Pull Request

## 📄 Лицензия

Проект распространяется по лицензии MIT. См. файл `LICENSE` для деталей.

## 📧 Контакты

- **GitHub**: [tdav/GraphRAG_net10](https://github.com/tdav/GraphRAG_net10)
- **Issues**: [Создать Issue](https://github.com/tdav/GraphRAG_net10/issues)

## 🙏 Благодарности

Проект основан на современных исследованиях в области GraphRAG, GNN и XAI:
- Microsoft GraphRAG
- PyTorch Geometric
- Apache AGE Project
- PostgreSQL Community

---

## 📊 Сводка по фазам проекта

### Статус фаз
| Фаза | Статус | Прогресс | Описание |
|------|--------|----------|----------|
| **Phase I: Infrastructure** | 🟢 В работе | 70% | Базовая инфраструктура готова |
| **Phase II: Backend Core** | ⬜ Запланировано | 0% | Детальный план создан |
| **Phase III: ML & GNN** | ⬜ Запланировано | 0% | Ожидает Phase II |
| **Phase IV: GraphRAG & XAI** | ⬜ Запланировано | 0% | Ожидает Phase III |
| **Phase V: Production** | ⬜ Запланировано | 0% | Ожидает Phase IV |

### Завершённые компоненты Phase I (✅ 100%)
- ✅ **Документация** - Полная техническая документация (1800+ строк)
- ✅ **Domain Layer** - 14 сущностей + 5 интерфейсов репозиториев
- ✅ **Database Schema** - PostgreSQL 17 + Apache AGE + pgvector + 10 таблиц + 15+ индексов
- ✅ **Docker Environment** - Готовое окружение для разработки
- ✅ **CI/CD Pipeline** - GitHub Actions с автоматическими проверками
- ✅ **.NET Solution** - Структура проектов и сборка

### В работе Phase I (🚧 30% осталось)
- 🚧 Infrastructure Implementation (репозитории, клиенты БД)
- 🚧 Application & API Setup (сервисы, DI, health checks)
- 🚧 Testing Infrastructure (unit + integration тесты)

---

**Версия**: 0.2.0-alpha  
**Дата обновления**: 04 февраля 2026  
**Текущая фаза**: 🟢 Phase I - Infrastructure Setup (70% завершено)  
**Следующая фаза**: Phase II - Backend Core (детальный план готов)  
**Детальный статус**: [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)