# Library Management API

An API built with ASP.NET Core for managing books, authors, and library loans.

This project was developed to practice backend development with ASP.NET Core, Entity Framework Core, JWT authentication, and automated testing.

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

### Testing

* Automated tests using xUnit
* Authentication and token validation tests
* Borrowing and returning functionality tests
* Tests for invalid operations and exception handling

## Getting Started

### Prerequisites

* .NET SDK
* SQLite

### Installation

1. Clone the repository:

   ```bash
   git clone <repository-url>
   cd <repository-name>
   ```

2. Configure the database connection and JWT settings in the application configuration.

3. Apply database migrations:

   ```bash
   dotnet ef database update
   ```

4. Run the API:

   ```bash
   dotnet run --project LibraryManagement.Api
   ```

## API Documentation

The API endpoints can be explored and tested using Swagger UI when running the application locally.

## Running Tests

Run the automated tests using:

```bash
dotnet test
```

The tests cover core authentication, borrowing, and returning functionality, including successful operations and error scenarios.

## Project Purpose

This project serves as a practical exercise in building APIs using ASP.NET Core.

It focuses on implementing common backend functionality, working with relational databases, handling authentication and authorization, and writing automated tests.
