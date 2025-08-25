## ⚠️ ATTENTION: IMPORTANT NOTICE

**🚫 DEVELOPMENT CURRENTLY HALTED**

Please be aware that this project relies on Entity Framework Core's internal mechanisms to generate view schemas. Unfortunately, I discovered that EF Core's approach to view generation is primarily designed for data fetching operations rather than creating optimized database views.

**Key limitations discovered:**
 - No support for column aliases in generated SQL
 -  Limited query optimization capabilities
 -   Generated view SQL may not be production-ready
 -   Missing database-specific view features
   
This project was put on hold after encountering these fundamental limitations in EF Core's view generation approach. 

# EFCoreViewTool
![EF Core](https://img.shields.io/badge/EF%20Core-View%20Migrations%20Tool-blue?style=flat-square)
![CLI Tool](https://img.shields.io/badge/CLI-Tool-green?style=flat-square)

An EF Core CLI tool that generates database view migrations using C# instead of writing raw SQL. Define your views using type-safe C# queries and let the tool handle the migration generation.

## Features

- **C# View Definitions**: Define database views using type-safe C# queries
- **View Type Support**: Create both materialized and normal views
- **Migration Generation**: Automatically generates EF Core migrations for views
- **EF Core Integration**: Seamlessly works with existing EF Core migration workflows
- **Cross-Platform**: Works on Windows, Linux, and macOS

## Installation

### As a Global Tool

```bash
dotnet tool install --global EFCoreViewTool
```

### As a Local Tool

```bash
# In your project directory
dotnet new tool-manifest
dotnet tool install EFCoreViewTool
```

## Prerequisites

- .NET 6.0 SDK or later
- EF Core in your project
- Existing EF Core migrations setup

## Usage

### 1. Define Your View Model

Create a model class for your view:

```csharp
public class ProductSummary
{
    public int ProductId { get; set; }
    public string Name { get; set; }
    public decimal TotalSales { get; set; }
    public int OrderCount { get; set; }
}
```

### 2. Create a View Configurator

Implement the `IViewConfigurator` interface to define your view:

```csharp
using EFCoreViewTool.Core.Contracts;
using Microsoft.EntityFrameworkCore;

public class ProductSummaryViewConfigurator : IViewConfigurator<MyDbContext, ProductSummary>
{
    public IQueryable<ProductSummary> ConfigureView(MyDbContext dbContext)
    {
        return dbContext.Products
            .Join(dbContext.Orders,
                p => p.Id,
                o => o.ProductId,
                (p, o) => new { Product = p, Order = o })
            .GroupBy(x => new { x.Product.Id, x.Product.Name })
            .Select(g => new ProductSummary
            {
                ProductId = g.Key.Id,
                Name = g.Key.Name,
                TotalSales = g.Sum(x => x.Order.Quantity * x.Order.UnitPrice),
                OrderCount = g.Count()
            });
    }
}
```

### 3. Register the View in Your DbContext

Tell EF Core that this is a view, not a table:

```csharp
public class MyDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    
    // Register the view
    public DbSet<ProductSummary> ProductSummaries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure the view as not a table
        modelBuilder.Entity<ProductSummary>().ToView("ProductSummaries");
        modelBuilder.Entity<ProductSummary>().HasNoKey();
    }
}
```

### 4. Generate Migrations

Use the CLI tool to generate migrations:

```bash
# Add a new view migration
efcoreview add <MigrationName> [options] #same as ef migrations options
```

### 5. Apply Migrations

Apply the generated migrations like standard EF Core migrations:

```bash
dotnet ef database update
```

## View Types

The tool supports two types of views:

1. **Standard Views** (`ViewType.Standard`): Standard database views
2. **Materialized Views** (`ViewType.Materialized`): Pre-computed views that store results physically

## Example

### View Configurator Definition

```csharp
[ViewType(ViewType.Standard)]
public class CustomerOrderSummaryViewConfigurator : IViewConfigurator<MyDbContext, CustomerOrderSummary>
{
    public IQueryable<CustomerOrderSummary> ConfigureView(MyDbContext dbContext)
    {
        return dbContext.Orders
            .Join(dbContext.Customers,
                o => o.CustomerId,
                c => c.Id,
                (o, c) => new { Order = o, Customer = c })
            .GroupBy(x => new { x.Customer.Id, x.Customer.Name })
            .Select(g => new CustomerOrderSummary
            {
                CustomerId = g.Key.Id,
                CustomerName = g.Key.Name,
                TotalOrders = g.Count(),
                TotalAmount = g.Sum(x => x.Order.TotalAmount)
            });
    }
}
```

### Generated Migration

The tool will generate a migration containing the appropriate SQL for your database provider:

```csharp
public partial class AddProductSummaryView : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE MATERIALIZED VIEW vw_ProductSummaries AS
            SELECT 
                p.Id as ProductId,
                p.Name as Name,
                SUM(o.Quantity * o.UnitPrice) as TotalSales,
                COUNT(o.Id) as OrderCount
            FROM Products p
            INNER JOIN Orders o ON p.Id = o.ProductId
            GROUP BY p.Id, p.Name
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP MATERIALIZED VIEW vw_ProductSummaries");
    }
}
```

## Benefits

1. **Type Safety**: Catch errors at compile time rather than runtime
2. **Maintainability**: C# code is easier to read and maintain than raw SQL
3. **Refactoring Support**: IDE refactoring tools work with C# code
4. **Database Portability**: The tool can generate database-specific SQL
5. **Version Control**: Migrations work with EF Core's migration history


## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Inspired by the EF Core migrations system
- Built on the great foundation of Entity Framework Core

---

**Note**: This tool is designed to work with EF Core 6.0 and later. Some features may not be available with earlier versions.
