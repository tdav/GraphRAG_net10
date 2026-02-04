-- GraphRAG Database Schema
-- This script creates the base tables for the GraphRAG system

SET LOCAL client_min_messages TO WARNING;

-- Create schema for application tables
CREATE SCHEMA IF NOT EXISTS graphrag;

-- Set search path
SET search_path TO graphrag, public, ag_catalog;

-- ============================================
-- Core Tables
-- ============================================

-- Tenants table (organizations, hospitals, clinics)
CREATE TABLE IF NOT EXISTS graphrag.tenants (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    description TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    configuration JSONB,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    is_deleted BOOLEAN DEFAULT FALSE
);

-- Users table
CREATE TABLE IF NOT EXISTS graphrag.users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES graphrag.tenants(id),
    email VARCHAR(255) UNIQUE NOT NULL,
    full_name VARCHAR(255) NOT NULL,
    role VARCHAR(50) NOT NULL,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    is_deleted BOOLEAN DEFAULT FALSE
);

-- Conversations table
CREATE TABLE IF NOT EXISTS graphrag.conversations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES graphrag.tenants(id),
    user_id UUID NOT NULL REFERENCES graphrag.users(id),
    title VARCHAR(500) NOT NULL,
    messages_json JSONB DEFAULT '[]'::jsonb,
    last_activity_at TIMESTAMP DEFAULT NOW(),
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    is_deleted BOOLEAN DEFAULT FALSE
);

-- ============================================
-- Medical Tables (FHIR-based)
-- ============================================

-- Patients table
CREATE TABLE IF NOT EXISTS graphrag.patients (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES graphrag.tenants(id),
    fhir_id VARCHAR(255) NOT NULL,
    name VARCHAR(500) NOT NULL,
    birth_date DATE,
    gender VARCHAR(50),
    fhir_data_json JSONB,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    is_deleted BOOLEAN DEFAULT FALSE,
    UNIQUE(tenant_id, fhir_id)
);

-- Conditions table (diagnoses)
CREATE TABLE IF NOT EXISTS graphrag.conditions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES graphrag.tenants(id),
    fhir_id VARCHAR(255) NOT NULL,
    patient_id UUID NOT NULL REFERENCES graphrag.patients(id),
    code VARCHAR(100) NOT NULL,
    code_system VARCHAR(500) NOT NULL,
    display VARCHAR(500) NOT NULL,
    clinical_status VARCHAR(50),
    onset_date TIMESTAMP,
    fhir_data_json JSONB,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    is_deleted BOOLEAN DEFAULT FALSE,
    UNIQUE(tenant_id, fhir_id)
);

-- MedicationRequests table
CREATE TABLE IF NOT EXISTS graphrag.medication_requests (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES graphrag.tenants(id),
    fhir_id VARCHAR(255) NOT NULL,
    patient_id UUID NOT NULL REFERENCES graphrag.patients(id),
    medication_code VARCHAR(100) NOT NULL,
    code_system VARCHAR(500) NOT NULL,
    medication_display VARCHAR(500) NOT NULL,
    status VARCHAR(50) NOT NULL,
    dosage_instructions TEXT,
    authored_on TIMESTAMP,
    fhir_data_json JSONB,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    is_deleted BOOLEAN DEFAULT FALSE,
    UNIQUE(tenant_id, fhir_id)
);

-- Observations table
CREATE TABLE IF NOT EXISTS graphrag.observations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES graphrag.tenants(id),
    fhir_id VARCHAR(255) NOT NULL,
    patient_id UUID NOT NULL REFERENCES graphrag.patients(id),
    code VARCHAR(100) NOT NULL,
    code_system VARCHAR(500) NOT NULL,
    display VARCHAR(500) NOT NULL,
    value TEXT,
    unit VARCHAR(50),
    status VARCHAR(50) NOT NULL,
    effective_date_time TIMESTAMP,
    fhir_data_json JSONB,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    is_deleted BOOLEAN DEFAULT FALSE,
    UNIQUE(tenant_id, fhir_id)
);

-- ============================================
-- Graph Tables
-- ============================================

-- GraphNodes table
CREATE TABLE IF NOT EXISTS graphrag.graph_nodes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES graphrag.tenants(id),
    label VARCHAR(100) NOT NULL,
    properties_json JSONB DEFAULT '{}'::jsonb,
    graph_name VARCHAR(100) DEFAULT 'medical_graph',
    age_vertex_id BIGINT,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    is_deleted BOOLEAN DEFAULT FALSE
);

-- GraphEdges table
CREATE TABLE IF NOT EXISTS graphrag.graph_edges (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES graphrag.tenants(id),
    source_node_id UUID NOT NULL REFERENCES graphrag.graph_nodes(id),
    target_node_id UUID NOT NULL REFERENCES graphrag.graph_nodes(id),
    edge_type VARCHAR(100) NOT NULL,
    properties_json JSONB DEFAULT '{}'::jsonb,
    graph_name VARCHAR(100) DEFAULT 'medical_graph',
    age_edge_id BIGINT,
    weight DOUBLE PRECISION,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    is_deleted BOOLEAN DEFAULT FALSE
);

-- Concepts table (medical terminology)
CREATE TABLE IF NOT EXISTS graphrag.concepts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES graphrag.tenants(id),
    code VARCHAR(100) NOT NULL,
    system VARCHAR(500) NOT NULL,
    display VARCHAR(500) NOT NULL,
    definition TEXT,
    parent_concepts_json JSONB,
    embedding_id UUID,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    is_deleted BOOLEAN DEFAULT FALSE,
    UNIQUE(tenant_id, system, code)
);

-- ============================================
-- AI/ML Tables
-- ============================================

-- Embeddings table (with pgvector)
CREATE TABLE IF NOT EXISTS graphrag.embeddings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES graphrag.tenants(id),
    text TEXT NOT NULL,
    vector vector(3072),  -- text-embedding-3-large dimensions
    model VARCHAR(100) NOT NULL,
    entity_type VARCHAR(100),
    entity_id UUID,
    metadata_json JSONB,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    is_deleted BOOLEAN DEFAULT FALSE
);

-- ============================================
-- Indexes
-- ============================================

-- Core indexes
CREATE INDEX IF NOT EXISTS idx_users_tenant_id ON graphrag.users(tenant_id) WHERE is_deleted = FALSE;
CREATE INDEX IF NOT EXISTS idx_users_email ON graphrag.users(email) WHERE is_deleted = FALSE;
CREATE INDEX IF NOT EXISTS idx_conversations_user_id ON graphrag.conversations(user_id) WHERE is_deleted = FALSE;

-- Medical indexes
CREATE INDEX IF NOT EXISTS idx_patients_tenant_id ON graphrag.patients(tenant_id) WHERE is_deleted = FALSE;
CREATE INDEX IF NOT EXISTS idx_conditions_patient_id ON graphrag.conditions(patient_id) WHERE is_deleted = FALSE;
CREATE INDEX IF NOT EXISTS idx_medication_requests_patient_id ON graphrag.medication_requests(patient_id) WHERE is_deleted = FALSE;

-- Graph indexes
CREATE INDEX IF NOT EXISTS idx_graph_nodes_tenant_id ON graphrag.graph_nodes(tenant_id) WHERE is_deleted = FALSE;
CREATE INDEX IF NOT EXISTS idx_graph_edges_source ON graphrag.graph_edges(source_node_id) WHERE is_deleted = FALSE;
CREATE INDEX IF NOT EXISTS idx_graph_edges_target ON graphrag.graph_edges(target_node_id) WHERE is_deleted = FALSE;

-- Embeddings indexes
CREATE INDEX IF NOT EXISTS idx_embeddings_tenant_id ON graphrag.embeddings(tenant_id) WHERE is_deleted = FALSE;

-- HNSW vector index for similarity search
CREATE INDEX IF NOT EXISTS embeddings_vector_hnsw_idx 
ON graphrag.embeddings USING hnsw (vector vector_l2_ops)
WITH (m = 16, ef_construction = 64)
WHERE is_deleted = FALSE;

-- GIN indexes for JSON search
CREATE INDEX IF NOT EXISTS idx_conditions_fhir_data_gin ON graphrag.conditions USING gin (fhir_data_json);

-- Text search indexes
CREATE INDEX IF NOT EXISTS idx_patients_name_trgm ON graphrag.patients USING gin (name gin_trgm_ops) WHERE is_deleted = FALSE;

-- ============================================
-- Row Level Security (RLS)
-- ============================================

ALTER TABLE graphrag.users ENABLE ROW LEVEL SECURITY;
ALTER TABLE graphrag.conversations ENABLE ROW LEVEL SECURITY;
ALTER TABLE graphrag.patients ENABLE ROW LEVEL SECURITY;
ALTER TABLE graphrag.conditions ENABLE ROW LEVEL SECURITY;
ALTER TABLE graphrag.medication_requests ENABLE ROW LEVEL SECURITY;
ALTER TABLE graphrag.observations ENABLE ROW LEVEL SECURITY;
ALTER TABLE graphrag.graph_nodes ENABLE ROW LEVEL SECURITY;
ALTER TABLE graphrag.graph_edges ENABLE ROW LEVEL SECURITY;
ALTER TABLE graphrag.concepts ENABLE ROW LEVEL SECURITY;
ALTER TABLE graphrag.embeddings ENABLE ROW LEVEL SECURITY;

-- Create RLS policies
CREATE POLICY tenant_isolation_policy_users ON graphrag.users
    USING (tenant_id = current_setting('app.current_tenant_id', TRUE)::UUID);

CREATE POLICY tenant_isolation_policy_conversations ON graphrag.conversations
    USING (tenant_id = current_setting('app.current_tenant_id', TRUE)::UUID);

CREATE POLICY tenant_isolation_policy_patients ON graphrag.patients
    USING (tenant_id = current_setting('app.current_tenant_id', TRUE)::UUID);

CREATE POLICY tenant_isolation_policy_conditions ON graphrag.conditions
    USING (tenant_id = current_setting('app.current_tenant_id', TRUE)::UUID);

CREATE POLICY tenant_isolation_policy_medication_requests ON graphrag.medication_requests
    USING (tenant_id = current_setting('app.current_tenant_id', TRUE)::UUID);

CREATE POLICY tenant_isolation_policy_observations ON graphrag.observations
    USING (tenant_id = current_setting('app.current_tenant_id', TRUE)::UUID);

CREATE POLICY tenant_isolation_policy_graph_nodes ON graphrag.graph_nodes
    USING (tenant_id = current_setting('app.current_tenant_id', TRUE)::UUID);

CREATE POLICY tenant_isolation_policy_graph_edges ON graphrag.graph_edges
    USING (tenant_id = current_setting('app.current_tenant_id', TRUE)::UUID);

CREATE POLICY tenant_isolation_policy_concepts ON graphrag.concepts
    USING (tenant_id = current_setting('app.current_tenant_id', TRUE)::UUID);

CREATE POLICY tenant_isolation_policy_embeddings ON graphrag.embeddings
    USING (tenant_id = current_setting('app.current_tenant_id', TRUE)::UUID);

-- ============================================
-- Helper Functions
-- ============================================

CREATE OR REPLACE FUNCTION graphrag.update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create triggers
CREATE TRIGGER update_tenants_updated_at BEFORE UPDATE ON graphrag.tenants
    FOR EACH ROW EXECUTE FUNCTION graphrag.update_updated_at_column();
CREATE TRIGGER update_users_updated_at BEFORE UPDATE ON graphrag.users
    FOR EACH ROW EXECUTE FUNCTION graphrag.update_updated_at_column();
CREATE TRIGGER update_conversations_updated_at BEFORE UPDATE ON graphrag.conversations
    FOR EACH ROW EXECUTE FUNCTION graphrag.update_updated_at_column();

DO $$
BEGIN
    RAISE NOTICE 'GraphRAG schema created successfully!';
END $$;
