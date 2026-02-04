# Docker Setup for GraphRAG

This directory contains Docker configuration for running GraphRAG infrastructure.

## Prerequisites

- Docker 20.10+
- Docker Compose 2.0+

## Quick Start

### 1. Build and Start PostgreSQL

```bash
docker-compose up -d postgres
```

This will:
- Build a custom PostgreSQL 17 image with Apache AGE and pgvector extensions
- Create the `graphrag_db` database
- Initialize extensions (AGE, pgvector, pgcrypto, pg_trgm, uuid-ossp)
- Create the `medical_graph` Apache AGE graph
- Create all necessary tables with indexes and RLS policies

### 2. Verify Database is Running

```bash
docker-compose ps
docker-compose logs postgres
```

### 3. Connect to Database

```bash
docker exec -it graphrag-postgres psql -U graphrag_user -d graphrag_db
```

Test AGE extension:
```sql
LOAD 'age';
SET search_path = ag_catalog, "$user", public;

-- List graphs
SELECT * FROM ag_catalog.ag_graph;

-- Query graph
SELECT * FROM cypher('medical_graph', $$
    MATCH (n)
    RETURN n
    LIMIT 10
$$) as (n agtype);
```

Test pgvector extension:
```sql
-- Check embeddings table
SELECT COUNT(*) FROM graphrag.embeddings;

-- Test vector operations (after adding data)
SELECT id, text, vector <-> '[0.1,0.2,...]'::vector as distance
FROM graphrag.embeddings
ORDER BY vector <-> '[0.1,0.2,...]'::vector
LIMIT 5;
```

## Database Configuration

### Default Credentials (Development Only)
- **Host**: localhost
- **Port**: 5432
- **Database**: graphrag_db
- **User**: graphrag_user
- **Password**: graphrag_password

**⚠️ WARNING**: Change these credentials for production!

### Connection String (.NET)
```
Host=localhost;Port=5432;Database=graphrag_db;Username=graphrag_user;Password=graphrag_password
```

## Schema Overview

The database includes:

### Core Tables
- `graphrag.tenants` - Organizations/hospitals
- `graphrag.users` - System users
- `graphrag.conversations` - Chat sessions

### Medical Tables (FHIR-based)
- `graphrag.patients` - Patient records
- `graphrag.conditions` - Diagnoses
- `graphrag.medication_requests` - Prescriptions
- `graphrag.observations` - Clinical measurements

### Graph Tables
- `graphrag.graph_nodes` - Knowledge graph nodes
- `graphrag.graph_edges` - Knowledge graph relationships
- `graphrag.concepts` - Medical terminology (SNOMED CT, LOINC, RxNorm)

### AI/ML Tables
- `graphrag.embeddings` - Vector embeddings with HNSW index

## Extensions Installed

1. **Apache AGE 1.5.0** - Graph database (openCypher)
2. **pgvector 0.8.0** - Vector similarity search
3. **pgcrypto** - Encryption functions
4. **pg_trgm** - Text similarity search
5. **uuid-ossp** - UUID generation

## Security Features

### Row Level Security (RLS)
All tenant-scoped tables have RLS enabled. The application should set:

```sql
SET LOCAL app.current_tenant_id = '<tenant_uuid>';
```

### Indexes
- HNSW index on embeddings for fast KNN search
- GIN indexes for JSON and text search
- B-tree indexes for foreign keys and common queries

## Maintenance

### Stop Services
```bash
docker-compose down
```

### Stop and Remove Data
```bash
docker-compose down -v
```

### View Logs
```bash
docker-compose logs -f postgres
```

### Rebuild Image
```bash
docker-compose build --no-cache postgres
docker-compose up -d postgres
```

### Backup Database
```bash
docker exec graphrag-postgres pg_dump -U graphrag_user graphrag_db > backup.sql
```

### Restore Database
```bash
docker exec -i graphrag-postgres psql -U graphrag_user -d graphrag_db < backup.sql
```

## Troubleshooting

### Container won't start
```bash
docker-compose logs postgres
docker-compose down -v
docker-compose up -d postgres
```

### Extensions not loading
The init scripts run only on first startup. To reinitialize:
```bash
docker-compose down -v
docker-compose up -d postgres
```

### Performance tuning
Edit `docker-compose.yml` to add PostgreSQL configuration:
```yaml
command:
  - "postgres"
  - "-c"
  - "shared_buffers=256MB"
  - "-c"
  - "max_connections=100"
  - "-c"
  - "work_mem=16MB"
```

## Next Steps

1. Build and start the .NET API service
2. Import sample FHIR data
3. Create embeddings for concepts
4. Test hybrid search functionality

## Documentation

- [PostgreSQL 17 Documentation](https://www.postgresql.org/docs/17/)
- [Apache AGE Documentation](https://age.apache.org/)
- [pgvector Documentation](https://github.com/pgvector/pgvector)
- [openCypher Query Language](https://opencypher.org/)
