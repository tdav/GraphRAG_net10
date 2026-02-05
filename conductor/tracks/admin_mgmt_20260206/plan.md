# Implementation Plan: Admin Data Management & Tenant Provisioning

## Phase 1: Tenant Provisioning Infrastructure
- [x] Task: Implement ITenantProvisioningService interface (c355fe1)
    - [ ] Create interface definition in Domain/Application
- [x] Task: Develop TenantProvisioningService (03546bb)
    - [ ] Write unit tests for tenant creation logic (Mock DB)
    - [ ] Implement SQL execution logic for RLS policies
    - [ ] Implement Apache AGE graph initialization for new tenants
- [x] Task: Conductor - User Manual Verification 'Tenant Provisioning Infrastructure' (Protocol in workflow.md)

## Phase 2: Synthetic Data Generation Service
- [ ] Task: Implement ISyntheticDataService interface
    - [ ] Define methods for generating Patient, Condition, and Observation resources
- [ ] Task: Develop SyntheticDataService
    - [ ] Write unit tests for FHIR resource generation (Correct format/schema)
    - [ ] Implement logic to generate randomized but clinically plausible medical data
    - [ ] Integrate with existing FhirEtlService for data population
- [ ] Task: Conductor - User Manual Verification 'Synthetic Data Generation Service' (Protocol in workflow.md)

## Phase 3: Admin API Implementation
- [ ] Task: Implement Admin Controllers
    - [ ] Create AdminTenantController with POST /api/admin/tenants
    - [ ] Create AdminDataController with POST /api/admin/data/generate
- [ ] Task: Add Integration Tests for Admin API
    - [ ] Write tests for tenant creation flow
    - [ ] Write tests for synthetic data generation and indexing flow
- [ ] Task: Conductor - User Manual Verification 'Admin API Implementation' (Protocol in workflow.md)

## Phase 4: Final Validation and Polish
- [ ] Task: Security Hardening
    - [ ] Implement basic API Key or Role check for Admin endpoints (Placeholder)
- [ ] Task: End-to-End Verification
    - [ ] Verify that a newly created tenant with generated data can be queried via standard search
- [ ] Task: Conductor - User Manual Verification 'Final Validation and Polish' (Protocol in workflow.md)




