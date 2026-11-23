# Hospital Management System

ASP.NET Core MVC application built with .NET 10, ASP.NET Core Identity, Entity Framework Core, SQL Server, and a service/DTO-oriented architecture.

## Implementation phases

1. Authentication foundation
   - Registration and login
   - ASP.NET Core Identity password hashing
   - Forgot and reset password
   - OTP login
   - Logout
   - Audit history
   - Admin and user roles
   - Role-based redirects and MVC pages
2. Hospital management
   - Patients
   - Doctors
   - Appointments
   - Departments
   - Staff
   - Billing and reports

## Development configuration

Secrets must not be stored in source control. Use environment variables or `dotnet user-secrets` for SQL Server and SMTP credentials.

Example SQL Server Docker setup:

```bash
docker run --name hospital-sql \
  -e ACCEPT_EULA=Y \
  -e MSSQL_SA_PASSWORD='YourStrongPassword1!' \
  -p 1433:1433 \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

Use this local connection string as a starting point:

```text
Server=localhost,1433;Database=HospitalManagementDb;User Id=sa;Password=YourStrongPassword1!;TrustServerCertificate=True
```
