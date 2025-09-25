# EF Core Demo (MySQL / Pomelo)

This sample shows:
- Code First (migrations)
- Data Annotations + Fluent API
- Transactions & resilient retries
- Optimistic Concurrency using a MySQL `timestamp(6)` column (`UpdatedAt`)
- Simple seeding

## Prereqs
```bash
dotnet --version
mysql --version
```

## Packages
```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Pomelo.EntityFrameworkCore.MySql
dotnet add package Microsoft.EntityFrameworkCore.Design
```

## Connection String
Edit `appsettings.json`:
```
Server=localhost;Port=3306;Database=ef_demo_db;User Id=root;Password=your_password;TreatTinyAsBoolean=false;
```

## Migrations
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Notes on Concurrency
- MySQL has no `rowversion` type. We use `UpdatedAt timestamp(6)` as a concurrency token.
- The column is configured with `DEFAULT CURRENT_TIMESTAMP(6)` and `ON UPDATE CURRENT_TIMESTAMP(6)` so every write updates it automatically.
- EF Core marks it as `IsConcurrencyToken()`; updates/delete include the original value in the WHERE clause.
