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

- **[План разработки](docs/DEVELOPMENT_PLAN.md)** - Подробный план реализации (фазы I-V, 6-8 месяцев)
- **[Структура проекта](docs/PROJECT_STRUCTURE.md)** - Описание архитектуры и организации кода
- **[Этап 1: Анализ требований](docs/stages/stage-01-analysis.md)** - Анализ технических требований
- **[Этап 2: Архитектура](docs/stages/stage-02-architecture.md)** - Проектирование архитектуры системы
- **[Техническое задание](Техническое%20Задание-%20GraphRAG%20на%20.NET%20(1).pdf)** - Исходное ТЗ проекта

## 🎯 Текущий статус

### ✅ Завершено
- [x] Анализ технического задания
- [x] Создание подробного плана разработки
- [x] Создание структуры .NET solution
- [x] Настройка базовых проектов (Domain, Application, Infrastructure, Api, Tests)
- [x] Настройка зависимостей между проектами
- [x] Документация архитектуры и структуры

### 🚧 В процессе
- [ ] **Фаза I: Инфраструктура** (4-6 недель)
  - [ ] PostgreSQL 18 + Apache AGE + pgvector setup
  - [ ] Базовая схема БД
  - [ ] Docker образы
  - [ ] CI/CD pipeline

### 📋 Запланировано
- [ ] **Фаза II: Backend Core** (6-8 недель)
- [ ] **Фаза III: ML & GNN** (4-6 недель)
- [ ] **Фаза IV: GraphRAG & XAI** (6-8 недель)
- [ ] **Фаза V: Оптимизация** (4-6 недель)

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

**Версия**: 0.1.0-alpha  
**Дата**: Февраль 2026  
**Статус**: 🚧 В разработке (Development Phase I)