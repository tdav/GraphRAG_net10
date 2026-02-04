#!/bin/bash
set -e

echo "Initializing GraphRAG database extensions..."

# Create extensions
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    -- Enable Apache AGE extension
    CREATE EXTENSION IF NOT EXISTS age;
    LOAD 'age';
    SET search_path = ag_catalog, "$user", public;
    
    -- Enable pgvector extension
    CREATE EXTENSION IF NOT EXISTS vector;
    
    -- Enable pgcrypto for encryption
    CREATE EXTENSION IF NOT EXISTS pgcrypto;
    
    -- Enable pg_trgm for text search
    CREATE EXTENSION IF NOT EXISTS pg_trgm;
    
    -- Enable uuid-ossp for UUID generation
    CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
    
    -- Create AGE graph
    SELECT create_graph('medical_graph');
    
    GRANT USAGE ON SCHEMA ag_catalog TO $POSTGRES_USER;
    GRANT ALL ON ALL TABLES IN SCHEMA ag_catalog TO $POSTGRES_USER;
    GRANT ALL ON ALL SEQUENCES IN SCHEMA ag_catalog TO $POSTGRES_USER;
    
    -- Log successful initialization
    \echo 'GraphRAG database extensions initialized successfully'
EOSQL

echo "Database initialization complete!"
