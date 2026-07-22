# Copilot instructions for TNTU Internship 2026

## Project structure

- `src/Projects.Api/` — ASP.NET Core Web API (.NET 8).
  - `Controllers/` API endpoints
  - `Services/` business logic
  - `Data/` EF Core `DbContext`
  - `Models/` domain models and DTOs
  - `Errors/` custom error types
- `src/Projects.Api.Tests/` — xUnit test project.
  - `unit/` unit tests
  - `integration/` integration tests using `WebApplicationFactory<Program>`
- `docs/` architecture, prerequisites, schedule, and user stories.
- `.github/workflows/` CI/CD workflows for build, test, and deploy.

## Coding conventions

- Target framework is **.NET 8** with nullable reference types enabled.
- Follow existing API layering: **Controller -> Service -> Data/DbContext**.
- Use dependency injection (`IProjectService`, `ProjectContext`) via `Program.cs`.
- Keep endpoints versioned under `api/v{version:apiVersion}/...` and use lowercase URLs.
- Use asynchronous methods with `Async` suffix for service and controller operations.
- Match current model/property naming patterns used in this repository (including existing lower-cased domain fields like `id`, `name`, `description`, `isArchived`, `createdAt`).
- Return appropriate HTTP responses and `ProblemDetails` for errors.

## Test framework

- Tests use **xUnit** (`[Fact]`) with:
  - **Moq** and **MockQueryable** for unit test mocking
  - **Microsoft.EntityFrameworkCore.InMemory** and `WebApplicationFactory` for integration tests
- Test projects live in `src/Projects.Api.Tests`.

## Build, test, and run

From repository root:

- Restore:
  - `dotnet restore src/Projects.Api/Projects.Api.csproj`
- Build:
  - `dotnet build src/Projects.Api/Projects.Api.csproj --configuration Release --no-restore`
- Test:
  - `dotnet test src/Projects.Api.Tests/Projects.Api.Tests.csproj --configuration Release`
- Run API locally:
  - `dotnet run --project src/Projects.Api/Projects.Api.csproj`

## Notes for Copilot changes

- Prefer minimal, focused edits.
- Keep architecture and naming consistent with existing code and tests.
- When changing behavior, update/add tests in `src/Projects.Api.Tests`.
