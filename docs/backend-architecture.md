# 🏗️ Architecture Guidelines: EleveRats Backend (The Mother-Ship's Blueprint)

> "Built on rock, not on sand. Our foundations are solid because they honor the Master Architect."

This document outlines the architectural decisions and patterns used in the EleveRats backend. The system is designed as a **Modular Monolith** applying **Clean Architecture** principles to maximize testability, maintainability, and domain isolation. Just like a well-prepared RPG party, every layer has its role, and they don't step on each other's toes.

## 1. Core Principles (The Tenets)

* **Persistence Ignorance:** The core domain is a hermit; it has no knowledge of databases, web frameworks, or ORMs. It only cares about the pure truth of the business.
* **Dependency Inversion (SOLID):** The flow of dependencies is a one-way street pointing inward. The Application layer defines the contracts (Interfaces), and the outer layers (Infrastructure) are the hired hands that implement them.
* **Strict Model Separation:**
  * **Domain Entities:** Rich, strictly typed models that protect their invariants like a Paladin protects the weak. No empty constructors or public setters.
  * **Persistence Models (DbRecords):** Anemic ("dumb") classes designed exclusively for Entity Framework Core mapping. The Repository pattern acts as the translator between Domain and Persistence.

## 2. Layer Definitions (The Stratigraphy)

### 🎯 1. Domain (The Holy of Holies)

The heart of the software.

* **Responsibilities:** Hosts Entities, Value Objects, Domain Enums, and custom business exceptions.
* **Rules:** Zero external dependencies. Entity states can only be mutated through specific domain methods (e.g., `AddRpgPoints()`), ensuring the "character sheet" is never in an invalid state.

### 🧠 2. Application (The War Room)

The orchestrator.

* **Responsibilities:** Contains the Use Cases (e.g., `RegisterUserHandler`) and defines the interfaces for external communication (e.g., `IUserRepository`).
* **Rules:** Coordinates the interaction between the Domain and external interfaces. It contains application logic (fetching data, calling domain methods, saving) but no pure business rules. It's the dungeon master managing the flow.

### 🏗️ 3. Infrastructure & Persistence (The Gatekeepers)

The adapters for the external world.

* **Responsibilities:** Interacts with databases, message queues, and third-party APIs.
* **Rules:**
  * Hosts Entity Framework Core and PostgreSQL configurations.
  * Uses **UUID v7** for primary keys to optimize relational database indexing and keep our "chronicle" sorted.
  * Implements advanced Audit Trails using `JSONB` columns in PostgreSQL (`CreatedBy`, `UpdatedBy`, `DeletedBy`) to store request metadata (e.g., `TraceId`, `UserId`).
  * Contains the concrete Repository implementations that encapsulate SQL queries and map `DbRecords` back to rich Domain Entities.

### 🚪 4. Presentation (The Outer Walls)

The entry point.

* **Responsibilities:** Handles HTTP requests and responses.
* **Rules:** Built with **.NET Minimal APIs**. Endpoints are lean: they only validate incoming requests, invoke the appropriate Use Case in the Application layer, and return the mapped HTTP response. Organized for high cohesion, keeping the gates clean.

---

> "For from him and through him and to him are all things. To him be glory forever. Amen."
> — **Romans 11:36**
