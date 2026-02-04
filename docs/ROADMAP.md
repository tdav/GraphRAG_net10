# GraphRAG Implementation Roadmap
## Дорожная карта реализации проекта

### Версия: 1.0
### Дата: 04.02.2026

---

## 📊 Общий обзор проекта

**Название**: GraphRAG система с XAI для здравоохранения  
**Длительность**: 24-32 недели (6-8 месяцев)  
**Команда**: 3-5 разработчиков  
**Статус**: 🟢 Планирование завершено, начало Phase I

---

## 🎯 Ключевые этапы разработки

### Phase I: Infrastructure Setup (Недели 1-6)
**Цель**: Развернуть базовую инфраструктуру

#### Подзадачи:
1. **PostgreSQL 18 Environment** [Неделя 1-2]
   - [ ] Установка PostgreSQL 18 (или beta/RC)
   - [ ] Компиляция Apache AGE 1.7+ из исходников
   - [ ] Установка pgvector 0.8+
   - [ ] Тестирование совместимости расширений
   - [ ] Создание Docker образа: `graphrag/postgres18-age:1.0`

2. **Database Schema** [Неделя 2-3]
   - [ ] Создание базовых таблиц (tenants, users, documents, embeddings)
   - [ ] Настройка Apache AGE графовой схемы
   - [ ] Реализация функции `hybrid_search_context()`
   - [ ] Создание индексов (HNSW для векторов, GIN для текста)
   - [ ] Настройка Row Level Security (RLS) политик

3. **.NET Solution Setup** [Неделя 3-4]
   - [x] Создание solution структуры
   - [ ] Настройка NuGet пакетов:
     - Npgsql 9.0+
     - Microsoft.SemanticKernel 1.30+
     - Microsoft.ML.OnnxRuntime 2.0+
     - Hl7.Fhir.R4 5.10+
   - [ ] Базовая конфигурация DI
   - [ ] Настройка Serilog для логирования

4. **DevOps** [Неделя 4-6]
   - [ ] Docker Compose для локальной разработки
   - [ ] GitHub Actions CI/CD pipeline
   - [ ] Health checks для БД и API
   - [ ] Базовые интеграционные тесты с Testcontainers

**Критерии завершения Phase I:**
- ✅ Docker контейнер с PG18 + AGE + pgvector запускается
- ✅ Можно выполнить простой Cypher запрос
- ✅ Можно выполнить векторный поиск (KNN)
- ✅ .NET solution собирается без ошибок
- ✅ Базовые тесты проходят

---

### Phase II: Backend Core (Недели 7-14)
**Цель**: Реализовать основной backend и интеграции

#### Подзадачи:
1. **Domain Layer** [Неделя 7-8]
   - [ ] Entities: Patient, Condition, MedicationRequest, Observation, Concept
   - [ ] Value Objects: FhirResourceId, ConceptCode, EmbeddingVector
   - [ ] Repository Interfaces: IDocumentRepository, IGraphRepository, etc.
   - [ ] Domain Services (если нужны)

2. **Infrastructure - Database** [Неделя 8-10]
   - [ ] PostgresDbContext (EF Core)
   - [ ] ApacheAgeClient для Cypher запросов
   - [ ] PgVectorClient для векторного поиска
   - [ ] Repository implementations
   - [ ] Миграции Entity Framework

3. **Infrastructure - External Services** [Неделя 10-11]
   - [ ] AzureOpenAIService (embeddings + chat completion)
   - [ ] SemanticKernelService основная настройка
   - [ ] FhirMappingService (FHIR Bundle → Graph)
   - [ ] EntityExtractionService (NER через ML.NET или ONNX)

4. **Application Layer** [Неделя 11-12]
   - [ ] GraphRagService - основной оркестратор
   - [ ] HybridSearchService
   - [ ] DTOs: QueryRequest, QueryResponse, GraphContext
   - [ ] Validation логика

5. **Semantic Kernel Plugins** [Неделя 12-13]
   - [ ] GraphQueryPlugin (ExecuteCypher)
   - [ ] VectorMemoryPlugin (SearchNotes)
   - [ ] TerminologyPlugin (NormalizeEntity)
   - [ ] Planner configuration

6. **FHIR ETL Pipeline** [Неделя 13-14]
   - [ ] Парсинг FHIR Bundle (JSON)
   - [ ] Маппинг правила: Patient → Node, Condition → Node + Edge
   - [ ] Обработка Reference связей
   - [ ] Терминология: SNOMED CT, LOINC, RxNorm коды
   - [ ] Batch операции для производительности

**Критерии завершения Phase II:**
- ✅ FHIR Bundle успешно импортируется в граф
- ✅ Можно выполнить гибридный поиск
- ✅ Semantic Kernel planner выбирает корректные плагины
- ✅ Entity extraction работает на медицинских текстах
- ✅ Integration тесты с реальной БД проходят

---

### Phase III: Machine Learning & GNN (Недели 15-20)
**Цель**: Интегрировать GNN модель для ранжирования

#### Подзадачи:
1. **Data Preparation (Python)** [Неделя 15-16]
   - [ ] Экспорт графа из PostgreSQL в формат PyG
   - [ ] Генерация node features (embeddings имен узлов)
   - [ ] Создание training dataset
   - [ ] Разметка релевантности ребер (если данные доступны)

2. **GNN Training (Python)** [Неделя 16-18]
   - [ ] Выбор архитектуры: GAT (Graph Attention Network)
   - [ ] Реализация модели в PyTorch Geometric
   - [ ] Обучение на медицинских графах (синтетических или реальных)
   - [ ] Валидация на тестовой выборке
   - [ ] Экспорт в ONNX с dynamic axes
   - [ ] Верификация ONNX модели

3. **ONNX Integration (.NET)** [Неделя 18-19]
   - [ ] GnnInferenceService
   - [ ] Преобразование AGE JSON → ONNX Tensors
   - [ ] Переиндексация node IDs (global → local [0, N-1])
   - [ ] Обработка dynamic batch sizes
   - [ ] Unit тесты для tensor preparation

4. **Testing & Optimization** [Неделя 19-20]
   - [ ] Performance benchmarks
   - [ ] Валидация выходов модели
   - [ ] Оптимизация памяти (использование Span<T>)
   - [ ] Целевая метрика: <500ms для подграфов до 1000 узлов

**Критерии завершения Phase III:**
- ✅ GNN модель экспортирована в ONNX
- ✅ Инференс работает в .NET
- ✅ Латентность инференса приемлема (<500ms)
- ✅ Node scores корректны и используются для фильтрации

---

### Phase IV: GraphRAG Pipeline & XAI (Недели 21-28)
**Цель**: Полная интеграция RAG с объяснимостью

#### Подзадачи:
1. **GraphRAG Workflow** [Неделя 21-23]
   - [ ] ProcessQuery метод - полная реализация
   - [ ] NER: извлечение медицинских сущностей
   - [ ] Генерация подграфа (Cypher queries)
   - [ ] Векторный поиск релевантных заметок
   - [ ] GNN инференс и фильтрация узлов
   - [ ] Построение промпта для LLM
   - [ ] Генерация ответа через Semantic Kernel

2. **Explainability (XAI)** [Неделя 23-24]
   - [ ] ExplainabilityService
   - [ ] Извлечение attention weights из GAT
   - [ ] Нормализация весов
   - [ ] Ранжирование ребер по важности
   - [ ] Генерация explanation JSON
   - [ ] GraphVisualization DTO

3. **API Endpoints** [Неделя 24-25]
   - [ ] POST /api/query - основной RAG endpoint
   - [ ] GET /api/explanation/{queryId} - детальное объяснение
   - [ ] POST /api/admin/import-fhir - импорт данных
   - [ ] GET /api/health - health check
   - [ ] Swagger/OpenAPI документация

4. **Security & RLS** [Неделя 25-26]
   - [ ] TenantIsolationMiddleware
   - [ ] Автоматическая инъекция tenant_id в Cypher
   - [ ] AuditLogger для pgaudit
   - [ ] SSL/TLS конфигурация
   - [ ] HIPAA compliance проверки

5. **Frontend (Optional)** [Неделя 26-28]
   - [ ] React/Vue.js SPA
   - [ ] D3.js/vis.js для визуализации графа
   - [ ] Подсветка important edges (красным)
   - [ ] Интерактивный query interface
   - [ ] Responsive design

**Критерии завершения Phase IV:**
- ✅ End-to-end query работает
- ✅ Explanation показывает reasoning path
- ✅ API возвращает корректный JSON
- ✅ Frontend (если есть) визуализирует граф
- ✅ Security проверки проходят

---

### Phase V: Optimization & Production (Недели 29-32)
**Цель**: Оптимизация и подготовка к production

#### Подзадачи:
1. **Database Optimization** [Неделя 29]
   - [ ] Анализ медленных запросов (pg_stat_statements)
   - [ ] Оптимизация HNSW индексов (параметры ef_construction, M)
   - [ ] Настройка GIN индексов для pg_trgm
   - [ ] Партиционирование больших таблиц (если >10M строк)
   - [ ] Connection pooling tuning

2. **Caching** [Неделя 29-30]
   - [ ] Redis для кэширования подграфов
   - [ ] Кэширование embeddings
   - [ ] Мемоизация GNN инференса
   - [ ] HTTP response caching (OutputCache)

3. **Performance Testing** [Неделя 30]
   - [ ] Load testing (k6 или JMeter): 100+ RPS
   - [ ] Stress testing графовых обходов
   - [ ] Concurrency testing
   - [ ] Профилирование .NET (dotnet-trace, dotnet-counters)

4. **Monitoring & Observability** [Неделя 31]
   - [ ] Prometheus metrics экспорт
   - [ ] Grafana dashboards
   - [ ] OpenTelemetry distributed tracing
   - [ ] Alert rules для критических метрик
   - [ ] Log aggregation (ELK или Loki)

5. **Production Readiness** [Неделя 31-32]
   - [ ] Kubernetes deployment manifests
   - [ ] Helm charts
   - [ ] ConfigMaps и Secrets
   - [ ] Horizontal Pod Autoscaling (HPA)
   - [ ] Backup & restore процедуры
   - [ ] Disaster recovery plan
   - [ ] Runbook документация

**Критерии завершения Phase V:**
- ✅ Система выдерживает 100+ RPS
- ✅ P99 latency < 2 секунд
- ✅ Все health checks проходят
- ✅ Мониторинг и алерты настроены
- ✅ Production deployment работает

---

## 📦 Deliverables по фазам

### Phase I Deliverables:
- Docker образ: `graphrag/postgres18-age:1.0`
- Docker Compose файл для локальной разработки
- База данных с схемой и индексами
- .NET solution с базовой структурой
- CI/CD pipeline (GitHub Actions)

### Phase II Deliverables:
- Полностью реализованный backend
- FHIR ETL pipeline
- Semantic Kernel плагины
- Repository implementations
- Integration tests (>70% coverage)

### Phase III Deliverables:
- Обученная GNN модель (medical_gat.onnx)
- ONNX Runtime integration
- GnnInferenceService
- Performance benchmarks отчет

### Phase IV Deliverables:
- Полный GraphRAG pipeline
- XAI объяснения
- REST API
- API документация (Swagger)
- Frontend (опционально)

### Phase V Deliverables:
- Production-ready deployment
- Kubernetes manifests
- Monitoring dashboards
- Performance testing отчет
- Runbook и документация

---

## 🎓 Требования к команде

### Роли:
1. **Backend Developer (.NET)** x2
   - C# expert, .NET 10
   - Entity Framework Core
   - PostgreSQL, SQL оптимизация
   - Semantic Kernel

2. **ML Engineer (Python/C#)** x1
   - PyTorch Geometric
   - ONNX export/import
   - GNN architectures
   - C# для ONNX Runtime integration

3. **DevOps Engineer** x1
   - Docker, Kubernetes
   - PostgreSQL administration
   - CI/CD (GitHub Actions)
   - Monitoring (Prometheus, Grafana)

4. **Domain Expert (Healthcare)** x1 (консультант)
   - FHIR стандарты
   - Медицинские терминологии (SNOMED CT, LOINC)
   - Use cases валидация

---

## 🔍 Риски и митигация

### Высокие риски:

#### 1. PostgreSQL 18 доступность
**Риск**: PG18 еще не вышел (релиз сентябрь 2025)  
**Митигация**: 
- Использовать PG 17 для начальной разработки
- Следить за beta/RC релизами PG18
- Подготовить план миграции

#### 2. Apache AGE совместимость с PG18
**Риск**: AGE может не поддерживать PG18 сразу после релиза  
**Митигация**:
- Подготовить патчи заранее
- Связаться с AGE community
- Рассмотреть альтернативы (Neo4j, другие графовые БД)

#### 3. GNN модель качество
**Риск**: Недостаток размеченных данных для обучения  
**Митигация**:
- Использовать синтетические данные
- Transfer learning с публичных медицинских графов
- Unsupervised/self-supervised обучение

#### 4. LLM галлюцинации
**Риск**: LLM может генерировать неправильные ответы  
**Митигация**:
- Factuality tests с LLM-judge
- Строгий контекст из графа
- Fine-tuning промптов
- Confidence thresholds

### Средние риски:

#### 5. Производительность графовых обходов
**Риск**: Медленные запросы на больших графах (>10M узлов)  
**Митигация**:
- Оптимизация индексов
- Ограничение глубины обхода (max 3 hops)
- Кэширование популярных подграфов
- Партиционирование данных

---

## 📈 Метрики успеха

### Функциональные метрики:
- **Accuracy**: >90% для тестовых медицинских запросов
- **Factuality**: <5% галлюцинаций (через LLM-judge)
- **Explainability**: >80% пользователей понимают reasoning path

### Производительные метрики:
- **Query Latency**: P95 < 2s, P99 < 5s
- **Throughput**: >100 RPS
- **Graph Traversal**: <500ms для подграфов до 5000 узлов
- **GNN Inference**: <300ms

### Качественные метрики:
- **Code Coverage**: >80% (unit + integration)
- **Security Vulnerabilities**: 0 critical, 0 high
- **Uptime**: >99.9%
- **FHIR Compliance**: 100% для поддерживаемых ресурсов

---

## 📅 Milestones

| Milestone | Дата | Описание |
|-----------|------|----------|
| M1: Infrastructure Ready | Конец недели 6 | PostgreSQL + .NET solution готовы |
| M2: Backend Core Complete | Конец недели 14 | FHIR integration работает |
| M3: GNN Integration | Конец недели 20 | ML компонент интегрирован |
| M4: GraphRAG Pipeline | Конец недели 28 | Полный RAG работает |
| M5: Production Ready | Конец недели 32 | Готов к деплою |

---

## 🔗 Ссылки на документацию

- [Подробный план разработки](DEVELOPMENT_PLAN.md)
- [Структура проекта](PROJECT_STRUCTURE.md)
- [Техническое задание](../Техническое%20Задание-%20GraphRAG%20на%20.NET%20(1).pdf)
- [Этап 1: Анализ](stages/stage-01-analysis.md)
- [Этап 2: Архитектура](stages/stage-02-architecture.md)

---

## 🎯 Текущий статус

**Фаза**: Планирование → Phase I  
**Прогресс**: █░░░░░░░░░ 10%  
**Следующий milestone**: M1 - Infrastructure Ready (через 6 недель)

**Последнее обновление**: 04.02.2026  
**Версия roadmap**: 1.0
