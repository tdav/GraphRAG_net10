# Implementation Plan: GraphRAG Backend Core & AI Integration

## Phase 1: Infrastructure Realization (Current Focus)
**Goal**: Replace all skeletons in `GraphRAG.Infrastructure` with working code.

### 1.1 Apache AGE Repository Implementation
- Implement `ExecuteCypherQueryAsync` using `ag_catalog.cypher` and `Npgsql`.
- Ensure proper mapping of AGE JSON results to `GraphNode` and `GraphEdge` entities.
- Implement `GetSubgraphAsync` to retrieve 1-2 hop neighbors for a given set of start nodes.

### 1.2 PgVector Refinement
- Finalize `VectorRepository.SearchSimilarAsync` with proper HNSW index usage.
- Implement automatic embedding generation using Semantic Kernel during data ingestion.

### 1.3 FHIR Ingestion Pipeline
- Implement `FhirRepository` to parse HL7 FHIR R4 Bundles.
- Map FHIR resources to Graph Nodes and create edges based on FHIR References (e.g., `Condition.subject` -> `Patient`).

## Phase 2: AI Orchestration (Semantic Kernel)
- **Goal**: Connect the LLM to the database and graph.
- Implement `GraphQueryPlugin` for Semantic Kernel.
- Implement `VectorMemoryPlugin` for Semantic Kernel.
- Configure `GraphRagService` to orchestrate:
  1. NER (Entity Extraction) from query.
  2. Hybrid Search (Vector + Graph).
  3. Context Assembly for LLM.

## Phase 3: ML Integration (GNN & ONNX)
- **Goal**: Implement re-ranking and XAI.
- Develop `GnnInferenceService` using `Microsoft.ML.OnnxRuntime`.
- Implement tensor preparation: Convert AGE subgraph JSON to `node_features` and `edge_index` tensors.
- Integrate attention weights extraction for XAI visualization.

## Phase 4: Verification & Performance
- **Goal**: Ensure reliability and speed.
- Implement Integration Tests using `Testcontainers` (PostgreSQL + AGE + vector).
- Benchmark Cypher query performance on large medical graphs.
- Validate RLS policies to ensure no cross-tenant data leakage.

## Phase 5: Production Readiness
- Complete Audit Logging system.
- Implement HIPAA-compliant encryption management.
- Finalize OpenAPI documentation and client SDKs.
