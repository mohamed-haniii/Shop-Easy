# ShopEasy

ShopEasy is an e-commerce data management console application built with **.NET 8**, **C#**, **Entity Framework Core**, and **SQL Server LocalDB**. The project focuses on backend fundamentals: database modeling, relationships, migrations, seeding, transactions, and query patterns for a shop system.

## Features

- Customer registration with related customer profile data.
- Product catalog with categories, tags, images, discounts, and reviews.
- Order processing with order items, payments, and customer relationships.
- EF Core entity configurations using Fluent API.
- JSON-based seed data for repeatable local setup.
- SQL Server database connection through `appsettings.json`.
- Migration support and stored procedure migration example.

## Tech Stack

- C#
- .NET 8
- Entity Framework Core 8
- SQL Server / LocalDB
- LINQ
- JSON configuration
- Code-first database modeling

## Skills Demonstrated

- Backend development with C# and .NET.
- Relational database design and entity relationships.
- EF Core DbContext, DbSet, migrations, and Fluent API configuration.
- Data seeding and application initialization.
- Transaction handling for multi-step operations.
- Console-based CRUD workflows and query logic.

## Main Domain Models

- `Customer`
- `CustomerProfile`
- `Product`
- `Category`
- `Tag`
- `Order`
- `OrderItem`
- `Payment`
- `Discount`
- `Review`
- `ProductImage`

## Run Locally

Requirements:

- .NET 8 SDK
- SQL Server LocalDB or SQL Server

Commands:

```bash
dotnet restore
dotnet build
dotnet run
```

Default connection string:

```json
"Server=(localdb)\\mssqllocaldb;Database=ShopEasyDb;Trusted_Connection=True;MultipleActiveResultSets=true"
```

## Project Structure

```text
Data/
  AppDbContext.cs
  DataSeeder.cs
  Configurations/
Models/
  JsonData/
Migrations/
Program.cs
appsettings.json
```

