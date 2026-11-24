# Hospital Management System

ASP.NET Core MVC application built with .NET 10, ASP.NET Core Identity, Entity Framework Core, SQL Server, SMTP email delivery, and a service/DTO-oriented architecture.

## Implementation phases

1. Authentication foundation
   - Registration and login
   - ASP.NET Core Identity password hashing
   - Forgot and reset password
   - Email OTP/two-factor login
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

Secrets must not be stored in source control. Use environment variables or `dotnet user-secrets` for SQL Server, SMTP, seed-admin, and public URL settings.

```bash
dotnet user-secrets init --project WebApplication1
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=HospitalManagementDb;User Id=sa;Password=YourStrongPassword1!;TrustServerCertificate=True" --project WebApplication1
dotnet user-secrets set "SeedAdmin:Email" "admin@example.com" --project WebApplication1
dotnet user-secrets set "SeedAdmin:Password" "ChangeMe123!" --project WebApplication1
dotnet user-secrets set "App:PublicBaseUrl" "https://localhost:5001" --project WebApplication1
dotnet user-secrets set "Email:Host" "smtp.example.com" --project WebApplication1
dotnet user-secrets set "Email:Username" "smtp-user" --project WebApplication1
dotnet user-secrets set "Email:Password" "smtp-password" --project WebApplication1
dotnet user-secrets set "Email:FromAddress" "noreply@example.com" --project WebApplication1
```

## Run

```bash
cd WebApplication1
dotnet ef database update
dotnet run
```
