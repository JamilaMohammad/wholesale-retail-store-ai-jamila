# Database Migrations

This folder contains Entity Framework Core migrations for the CommerceHub database.

## Creating Migrations

To create a new migration after modifying models:

```bash
dotnet ef migrations add MigrationName
```

## Applying Migrations

To apply migrations to the database:

```bash
dotnet ef database update
```

## Migration Files

Migrations are automatically generated based on changes to your Entity Framework models. Each migration contains:

- **Up method**: Applies the changes to move the database forward
- **Down method**: Reverts the changes to move the database backward

## Initial Migration

The initial migration will be created when you first run:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

This will create all the tables defined in your DbContext:
- Customers
- Products  
- Orders
- OrderItems
- CartItems

## Best Practices

1. Always review generated migrations before applying them
2. Test migrations on a development database first
3. Backup production databases before applying migrations
4. Use descriptive names for migrations
5. Don't modify migration files after they've been applied to production