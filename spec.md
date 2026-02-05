# Technical Specification: GraphRAG for Healthcare (Explainable AI)

## 1. Project Overview
GraphRAG_net10 is a specialized Retrieval-Augmented Generation (RAG) system built on .NET 10. It integrates a Knowledge Graph (Apache AGE) and Vector Search (pgvector) within a single PostgreSQL 17/18 instance to provide explainable AI (XAI) for clinical decision support.

## 2. Technology Stack
- **Runtime**: .NET 10 (C# 13)
- **Database**: PostgreSQL 17/18
  - **Extensions**: `age` (Graph), `vector` (Embeddings), `pgcrypto`, `pg_trgm`, `uuid-ossp`.
- **AI Orchestration**: Microsoft Semantic Kernel
- **ML Inference**: ONNX Runtime (GAT/GNN models)
- **Medical Standard**: HL7 FHIR R4
- **Architecture**: Clean Architecture (Domain, Application, Infrastructure, Api)

## 3. Data Model
### 3.1 Medical Entities (FHIR R4)
- **Patient**: Demographic data.
- **Condition**: Diagnoses (SNOMED CT).
- **MedicationRequest**: Prescriptions (RxNorm).
- **Observation**: Clinical measurements (LOINC).

### 3.2 Graph Model (Apache AGE)
- **Nodes**: Clinical entities (Resources) and Medical Concepts.
- **Edges**: Relationships (e.g., `Patient -> HasCondition -> Condition`, `Condition -> TreatedBy -> Medication`).
- **Labels**: `Patient`, `Condition`, `Observation`, `Medication`, `Concept`.

### 3.3 Vector Model (pgvector)
- **Table**: `graphrag.embeddings`
- **Dimensions**: Dependent on the model (e.g., 1536 for OpenAI `text-embedding-3-small`).
- **Search**: Cosine similarity (`<=>` operator) with HNSW indexing.

## 4. Key Components
### 4.1 Hybrid Search Service
Combines vector-based semantic search with graph-based relationship traversal.
1. **Vector Step**: Find top-K nodes similar to the query embedding.
2. **Graph Step**: Expand those nodes by 1-2 hops to find contextually relevant neighbors.
3. **Fusion**: Rank results using GNN scores.

### 4.2 GNN Inference Engine
Uses a Graph Attention Network (GAT) to score the relevance of retrieved subgraphs.
- **Input**: Subgraph nodes and edges.
- **Output**: Importance scores and attention weights.
- **Integration**: ONNX Runtime in .NET.

### 4.3 Explainable AI (XAI)
Provides a "reasoning path" by visualizing the subgraph and attention weights that influenced the LLM response.

## 5. Security and Compliance
- **Multi-tenancy**: Row Level Security (RLS) in PostgreSQL based on `tenant_id`.
- **HIPAA Compliance**: Encryption at rest/transit, comprehensive audit logging.
- **Data Isolation**: Strict separation of patient data between clinics.

## 6. API Endpoints
- `POST /api/query`: Primary RAG interface.
- `POST /api/fhir/import`: Batch import of FHIR resources.
- `GET /api/health`: System health and extension status.
