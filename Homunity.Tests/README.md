# Homunity.Tests

Test Project for the **Homunity Backend — Sprint 7**.

---

## Requirements

Before running the tests, make sure the following requirements are available:

* A local **SQL Server** database is running (`localhost\Homunity`) using the same configuration used during development.
* **.NET 8 SDK** is installed on the machine.
* The `Homunity Web_Api` project has its **User Secrets** configured (`Jwt:Key`, `ConnectionStrings:HomunityDb`) as described in Sprint 4.

---

## Test Types

| Type                  | Location       | Description                                                                                                                  |
| ----------------------| -------------- | -----------------------------------------------------------------------------------------------------------------------------|
| **Unit Tests**        | `Unit/`        | Isolated tests for `PasswordHasher` and `JwtService`, with no database connection.                                           |
| **Integration Tests** | `Integration/` | Tests that run the real API through `WebApplicationFactory` and verify the behavior of the Endpoints (`Auth`, `Properties`). |

---

## Running the Tests

### Using Visual Studio

1. Open **Test Explorer** from **Test → Test Explorer**.
2. Select **Run All Tests**.

### Using the Command Line

From the **Solution** directory, run:

```bash
dotnet test
```

---

## Important Notes
# Homunity.Tests

Test Project for the **Homunity Backend — Sprint 7**.

---

## Requirements

Before running the tests, make sure the following requirements are available:

* A local **SQL Server** database is running (`localhost\Homunity`) using the same configuration used during development.
* **.NET 8 SDK** is installed on the machine.

---

## Test Types

| Type                  | Location       | Description                                                                                                                  |
| --------------------- | -------------- | ---------------------------------------------------------------------------------------------------------------------------- |
| **Unit Tests**        | `Unit/`        | Isolated tests for `PasswordHasher` and `JwtService`, with no database connection.                                           |
| **Integration Tests** | `Integration/` | Tests that run the real API through `WebApplicationFactory` and verify the behavior of the Endpoints (`Auth`, `Properties`). |

---

## Running the Tests

### Using Visual Studio

1. Open **Test Explorer** from **Test → Test Explorer**.
2. Select **Run All Tests**.

### Using the Command Line

From the **Solution** directory, run:

```bash
dotnet test
```

---

## Important Notes

* The **Integration Tests** connect to the real local database. There is **no In-Memory Database** at this stage. Make sure SQL Server is running before executing the tests.
* The `Jwt:Key` and `ConnectionStrings:HomunityDb` settings are explicitly defined inside the test files (`Integration/*.cs`) using the same values as the development **User Secrets**, ensuring that `WebApplicationFactory` works correctly without depending on external configuration.
* The `RegisterLoginAndReadProperties_FullFlow_Succeeds` test creates a new user on every execution using a unique phone number automatically generated to avoid data conflicts.

* The **Integration Tests** connect to the real local database. There is **no In-Memory Database** at this stage. Make sure SQL Server is running before executing the tests.
* The `Jwt:Key` and `ConnectionStrings:HomunityDb` settings are explicitly defined inside the test files (`Integration/*.cs`) using the same values as the development **User Secrets**, ensuring that `WebApplicationFactory` works correctly without depending on external configuration.
* The `RegisterLoginAndReadProperties_FullFlow_Succeeds` test creates a new user on every execution using a unique phone number automatically generated to avoid data conflicts.