# Employee Management

An employee management system built with ASP.NET Core in a layered architecture, with a separate Web API and MVC web app.

## Architecture

```
EmployeeManagement.Domain   entities
EmployeeManagement.Core     business logic and interfaces
EmployeeManagement.Common   shared helpers and DTOs
EmployeeManagement.API      ASP.NET Core Web API (JWT auth, Swagger)
EmployeeManagement.Web      ASP.NET Core MVC front end
```

## Features

- Authentication with ASP.NET Core Identity and JWT bearer tokens
- Data access with Entity Framework Core and SQL Server
- Object mapping with AutoMapper
- API documentation with Swagger

## Tech stack

C# · ASP.NET Core · Entity Framework Core · SQL Server · Identity · JWT · AutoMapper · Swagger

## Run it

1. Set the SQL Server connection string in `appsettings.json`
2. Apply migrations: `dotnet ef database update`
3. Run the API and web projects from `EmployeeManagement.sln`
