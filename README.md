# Clinic API

A RESTful API for managing a medical clinic's patients, doctors and appointments, built with ASP.NET Core and Entity Framework Core. This project was built as a portfolio piece to demonstrate backend development practices commonly expected for junior/mid-level .NET positions: layered architecture, repository pattern, JWT authentication, request validation, centralized error handling, containerization and automated testing.

## Features

- **Patients, Doctors and Appointments management** - full CRUD for patients and doctors, plus appointment scheduling with business-rule enforcement (no past-dated appointments, no double-booking for a doctor, and a controlled status workflow: Scheduled to Confirmed to Completed, or Cancelled at any point before completion).
- **JWT Authentication** - user registration and login with BCrypt password hashing; write operations (create/update/delete) require a valid bearer token.
- **Request validation** - all input DTOs are validated with FluentValidation before reaching the business logic.
- **Centralized error handling** - a global exception-handling middleware maps domain exceptions to the correct HTTP status codes and returns a consistent JSON error shape.
- **Dockerized** - the API and SQL Server both run in containers via Docker Compose; database migrations are applied automatically on startup.
- **Unit tested** - the service layer is covered by xUnit tests using Moq to isolate dependencies.
- **Interactive API docs** - Swagger UI with JWT bearer authorization support.

## Tech Stack

| Layer | Technology |
|---|---|
| Language / Runtime | C# / .NET 10 |
| Web Framework | ASP.NET Core Web API (Controllers) |
| ORM | Entity Framework Core (Code First + Migrations) |
| Database | SQL Server |
| Authentication | JWT Bearer + BCrypt |
| Validation | FluentValidation |
| Object Mapping | Mapster |
| API Documentation | Swashbuckle (Swagger UI) |
| Testing | xUnit + Moq |
| Containerization | Docker / Docker Compose |

## Architecture

The project follows a simple layered architecture rather than full Clean Architecture, which keeps the codebase approachable while still separating concerns clearly:

Controllers -> Services -> Repositories -> DbContext

- **Controllers** handle HTTP concerns only (routing, status codes).
- **Services** contain the business logic and validation rules, and throw domain-specific exceptions (e.g. AppointmentConflictException, PatientAlreadyExistsException).
- **Repositories** follow the Repository Pattern, with a generic IGenericRepository<T> base and entity-specific repositories for custom queries (e.g. GetByCpfAsync, HasConflictAsync).
- A global exception-handling middleware translates domain exceptions into standardized HTTP error responses, so controllers stay free of try/catch blocks.

## Project Structure

ClinicApi/
  Controllers/      API endpoints
  Services/         Business logic and domain exceptions
  Repositories/      Data access (Repository Pattern)
  Models/           EF Core entities
  DTOs/             Request/response contracts
  Validators/        FluentValidation rules
  Mapping/          Mapster configuration
  Middleware/        Global exception handling
  Common/           Shared response types
  Data/             DbContext
  Migrations/        EF Core migrations
ClinicApi.Tests/
  Services/         Unit tests for the service layer

## Getting Started

### Prerequisites

- .NET 10 SDK
- Docker Desktop

### Running with Docker (recommended)

This spins up both the API and SQL Server, and applies database migrations automatically on startup.

    docker compose up -d --build

The API will be available at http://localhost:8080, with Swagger UI at http://localhost:8080/swagger.

### Running locally

Start only the database:

    docker compose up -d sqlserver

Apply migrations and run the API:

    dotnet ef database update
    dotnet run

## API Overview

All endpoints are prefixed with `/api`. Write operations require a `Bearer` token obtained from `/api/Auth/login` or `/api/Auth/register`.

| Resource | Endpoints |
|---|---|
| Auth | `POST /register`, `POST /login` |
| Patients | `GET /`, `GET /{id}`, `POST /` *(auth)*, `PUT /{id}` *(auth)*, `DELETE /{id}` *(auth)* |
| Doctors | `GET /`, `GET /{id}`, `POST /` *(auth)*, `PUT /{id}` *(auth)*, `DELETE /{id}` *(auth)* |
| Appointments | `GET /`, `GET /{id}`, `POST /`, `PATCH /{id}/confirm`, `PATCH /{id}/complete`, `PATCH /{id}/cancel` *(all require auth)* |

Full request/response schemas are available in Swagger UI.

## Running Tests

    dotnet test

## Author

Built by Michael Claudino as a backend portfolio project.
