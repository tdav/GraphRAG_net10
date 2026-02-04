# Quick Start Guide
## Быстрое начало работы с GraphRAG .NET

### Версия: 1.0
### Дата: 04.02.2026

---

## 🎯 Для кого это руководство

Это руководство предназначено для разработчиков, которые хотят:
- Быстро начать работу с проектом GraphRAG
- Понять базовую архитектуру
- Настроить локальное окружение
- Внести первый вклад

---

## 📋 Предварительные требования

### Обязательно:
- **.NET 10 SDK** - [Скачать](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Git** - [Скачать](https://git-scm.com/downloads)
- **IDE**: Visual Studio 2025, Rider 2025, или VS Code с C# extension
- **Docker Desktop** - [Скачать](https://www.docker.com/products/docker-desktop)

### Рекомендуется:
- **PostgreSQL Client (psql)** - для работы с БД
- **Postman или Insomnia** - для тестирования API
- **Python 3.10+** - для ML части (Phase III)

---

## 🚀 Быстрый старт (5 минут)

### 1. Клонирование репозитория
```bash
git clone https://github.com/tdav/GraphRAG_net10.git
cd GraphRAG_net10
```

### 2. Проверка .NET SDK
```bash
dotnet --version
# Ожидаемый вывод: 10.0.x
```

### 3. Восстановление зависимостей
```bash
dotnet restore
```

### 4. Сборка проекта
```bash
dotnet build
```

### 5. Запуск тестов
```bash
dotnet test
```

**Готово!** Если все команды выполнились без ошибок, вы готовы к разработке.

---

## 🐳 Запуск с Docker (Phase I)

> **Примечание**: Docker окружение будет доступно после завершения Phase I (недели 1-6)

### Запуск PostgreSQL с расширениями
```bash
# Запустить PostgreSQL + Apache AGE + pgvector
docker-compose up -d postgres

# Проверить статус
docker-compose ps

# Проверить логи
docker-compose logs -f postgres
```

### Применение миграций БД
```bash
dotnet ef database update \
    --project src/GraphRAG.Infrastructure \
    --startup-project src/GraphRAG.Api
```

### Запуск API
```bash
dotnet run --project src/GraphRAG.Api/GraphRAG.Api.csproj
```

API будет доступен по адресу: `https://localhost:7001`  
Swagger UI: `https://localhost:7001/swagger`

---

## 📁 Структура проекта (краткая)

```
GraphRAG_net10/
├── src/
│   ├── GraphRAG.Domain/          # Сущности и интерфейсы
│   ├── GraphRAG.Application/     # Бизнес-логика
│   ├── GraphRAG.Infrastructure/  # Реализации (БД, API)
│   └── GraphRAG.Api/             # REST API
├── tests/
│   └── GraphRAG.Tests/           # Тесты
└── docs/
    ├── DEVELOPMENT_PLAN.md       # Детальный план (34KB)
    ├── ROADMAP.md                # Дорожная карта (12KB)
    └── PROJECT_STRUCTURE.md      # Архитектура (12KB)
```

---

## 💻 Основные команды

### Сборка и запуск
```bash
# Сборка всего solution
dotnet build

# Сборка конкретного проекта
dotnet build src/GraphRAG.Api/GraphRAG.Api.csproj

# Запуск API
dotnet run --project src/GraphRAG.Api/GraphRAG.Api.csproj

# Запуск с hot reload (watch mode)
dotnet watch --project src/GraphRAG.Api/GraphRAG.Api.csproj
```

### Тестирование
```bash
# Запуск всех тестов
dotnet test

# Запуск конкретного проекта тестов
dotnet test tests/GraphRAG.Tests/GraphRAG.Tests.csproj

# Запуск с покрытием кода
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=opencover

# Запуск конкретного теста
dotnet test --filter "FullyQualifiedName~MyTestClass.MyTestMethod"
```

### NuGet пакеты
```bash
# Добавить пакет
dotnet add src/GraphRAG.Api package Microsoft.SemanticKernel

# Обновить пакет
dotnet add src/GraphRAG.Api package Microsoft.SemanticKernel --version 1.30.0

# Удалить пакет
dotnet remove src/GraphRAG.Api package PackageName

# Список пакетов
dotnet list package
```

### Entity Framework миграции
```bash
# Создать миграцию
dotnet ef migrations add InitialCreate \
    --project src/GraphRAG.Infrastructure \
    --startup-project src/GraphRAG.Api

# Применить миграции
dotnet ef database update \
    --project src/GraphRAG.Infrastructure \
    --startup-project src/GraphRAG.Api

# Откатить миграцию
dotnet ef database update PreviousMigrationName \
    --project src/GraphRAG.Infrastructure \
    --startup-project src/GraphRAG.Api

# Удалить последнюю миграцию
dotnet ef migrations remove \
    --project src/GraphRAG.Infrastructure \
    --startup-project src/GraphRAG.Api
```

---

## 🔧 Конфигурация

### appsettings.Development.json
Создайте файл `src/GraphRAG.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=graphrag_dev;Username=postgres;Password=your_password"
  },
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "ApiKey": "your-api-key",
    "DeploymentName": "gpt-4",
    "EmbeddingDeploymentName": "text-embedding-ada-002"
  },
  "GNN": {
    "ModelPath": "./models/medical_gat.onnx",
    "ScoreThreshold": 0.5
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

**Важно**: Добавьте `appsettings.Development.json` в `.gitignore` если он содержит секреты!

---

## 🧪 Примеры использования

### Пример 1: Простой RAG запрос (Phase IV)
```bash
curl -X POST https://localhost:7001/api/query \
  -H "Content-Type: application/json" \
  -d '{
    "query": "Какие препараты противопоказаны при приеме Warfarin?",
    "tenantId": "00000000-0000-0000-0000-000000000001"
  }'
```

Ожидаемый ответ:
```json
{
  "answer": "При приеме Warfarin противопоказаны следующие препараты...",
  "confidence": 0.89,
  "explanation": {
    "reasoning_path": [
      {
        "node_id": "node_123",
        "label": "Warfarin",
        "type": "MedicationRequest"
      },
      {
        "edge_id": "edge_456",
        "label": "CONTRAINDICATES",
        "weight": 0.92
      }
    ]
  }
}
```

### Пример 2: Импорт FHIR данных (Phase II)
```bash
curl -X POST https://localhost:7001/api/admin/import-fhir \
  -H "Content-Type: application/json" \
  -d @sample_fhir_bundle.json
```

---

## 🐛 Отладка

### Visual Studio
1. Открыть `GraphRAG.slnx`
2. Установить breakpoint в коде
3. F5 для запуска отладки

### VS Code
1. Открыть папку проекта
2. Создать `.vscode/launch.json`:
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (web)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/src/GraphRAG.Api/bin/Debug/net10.0/GraphRAG.Api.dll",
      "args": [],
      "cwd": "${workspaceFolder}/src/GraphRAG.Api",
      "stopAtEntry": false,
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
      },
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  ]
}
```
3. F5 для запуска отладки

### Логирование
Логи доступны в консоли при запуске приложения:
```bash
dotnet run --project src/GraphRAG.Api/GraphRAG.Api.csproj
```

Для более детальных логов установите уровень в `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Trace"
    }
  }
}
```

---

## 📚 Полезные ссылки

### Внутренняя документация:
- [Детальный план разработки](DEVELOPMENT_PLAN.md) - 34KB, все детали
- [Дорожная карта](ROADMAP.md) - timeline и milestones
- [Структура проекта](PROJECT_STRUCTURE.md) - архитектура в деталях
- [Техническое задание](../Техническое%20Задание-%20GraphRAG%20на%20.NET%20(1).pdf) - исходное ТЗ (PDF)

### Внешние ресурсы:
- [Microsoft Semantic Kernel Docs](https://learn.microsoft.com/en-us/semantic-kernel/)
- [Apache AGE Documentation](https://age.apache.org/)
- [pgvector GitHub](https://github.com/pgvector/pgvector)
- [ONNX Runtime Docs](https://onnxruntime.ai/docs/)
- [FHIR Specification](https://hl7.org/fhir/)
- [PyTorch Geometric](https://pytorch-geometric.readthedocs.io/)

---

## 🤝 Вклад в проект

### Workflow для контрибьюторов:
1. Fork репозитория
2. Создать feature branch: `git checkout -b feature/my-feature`
3. Внести изменения
4. Запустить тесты: `dotnet test`
5. Commit: `git commit -m "Add my feature"`
6. Push: `git push origin feature/my-feature`
7. Создать Pull Request

### Стандарты кода:
- Следовать C# Coding Conventions
- Писать XML комментарии для публичных API
- Покрытие тестами >80%
- Использовать async/await для I/O операций

### Code Review Process:
1. Создать PR с описанием изменений
2. Дождаться прохождения CI/CD checks
3. Получить approval от maintainer
4. Merge в main branch

---

## ❓ Часто задаваемые вопросы (FAQ)

### Q: Какая версия PostgreSQL поддерживается?
**A**: Целевая версия - PostgreSQL 18 (релиз сентябрь 2025). Для начальной разработки можно использовать PostgreSQL 17.

### Q: Можно ли использовать OpenAI вместо Azure OpenAI?
**A**: Да, система поддерживает оба варианта. Просто измените конфигурацию в `appsettings.json`.

### Q: Нужно ли обучать GNN модель самостоятельно?
**A**: В Phase III будет предоставлена предобученная модель. Для кастомизации можно обучить свою модель.

### Q: Поддерживаются ли другие языки кроме русского?
**A**: Да, система поддерживает любые языки через LLM. NER модель может потребовать адаптации.

### Q: Где хранятся медицинские данные?
**A**: Все данные хранятся в PostgreSQL. Данные разных тенантов изолированы через Row Level Security (RLS).

---

## 🆘 Получить помощь

### Нашли баг?
Создайте issue на GitHub: [Create Issue](https://github.com/tdav/GraphRAG_net10/issues/new)

### Есть вопросы?
- Проверьте [FAQ](#-часто-задаваемые-вопросы-faq)
- Посмотрите существующие [Issues](https://github.com/tdav/GraphRAG_net10/issues)
- Создайте новый [Discussion](https://github.com/tdav/GraphRAG_net10/discussions)

### Нужна консультация?
Свяжитесь с командой через GitHub Issues с тегом `question`.

---

## 📊 Статус проекта

**Текущая фаза**: Планирование → Phase I  
**Прогресс**: 10%  
**Версия**: 0.1.0-alpha  
**Последнее обновление**: 04.02.2026

---

## ✅ Чеклист для новых разработчиков

Перед началом работы убедитесь, что:

- [ ] .NET 10 SDK установлен (`dotnet --version`)
- [ ] Git настроен (`git config --global user.name "Your Name"`)
- [ ] Репозиторий склонирован
- [ ] `dotnet build` проходит успешно
- [ ] `dotnet test` проходит успешно
- [ ] Прочитан [DEVELOPMENT_PLAN.md](DEVELOPMENT_PLAN.md)
- [ ] Понята структура проекта из [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md)
- [ ] Создан `appsettings.Development.json` с вашими настройками
- [ ] Docker Desktop установлен и запущен (для Phase I+)

**Готовы к работе! 🚀**

---

**Версия документа**: 1.0  
**Поддерживается**: До начала Phase II (неделя 7)  
**Следующее обновление**: После завершения Phase I с Docker setup
