# Specification: Admin Data Management & Tenant Provisioning

## Objective
Implement administrative functions for dynamic tenant schema initialization and synthetic medical data generation to facilitate system testing and scaling.

## Scope
- **Tenant Provisioning:** 
    - Automate the creation of tenant-specific database structures.
    - Initialize Apache AGE graphs and pgvector indices for new tenants.
- **Synthetic Data Generation:**
    - Develop a service to generate HL7 FHIR R4 compliant resources (Patient, Condition, Observation).
    - Provide a way to populate a tenant's database with generated data via API.
- **Admin API:**
    - Create `POST /api/admin/tenants` for provisioning.
    - Create `POST /api/admin/data/generate` for synthetic data population.

## Functional Requirements
1. **Provisioning Engine:**
    - Create a new record in `graphrag.tenants`.
    - Execute SQL scripts to set up RLS policies for the new tenant.
    - Initialize a new AGE graph name specific to the tenant if required.
2. **Data Generator:**
    - Support configurable data volume (e.g., number of patients).
    - Map generated FHIR resources to the system's Graph and Vector stores using existing ETL pipelines.

## Non-Functional Requirements
- **Security:** Admin endpoints must be protected (Auth placeholder for now).
- **Isolation:** Generation must strictly respect `tenant_id` boundaries.

## Acceptance Criteria
- [ ] A new tenant can be created via a single API call.
- [ ] Synthetic data for 10+ patients can be generated and fully indexed (SQL, Graph, Vector) via API.
- [ ] Generated data is queryable via the standard `/api/query` endpoint for the specific tenant.
