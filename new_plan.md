# New Work Plan: GraphRAG on .NET 10 (Explainable AI for Healthcare)

## Executive Summary
This plan outlines the roadmap for completing the GraphRAG system as specified in the "Техническое Задание- GraphRAG на .NET (1).pdf". The project is currently at the end of Phase I, with a solid structural foundation but many functional placeholders. This plan focuses on transitioning from skeletons to real implementations.

---

## Phase 1: Infrastructure Finalization (Immediate Actions)
**Goal**: Move from placeholders to working infrastructure.

1.  **Apache AGE Integration**:
    *   Replace placeholder `ExecuteCypherQueryAsync` logic with actual parameterized queries using `ag_catalog.cypher`.
    *   Implement real `AddNodeAsync` and `AddEdgeAsync` that sync with the `medical_graph`.
    *   Fix the subgraph retrieval logic to use Cypher's `MATCH` and `path` instead of simple EF Core queries.
2.  **PgVector Refinement**:
    *   Ensure `VectorRepository.SearchSimilarAsync` correctly uses the `<=>` operator and HNSW index.
    *   Implement real embedding storage and retrieval logic.
3.  **FHIR Foundation**:
    *   Implement basic `FhirRepository` methods using `Hl7.Fhir.R4` for parsing and basic resource management.
4.  **Verification**:
    *   Run and expand the existing 11 unit tests.
    *   Add integration tests using Testcontainers for PostgreSQL with AGE and pgvector.

---

## Phase 2: Backend Core & AI Integration (Weeks 1-4)
**Goal**: Integrate Semantic Kernel and implement the FHIR ETL pipeline.

1.  **LLM Orchestration**:
    *   Integrate **Microsoft Semantic Kernel**.
    *   Configure Azure OpenAI (or local models via ONNX) for chat completion and embeddings.
    *   Replace placeholder embedding generation with real calls.
2.  **Semantic Kernel Plugins**:
    *   `GraphQueryPlugin`: Allow LLM to query the graph via Cypher.
    *   `VectorMemoryPlugin`: Allow LLM to search clinical notes.
    *   `TerminologyPlugin`: Medical entity normalization (SNOMED, LOINC).
3.  **FHIR ETL Pipeline**:
    *   Implement `FhirMappingService` to map Patient, Condition, Observation, and MedicationRequest to Graph nodes and edges.
    *   Implement `FhirEtlPipeline` for batch processing of FHIR Bundles.
4.  **Medical NER**:
    *   Implement real Entity Extraction (NER) from user queries using either LLM or specialized medical NLP models.

---

## Phase 3: ML & GNN Integration (Weeks 5-8)
**Goal**: Implement the GNN-based re-ranking and ONNX inference.

1.  **Data Preparation**:
    *   Export graph data from PostgreSQL to PyTorch Geometric (PyG) format.
    *   Generate node features (embeddings).
2.  **GNN Training (Python Integration)**:
    *   Train a **Graph Attention Network (GAT)** for node/edge relevance scoring.
    *   Export the trained model to **ONNX** format.
3.  **.NET ONNX Integration**:
    *   Implement `GnnInferenceService` using `Microsoft.ML.OnnxRuntime`.
    *   Implement AGE JSON to Tensor conversion logic.
4.  **Scoring & Ranking**:
    *   Integrate GNN scores into the `HybridSearchService` for context re-ranking.

---

## Phase 4: GraphRAG Pipeline & Explainability (Weeks 9-12)
**Goal**: Complete the full RAG pipeline and add XAI features.

1.  **End-to-End Pipeline**:
    *   Workflow: `Query` -> `NER` -> `Hybrid Search` -> `GNN Scoring` -> `Context Selection` -> `LLM Generation`.
2.  **Explainable AI (XAI)**:
    *   Extract attention weights from the GNN model.
    *   Visualize the "reasoning path" (subgraph) in the UI (API response).
    *   Generate natural language explanations of *why* specific information was retrieved.
3.  **Advanced FHIR**:
    *   Support for real-time FHIR synchronization and subscriptions.

---

## Phase 5: Production Readiness & Compliance (Weeks 13-16)
**Goal**: Performance, Security, and HIPAA.

1.  **Optimization**:
    *   Implement caching for embeddings and subgraphs.
    *   Optimize PG18/17 performance for HNSW and AGE.
2.  **Security & HIPAA**:
    *   Hardening Row Level Security (RLS) for multi-tenancy.
    *   Implement comprehensive audit logging for all medical data access.
    *   Encryption at rest and in transit.
3.  **Final Documentation & Deployment**:
    *   Kubernetes manifests and Helm charts.
    *   Prometheus/Grafana monitoring dashboards.

---

## Technical Stack (Confirmed)
*   **Runtime**: .NET 10 (C#)
*   **Database**: PostgreSQL 17/18 with Apache AGE 1.5.0 + pgvector 0.8.0
*   **AI SDK**: Microsoft Semantic Kernel 1.30.0
*   **ML Inference**: ONNX Runtime 1.20.1
*   **Medical Standard**: HL7 FHIR R4
*   **ORM**: Entity Framework Core 9.0
*   **Logging**: Serilog

---
**Plan Created**: 06.02.2026
**Based on**: Technical Specification v1.0 and Source Code Analysis.
