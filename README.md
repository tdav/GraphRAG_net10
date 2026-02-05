# GraphRAG на .NET 10

Система GraphRAG (Graph Retrieval-Augmented Generation) с объяснимым искусственным интеллектом (XAI) на базе графовых нейронных сетей (GNN) для здравоохранения.

## 🏗️ Архитектура
Проект следует принципам **Clean Architecture** и использует возможности **.NET 10** и **C# 13**.

## 🎯 Текущий статус
**Общий прогресс проекта**: ~45% (14 из 32 недель)  
**Текущая фаза**: 🚧 Phase III - ML & GNN Integration (запланировано)  
**Подробный статус**: [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)

### ✅ Phase I - Инфраструктура (100%)
Базовая структура, PostgreSQL с расширениями AGE/pgvector, CI/CD.

### ✅ Phase II - Backend Core (100%)
- [x] **AI Orchestration**: Semantic Kernel + Stepwise Planner.
- [x] **Plugins**: GraphQuery, VectorMemory, MedicalTerminology.
- [x] **FHIR ETL**: Полная автоматизация импорта FHIR R4.
- [x] **Use Cases**: Реализована логика запросов и управления данными.
- [x] **Tests**: 34+ успешно пройденных теста.

---
**Последнее обновление**: 06.02.2026