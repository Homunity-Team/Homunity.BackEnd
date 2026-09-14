# Homunity Backend API

Homunity is a real-estate rental platform designed to help expatriate and non-local students find suitable accommodation and connect directly with property owners.

This repository contains the **.NET 8 Web API backend** responsible for authentication, users, properties, media uploads, search, and related business operations.

## Project Status

The backend is an MVP-oriented **layered monolith** built with ASP.NET Core .NET 8. The current codebase includes authentication, authorization, rate limiting, structured problem responses, Swagger/OpenAPI documentation, logging, property media handling, paging/sorting, and integration tests.

## Technology Stack

- **C# / .NET 8**
- **ASP.NET Core Web API**
- **SQL Server**
- **ADO.NET** for existing data-access operations
- **Entity Framework Core 8** for the newer property/query flows already present in the project
- **JWT Bearer Authentication**
- **Swagger / OpenAPI** with Swashbuckle
- **xUnit / WebApplicationFactory** for automated testing
- **ASP.NET Core Rate Limiting**
- **Microsoft.Extensions.Logging / Console logging**

> The project follows a layered structure. It is not presented here as Clean Architecture or as an implementation of a specific Design Pattern collection.

## Main Backend Responsibilities

### Authentication & Users

- User registration
- User login
- JWT access-token generation and validation
- Role-based authorization
- User profile retrieval
- Admin user status management
- Admin user deletion
- Authentication rate limiting

### Properties

- Create full property listings
- Update property listings
- Delete properties
- Paginated property listing retrieval
- Property lookup by ID
- Property lookup by owner
- Search by city and area
- Price-range filtering
- Sorting and pagination
- Search by university and nearby distance

### Media

Property creation and update endpoints support multipart uploads with validation for:

- Up to **6 images**
- Image types: `.jpg`, `.jpeg`, `.png`, `.webp`
- Maximum image size: **2 MB** each
- Video types: `.mp4`, `.webm`
- Maximum video size: **30 MB**

Uploaded media is stored under the application's `wwwroot` paths used by the current implementation.

## Project Structure

The exact solution may contain additional projects, but the main backend layers are organized around:

```text
Homunity_Web_Api/
├── Controllers/
├── Contracts/
├── Program.cs
└── Homunity_Web_Api.csproj

Homunity_Buisness Logic/
├── Services/
├── Exceptions/
└── Business logic

Homunity_Data_Access/
├── Data/
├── Repositories/
└── Database access

Homunity_Shared_DTOs/
└── Request / response DTOs

Homunity.Tests/
├── Unit/
└── Integration/
```

The spelling of the existing `Homunity_Buisness Logic` project name is preserved because it is part of the current solution/project reference.

## API Documentation

Swagger/OpenAPI is enabled for the Web API.

After starting the API, open:

```text
https://localhost:<your-port>/swagger
```

The port is intentionally not fixed in this README because ASP.NET Core launch settings can use different local ports.

The current Swagger configuration provides:

- API title and version information
- XML Documentation for documented controllers/actions
- Bearer/JWT authentication support
- The **Authorize** button for authenticated endpoints

### JWT in Swagger

Use the Swagger **Authorize** button and enter:

```text
Bearer <your-jwt-token>
```

The token is returned by the login endpoint after successful authentication.

## Configuration & Secrets

Sensitive configuration should not be stored in `appsettings.json` or committed to Git.

The API currently expects configuration values such as:

```text
ConnectionStrings:HomunityDb
Jwt:Key
Jwt:Issuer
Jwt:Audience
Cors:AllowedOrigins
```

For local development, use **ASP.NET Core User Secrets**.

Example commands:

```bash
dotnet user-secrets init
```

Then set the required values:

```bash
dotnet user-secrets set "ConnectionStrings:HomunityDb" "<your-connection-string>"
dotnet user-secrets set "Jwt:Key" "<your-secret-key>"
dotnet user-secrets set "Jwt:Issuer" "<your-issuer>"
dotnet user-secrets set "Jwt:Audience" "<your-audience>"
```

Do not replace these placeholders with real credentials in files committed to Git.

## Error Handling

The API uses `ProblemDetails` responses for common client/server errors, including:

- `400 Bad Request`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`
- `409 Conflict`
- `429 Too Many Requests`
- `500 Internal Server Error`

Unhandled exceptions are processed by the global exception-handling middleware. Detailed exception information is exposed only in the Development environment.

## Security

The current backend includes several security-related controls:

- JWT authentication
- Role-based authorization
- Restricted CORS origins
- Rate limiting for authentication requests
- Rate limiting for upload operations
- Security response headers
- Centralized exception handling
- Secret configuration through User Secrets/environment configuration

## Logging

Controllers and services use `ILogger<T>` for application logging.

Examples include:

- Successful user registration
- Rejected registration due to an existing phone number
- Failed login attempts
- Property deletion events

Sensitive credentials are not written directly to logs.

## Testing

The test project is located in `Homunity.Tests` and contains unit and integration coverage.

Run all tests from the solution directory with:

```bash
dotnet test
```

See [`Homunity.Tests/README.md`](Homunity.Tests/README.md) for the test-project-specific setup and scope.

## Development Workflow

A typical local workflow is:

```text
1. Configure User Secrets
        ↓
2. Start SQL Server / required database
        ↓
3. Build the solution
        ↓
4. Run the API
        ↓
5. Open Swagger
        ↓
6. Login and obtain a JWT
        ↓
7. Authorize Swagger with the JWT
        ↓
8. Test protected endpoints
        ↓
9. Run automated tests
```

## Build & Run

From the solution directory:

```bash
dotnet restore
dotnet build
dotnet run --project "Homunity_Web_Api"
```

The exact project path may vary depending on the local solution layout.

## Sprint 8 — Documentation

The Sprint 8 documentation work includes:

- XML Documentation generation for the Web API project
- XML comments for selected API actions
- Swagger/OpenAPI metadata and XML comment integration
- Bearer authentication documentation in Swagger
- README cleanup and technical documentation
- `.gitignore` review for build output and local secret/configuration files

## Notes for Contributors

Before committing changes:

```bash
dotnet build
dotnet test
```

Do not commit:

- `bin/` or `obj/`
- local Visual Studio files
- production-specific configuration containing secrets
- local secret/configuration files
- database files such as `.mdf` / `.ldf`

---

## License

Add the project's license information here when an official license is selected.
