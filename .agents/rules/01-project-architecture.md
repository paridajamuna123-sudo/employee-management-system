---
trigger: always_on
description: Global architectural guidelines, design principles, and engineering lifecycle standards for EMS.
---

# Global Project Architecture Standards

## 1. Core Architectural Principles
- **Separation of Concerns & Clean Architecture**:
  - The solution structure adheres to Clean Architecture layers: **Domain**, **Application**, **Infrastructure**, and **Presentation**.
  - Maintain unidirectional dependency flow (Presentation & Infrastructure depend on Application & Domain; Domain has zero external dependencies).
- **SOLID & Clean Architecture**:
  - **Single Responsibility**: Every class, service, and component must have one well-defined purpose.
  - **Open/Closed**: Design code to be extensible without requiring rewrites of existing stable classes.
  - **Liskov Substitution & Interface Segregation**: Define focused, cohesive interfaces; avoid "god" interfaces.
  - **Dependency Inversion**: High-level modules must depend on abstractions, not concrete implementations.
- **Dependency Injection**: Always utilize DI containers (`IServiceCollection` in .NET, constructor injection in Angular). Avoid static service locators and ad-hoc singletons.
- **Composition over Inheritance**: Favor composition and aggregation over deep inheritance hierarchies.

## 2. API & Service Boundaries
- **Thin Controllers**: Controllers must handle HTTP transport concerns only (routing, status codes, model binding, authorization attributes). All validation and orchestration belongs in services.
- **Dedicated DTO Contracts**: Never expose Entity Framework entities directly across the wire. Always map to and from strongly typed DTOs.
- **Asynchronous I/O**: All database, network, and file I/O operations must be asynchronous end-to-end (`async`/`await` in C#, Observables in Angular).
- **Strongly Typed Models**: Strictly forbid magic strings and untyped dictionaries/objects (`any` in TypeScript, raw untyped `object` in C#) for business payloads.

## 3. Modification & Evolution Protocol
1. **Inspect First**: Always inspect existing code, entities, and services before writing new features.
2. **Preserve Existing Functionality**: Retain backwards compatibility for existing endpoints and contracts unless explicitly approved.
3. **No Gratuitous Rewrites**: Do not replace or refactor working modules simply because a different pattern looks newer.
4. **No Overengineering**: Keep the architecture proportionate to a modular monolith. Do not introduce microservices or event-driven message brokers unless explicitly requested.
5. **Small, Verifiable Steps**: Deliver features incrementally, verifying builds and functionality at each milestone.
