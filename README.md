# Library Management API

A RESTful API built with ASP.NET Core for managing books, authors, and library loans.

This project was developed to practice backend development with ASP.NET Core, Entity Framework Core, JWT authentication, RESTful API design, and automated testing.

## Technologies

* C# / ASP.NET Core Web API
* Entity Framework Core
* SQLite
* JWT Authentication
* xUnit

## Features

### Book and Author Management

* CRUD operations for books and authors
* Relationships between books and authors
* Book searching and pagination

### Borrowing and Returning

* Borrow available books
* Return borrowed books
* Track loan records
* Prevent borrowing unavailable books
* Prevent users from borrowing the same book multiple times

### Authentication and Authorization

* User registration and login
* JWT-based authentication
* Refresh token support
* Role-based authorization

### API Design and Error Handling

* RESTful endpoints using appropriate HTTP methods and status codes
* Global exception handling

### Testing

* Automated tests using xUnit
* Authentication and token validation tests
* Borrowing and returning functionality tests
* Tests for invalid operations and exception handling

## Getting Started

### Default Admin Account

A default admin account is automatically created on application startup, allowing immediate access to admin-restricted endpoints.

**Credentials:**
- **Username:** `Admin`
- **Password:** `PassWord`

Use these credentials to log in and obtain a JWT access token.

### Prerequisites

* .NET SDK
* SQLite

### Installation

1. Clone the repository:

   ```bash
   git clone <repository-url>
   cd LibraryManagementApi
   ```

2. Configure the application:

   - Install .NET 10 SDK and SQL Server.
   - Update the `UserDatabase` connection string in `appsettings.json` to match your SQL Server instance.
   - Configure the JWT `Token`, `Issuer`, and `Audience` settings. Use a securely stored signing key.

   - To configure the JWT `Token` using the following command is recommended
   ```bash
   $bytes = New-Object byte[] 64
   $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
   $rng.GetBytes($bytes)
   $rng.Dispose()

   dotnet user-secrets set "AppSettings:Token" ([Convert]::ToBase64String($bytes))
   ```

3. Apply database migrations:

   ```bash
   dotnet ef database update --project LibraryManagement.Api
   ```

4. Run the API:

   ```bash
   dotnet run --project LibraryManagement.Api
   ```

5. Access Swagger UI at `/swagger` or Scalar at `/scalar/v1` when running in the Development environment.

## API Documentation

The API endpoints can be explored and tested using Swagger UI when running the application locally.

## Running Tests

Run the automated tests using:

```bash
dotnet test
```

The tests cover core authentication, borrowing, and returning functionality, including successful operations and error scenarios.


## References & Acknowledgments

* **JWT Authentication:** Adapted from [ASP.NET Core JWT Authentication Tutorial - Patrick God](https://www.youtube.com/watch?v=6EEltKS8AwA).
* **Global Exception Handling:** Adapted from [Milan Jovanović's article on global error handling](https://milanjovanovic.tech/blog/global-error-handling-in-aspnetcore-from-middleware-to-modern-handlers).


## Project Purpose

This project serves as a practical exercise in building RESTful APIs using ASP.NET Core.

It focuses on implementing common backend functionality, working with relational databases, handling authentication and authorization, and writing automated tests.
