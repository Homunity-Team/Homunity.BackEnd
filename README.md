# Homunity Backend

ASP.NET Core Web API for the Homunity student-housing platform.

## Architecture

Layered structure:

| Project | Role |
|---------|------|
| **Homunity Web_Api** | REST controllers, JWT auth, rate limiting, static media (`wwwroot`) |
| **Homunity_Buisness Logic** | Application services, policies, Gemini client, payment gateway abstraction |
| **Homunity_Data Access** | EF Core `DbContext`, entities, Fluent configurations, repositories |
| **Shared** | DTOs by domain (Auth, Users, Properties, Bookings, AdminActions, …) |
| **Homunity.Tests** | Unit and integration tests |
| **Database_Script** | `sql.sql` — current SQL Server schema source of truth |

Primary data access is **EF Core**. Legacy ADO.NET (`cls*`) classes have been removed.

## Domains

- **Auth / Users** — register (Student/Owner only), login (JWT), profiles
- **Properties** — CRUD, images/videos, search, owner lists, status workflow
- **Bookings** — request / confirm / cancel
- **Payments** — mock gateway order + process + status
- **AdminActions** — approve / reject pending properties
- **Chat** — Gemini-backed assistant
- **Locations / Universities / Services / Roles** — reference data

## Auth

- JWT Bearer (`ClaimTypes.NameIdentifier` = user id, `ClaimTypes.Role` = role name)
- Roles: `Admin`, `Owner`, `Student`
- Password hashing: BCrypt (via `PasswordHasher`)
- Public registration **cannot** create Admin accounts (Owner=2, Student=3 only)

## Configuration

Use **User Secrets** (or environment variables) for the API project:

```
ConnectionStrings:HomunityDb = Server=...;Database=Homunity;...
Jwt:Key = (long random secret, 32+ chars)
Jwt:Issuer = ...
Jwt:Audience = ...
Jwt:ExpiryMinutes = 60
Gemini:ApiKey = ...   (if Chat is enabled)
Gemini:Model = ...
```

Do not commit real production secrets.

## Run

```bash
cd "Homunity Web_Api"
dotnet restore
dotnet run
```

Swagger is available in Development.

## Tests

```bash
cd Homunity.Tests
dotnet test
```

Integration tests expect a local SQL Server and JWT test configuration (see test project README).

## Notes

- Payment gateway is abstracted (`IPaymentGateway` / `MockPaymentGateway`) for future real providers.
- AI chat uses `IGeminiClient`.
- Controllers should depend on **services**, not repositories (reference data goes through `IReferenceDataService`).
