# Git Development Log

This file tracks the major development milestones and corresponding Git commits for the Lease Property Agent project.

## Development History

| Date       | Commit | Description                                                                                                                                            |
| ---------- | ------ | ------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 2026-09-07 | —      | Project workspace and Git repository initialized.                                                                                                      |
| 2026-09-07 | —      | Created the .NET 8 solution with API, Domain, Application, Infrastructure, and Test projects. Verified that the complete solution builds successfully. |
| 2026-09-07 | — | Configured project dependencies following a clean architecture: API → Application → Domain, with Infrastructure implementing application/domain concerns. |
| 2026-09-07 | `chore: configure project architecture` | Configured the .NET solution with separate API, Application, Domain, Infrastructure, and Test projects and verified the complete solution builds successfully. |
| 2026-09-07 | — | Added EF Core 8 with SQLite, configured the database context and entity relationships, and created/applied the InitialCreate migration. |
| 2026-09-07 | — | Added JSON catalog and ruleset integration tests covering unit loading, unit lookup, missing units, and owner rules R1-R7. |
| 2026-09-07 | — | Added deterministic unit matching against the owner catalog, including available, occupied, unknown, and missing unit scenarios. |
| 2026-09-07 | — | Added deterministic lease validation engine with R1 deposit-to-monthly-rent validation and automated PASS, FAIL, and NOT_DETERMINABLE tests. |
| 2026-09-07 | — | Added deterministic lease validation for owner rules R1 and R2, including PASS, FAIL, NOT_DETERMINABLE handling and automated tests. |
| 2026-09-07 | — | Added deterministic lease validation for R3, enforcing the maximum 36-month lease term with PASS, FAIL, and NOT_DETERMINABLE test coverage. |
| 2026-09-07 | — | Added deterministic lease validation for R4, validating expiry/commencement ordering and consistency between declared lease term and calendar date difference, with comprehensive edge-case tests. |
| 2026-09-07 | — | Added deterministic lease validation for R5, validating landlord and tenant presence and signatures with PASS, FAIL, and NOT_DETERMINABLE test coverage. |
| 2026-09-07 | — | Added deterministic lease validation for R6, validating annual rent against monthly rent × 12 with PASS, FAIL, and NOT_DETERMINABLE test coverage. |
| 2026-09-07 | — | Completed deterministic lease validation rules R1-R7, covering rent, escalation, term, date consistency, parties/signatures, annual rent, and unit availability with automated PASS, FAIL, and NOT_DETERMINABLE tests. |
| 2026-09-08 | — | Added page-aware document extraction and a stub lease agent with structured lease fields, confidence scores, source references, and extraction flags. | 
| 2026-09-08 | — | Completed the stub lease agent with evidence-driven field extraction, source traceability, confidence scores, missing-field detection, contradiction detection, and suspicious-value checks. |
| 2026-09-08 | — | Added the lease processing orchestration service connecting lease extraction, unit matching, domain lease mapping, and deterministic validation into one testable workflow. |
| 2026-09-08 | — | Added SQLite persistence infrastructure with lease and unit repositories, plus a property catalog seeder to synchronize the JSON property/unit catalog with the relational domain model. |
| 2026-09-08 | — | Added SQLite persistence infrastructure with lease and unit repositories, plus a property catalog seeder to synchronize the JSON property/unit catalog with the relational domain model. Added integration tests covering catalog import and idempotent seeding. |
| 2026-09-08 | — | Connected lease processing to persistence abstractions, including database unit resolution, domain mapping of extracted fields and flags, validation result persistence, and repository-based lease saving. |
| 2026-09-08 | — | Added an integration test for complete lease aggregate persistence using SQLite, covering the property/building/unit hierarchy, lease parties, extracted fields and source evidence, flags, and validation results. Verified the persisted aggregate can be reloaded correctly from the database. |
| 2026-09-09 | — | Extended issue image persistence with agent-generated observations and confidence scores, added the corresponding EF Core migration, and verified the database update with the full test suite. |
| 2026-09-09 | — | Added the property issue agent workflow, including image-based condition assessment, image-level evidence and confidence, draft work-order generation, SQLite persistence, API endpoint, clean response DTOs, and end-to-end integration tests. |
| 2026-09-09 | — | Added the human review workflow for lease fields, lease flags, and work orders, including accept/reject/edit actions, audit history through ReviewAction, SQLite persistence, and integration tests. | 
| 2026-09-09 | — | Added review API endpoints for lease fields, lease flags, and work orders, exposing the human review workflow through HTTP endpoints with validation and not-found handling. | 
| 2026-09-09 | — | Added HTTP integration coverage for the human review API, verifying lease field, lease flag, and work-order review endpoints, persistence, audit actions, and not-found handling; also isolated database integration tests from parallel execution. |
| 2026-09-09 | — | Added the unit workspace response models and application service, mapping unit, property, building, lease, source evidence, validation results, issues, images, and work orders into a frontend-safe workspace response; added automated mapping coverage. |
| 2026-09-09 | — | Added the unit workspace API, exposing a frontend-safe unit view with property and building details, lease information, source evidence, validation results, property issues, image assessments, and work orders; added end-to-end API integration tests. |
| 2026-09-09 | — | Added the unit listing API with repository and application service layers, exposing unit IDs, numbers, types, areas, and statuses for frontend workspace selection; added API integration tests including deterministic unit ordering. |
| 2026-09-10 | — | Connected the React property workspace to the unit listing and workspace APIs, added live unit selection, workspace loading states, and empty-state handling for leases and property issues. | 
| 2026-09-10 | — | Added the lease processing API and connected the frontend human-review workflow, allowing users to accept, reject, or edit extracted lease fields with source evidence and persisted review status; added integration test isolation and verified the full backend suite with 92/92 tests passing. |
## Commit Convention

This project follows Conventional Commit-style messages:

* `feat:` — New functionality
* `fix:` — Bug fix
* `refactor:` — Code restructuring
* `test:` — Tests
* `docs:` — Documentation
* `chore:` — Setup, configuration, or maintenance
