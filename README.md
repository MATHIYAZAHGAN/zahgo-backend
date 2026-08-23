# ZAH E-Commerce Backend

## Enterprise-Grade ASP.NET Core Web API

Production-ready, scalable e-commerce backend built with:
- **.NET 10** (Latest stable ASP.NET Core)
- **C# 13**
- **MongoDB** (Document Database)
- **Clean Architecture**
- **JWT Authentication**
- **RESTful APIs**
- **Swagger/OpenAPI Documentation**

## 🏗️ Architecture

```
ZAH.Ecommerce/
├── src/
│   ├── ZAH.API/              # API Layer (Controllers, Middleware, Configuration)
│   ├── ZAH.Application/      # Business Logic (Services, DTOs, Validators)
│   ├── ZAH.Domain/           # Domain Entities (Models, Value Objects, Enums)
│   ├── ZAH.Infrastructure/   # External Concerns (MongoDB, Auth, Payments, Email)
│   └── ZAH.Shared/           # Shared Resources (Constants, Extensions, Responses)
├── tests/                    # Unit & Integration Tests
├── docker/                   # Docker Configuration
└── docs/                     # API Documentation
```

## 🚀 Features

### Security
- ✅ JWT-based authentication with refresh tokens
- ✅ Role-based authorization (Customer, Admin, Manager)
- ✅ Password hashing with PBKDF2
- ✅ Rate limiting on critical endpoints
- ✅ CORS policy configuration
- ✅ Input validation using FluentValidation
- ✅ Global exception handling

### Performance
- ✅ Async/await throughout
- ✅ MongoDB indexes for optimized queries
- ✅ Caching-ready architecture
- ✅ Pagination for large datasets
- ✅ Response compression

### Reliability
- ✅ Structured logging with Serilog
- ✅ Health checks
- ✅ MongoDB connection resilience
- ✅ Graceful degradation
- ✅ Idempotent operations

### E-Commerce Features
- 🛍️ Product catalog with variants (colors, sizes)
- 📦 Inventory management with stock tracking
- 🛒 Shopping cart management
- ❤️ Wishlist functionality
- 🔖 Categories and brands
- 🎫 Coupon system
- 📝 Order management with complete lifecycle
- 💳 Payment integration architecture
- ⭐ Product reviews and ratings
- 🔍 Advanced search and filtering
- 📊 Order tracking

## 📋 Prerequisites

- .NET SDK 10.0 or later
- MongoDB 7.0 or later
- Visual Studio 2022 / VS Code / Rider

## 🔧 Getting Started

### 1. Clone the repository
```bash
git clone <repository-url>
cd zah-backend
```

### 2. Configure MongoDB
Update `appsettings.Development.json`:
```json
{
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "zah_ecommerce"
  }
}
```

### 3. Configure Secrets (Development)
```bash
cd src/ZAH.API
dotnet user-secrets init
dotnet user-secrets set "JWT:Secret" "your-256-bit-secret-key-here-minimum-32-characters"
dotnet user-secrets set "MongoDB:ConnectionString" "mongodb://localhost:27017"
```

### 4. Restore packages
```bash
dotnet restore
```

### 5. Build the solution
```bash
dotnet build
```

### 6. Run the API
```bash
cd src/ZAH.API
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger: `https://localhost:5001/swagger`

## 📚 API Documentation

### Base URL
```
https://api.zah.com/api/v1
```

### Endpoints

#### Authentication
- `POST /api/v1/auth/register` - Register new user
- `POST /api/v1/auth/login` - User login
- `POST /api/v1/auth/refresh` - Refresh access token
- `POST /api/v1/auth/logout` - User logout
- `POST /api/v1/auth/forgot-password` - Request password reset
- `POST /api/v1/auth/reset-password` - Reset password

#### Products
- `GET /api/v1/products` - Get all products (paginated)
- `GET /api/v1/products/{slug}` - Get product by slug
- `GET /api/v1/products/search` - Search products
- `POST /api/v1/products` - Create product (Admin)
- `PUT /api/v1/products/{id}` - Update product (Admin)
- `DELETE /api/v1/products/{id}` - Delete product (Admin)

#### Categories
- `GET /api/v1/categories` - Get all categories
- `GET /api/v1/categories/{slug}` - Get category by slug
- `POST /api/v1/categories` - Create category (Admin)

#### Cart
- `GET /api/v1/cart` - Get user cart
- `POST /api/v1/cart/items` - Add item to cart
- `PUT /api/v1/cart/items/{itemId}` - Update cart item
- `DELETE /api/v1/cart/items/{itemId}` - Remove cart item
- `DELETE /api/v1/cart` - Clear cart

#### Orders
- `GET /api/v1/orders` - Get user orders
- `GET /api/v1/orders/{id}` - Get order details
- `POST /api/v1/orders` - Create order (checkout)
- `PUT /api/v1/orders/{id}/cancel` - Cancel order

#### Wishlist
- `GET /api/v1/wishlist` - Get user wishlist
- `POST /api/v1/wishlist/{productId}` - Add to wishlist
- `DELETE /api/v1/wishlist/{productId}` - Remove from wishlist

## 🔒 Security Best Practices

- Never commit secrets to Git
- Use User Secrets for development
- Use Environment Variables or Secret Manager for production
- Rotate JWT secrets regularly
- Enable HTTPS in production
- Configure MongoDB authentication
- Implement rate limiting on authentication endpoints
- Use strong password policies

## 🐳 Docker Support

```bash
# Build Docker image
docker build -t zah-ecommerce-api -f docker/Dockerfile .

# Run with Docker Compose
docker-compose -f docker/docker-compose.yml up
```

## 🧪 Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## 📦 Database Seeding

Initial seed data is automatically created on first run:
- Sample products (24 products across 8 categories)
- Categories (Electronics, Fashion, Home, etc.)
- Brands
- Sample coupons

## 🔍 Monitoring & Health Checks

Health check endpoint:
- `GET /health` - Basic health status
- `GET /health/live` - Liveness probe
- `GET /health/ready` - Readiness probe (includes MongoDB check)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## 📄 License

This project is proprietary and confidential.

## 📧 Support

For support, email support@zah.com
