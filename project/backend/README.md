# CommerceHub Backend API

A C# ASP.NET Core Web API for the CommerceHub e-commerce platform with role-based pricing for wholesalers and retailers.

## Features

- **Authentication & Authorization**: JWT-based authentication with role-based access
- **Customer Management**: Registration and login for wholesalers and retailers
- **Product Catalog**: CRUD operations for products with category filtering
- **Shopping Cart**: Persistent cart functionality with real-time updates
- **Order Management**: Complete order processing workflow
- **Role-based Pricing**: Different pricing tiers for wholesalers vs retailers
- **MSSQL Database**: Entity Framework Core with SQL Server integration

## Database Schema

### Tables
- **Customers**: User accounts with client type (wholesaler/retailer)
- **Products**: Product catalog with wholesale and retail pricing
- **CartItems**: Shopping cart persistence
- **Orders**: Order headers with customer and shipping information
- **OrderItems**: Individual items within orders

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server or SQL Server LocalDB
- Visual Studio 2022 or VS Code

### Setup Instructions

1. **Clone and Navigate**
   ```bash
   cd backend/CommerceHub.API
   ```

2. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

3. **Update Connection String**
   - Edit `appsettings.json` to match your SQL Server configuration
   - For production, use environment variables or Azure Key Vault

4. **Create Database**
   ```bash
   dotnet ef database update
   ```

5. **Run the Application**
   ```bash
   dotnet run
   ```

The API will be available at `https://localhost:7001` and `http://localhost:5001`.

### API Endpoints

#### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration

#### Products
- `GET /api/products` - Get all products (with search and category filters)
- `GET /api/products/{id}` - Get specific product
- `GET /api/products/categories` - Get all categories
- `POST /api/products` - Create new product (admin only)

#### Cart
- `GET /api/cart` - Get current user's cart
- `POST /api/cart/items` - Add item to cart
- `PUT /api/cart/items/{id}` - Update cart item quantity
- `DELETE /api/cart/items/{id}` - Remove item from cart
- `DELETE /api/cart` - Clear entire cart

#### Orders
- `GET /api/orders` - Get current user's orders
- `GET /api/orders/{id}` - Get specific order
- `POST /api/orders` - Create order from cart

### Configuration

#### JWT Settings
Update the `JwtSettings` section in `appsettings.json`:
```json
{
  "JwtSettings": {
    "SecretKey": "YourSecretKeyHere",
    "Issuer": "CommerceHub.API",
    "Audience": "CommerceHub.Client",
    "ExpiryInDays": 7
  }
}
```

#### Database Connection
Update the connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=CommerceHubDB;Trusted_Connection=true;"
  }
}
```

### Frontend Integration

The React frontend should make API calls to these endpoints. Update the frontend's API base URL to point to your backend:

```typescript
const API_BASE_URL = 'https://localhost:7001/api';
```

### Security Considerations

- Change the JWT secret key in production
- Use HTTPS in production
- Implement proper input validation
- Add rate limiting for API endpoints
- Use environment variables for sensitive configuration
- Implement proper logging and monitoring

### Database Migrations

To add new migrations:
```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Testing

The API includes Swagger documentation available at `/swagger` when running in development mode.

### Deployment

For production deployment:
1. Update connection strings and JWT settings
2. Build the application: `dotnet publish -c Release`
3. Deploy to your preferred hosting platform (Azure, AWS, etc.)
4. Ensure SQL Server is accessible from your hosting environment