# NotesAdmin.Services

A personal microservice project for notes and categories management, built with Clean Architecture principles and modern .NET technologies.

## 🏗️ Architecture

This microservice follows Clean Architecture with the following layers:

```
AdminServices/
├── src/
│   ├── AdminServices.Domain/          # Core business logic and entities
│   ├── AdminServices.Application/     # Business use cases (CQRS with MediatR)
│   ├── AdminServices.Infrastructure/  # External services and data persistence
│   ├── AdminServices.Presentation/    # API endpoints and DTOs
│   └── AdminServices.WebApi/          # Entry point and configuration
├── tests/
│   ├── AdminServices.UnitTests/
│   ├── AdminServices.IntegrationTests/
│   └── AdminServices.FunctionalTests/
├── Infra/                             # Kubernetes manifests
└── Pipelines/                         # CI/CD pipelines
```

## ✨ Features

- ✅ **Clean Architecture** - Separation of concerns and dependency inversion
- ✅ **CQRS Pattern** - with MediatR for commands and queries
- ✅ **Domain-Driven Design** - Rich domain models
- ✅ **Repository Pattern** - Generic repository and Unit of Work
- ✅ **Azure Integration** - Blob Storage, Redis Cache, Service Bus
- ✅ **JWT Authentication** - Azure AD integration
- ✅ **API Documentation** - Swagger/OpenAPI with Scalar
- ✅ **Background Jobs** - Quartz.NET for scheduled tasks
- ✅ **Distributed Caching** - Redis for performance
- ✅ **Comprehensive Testing** - Unit, Integration, and Functional tests
- ✅ **Docker Support** - Multi-stage Dockerfile
- ✅ **Kubernetes Ready** - Full K8s manifests with HPA
- ✅ **CI/CD Pipelines** - Azure DevOps pipelines for DEV, QA, PRD

## 🚀 Technologies

- **.NET 8** - Latest LTS version
- **Entity Framework Core** - ORM with SQL Server
- **MediatR** - CQRS implementation
- **FluentValidation** - Input validation
- **AutoMapper** - Object mapping
- **Azure Storage** - Blob storage for files
- **Azure Service Bus** - Message queue
- **Redis** - Distributed caching
- **Quartz.NET** - Background job scheduling
- **Swagger** - API documentation
- **xUnit** - Unit testing
- **Docker** - Containerization
- **Kubernetes** - Orchestration

## 📋 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB or Azure SQL)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (optional)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

## 🔧 Local Development Setup

### 1. Clone the repository

```bash
cd AdminServices
```

### 2. Update appsettings

Edit `src/AdminServices.WebApi/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AdminServicesDb;Trusted_Connection=True;",
    "Redis": "localhost:6379"
  }
}
```

### 3. Run database migrations

```bash
cd src/AdminServices.WebApi
dotnet ef migrations add InitialCreate --project ../AdminServices.Infrastructure
dotnet ef database update
```

### 4. Run the application

```bash
dotnet run
```

The API will be available at:
- **Swagger UI**: https://localhost:7001/swagger
- **Scalar API Docs**: https://localhost:7001/scalar/v1
- **Health Check**: https://localhost:7001/health

## 🧪 Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run specific test project
dotnet test tests/AdminServices.UnitTests
```

## 🐳 Docker

### Build Docker image

```bash
docker build -t adminservices:latest -f Dockerfile.app .
```

### Run container

```bash
docker run -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="your-connection-string" \
  adminservices:latest
```

## ☸️ Kubernetes Deployment

### Apply manifests

```bash
# Create namespace
kubectl apply -f Infra/01-Ns-AdminServices.yml

# Create service account
kubectl apply -f Infra/01-Sa-AdminServices.yml

# Create config map
kubectl apply -f Infra/01-ConfigMap-AdminServices.yml

# Deploy application
kubectl apply -f Infra/01-Deployment-AdminServices.yml

# Create service
kubectl apply -f Infra/01-Svc-AdminServices.yml

# Enable autoscaling
kubectl apply -f Infra/01-Hpa-AdminServices.yml
```

### Check deployment

```bash
kubectl get pods -n adminservices
kubectl get svc -n adminservices
kubectl logs -f deployment/adminservices-app -n adminservices
```

## 📊 API Endpoints

### Notes

- `GET /api/notes` - Get all notes
- `POST /api/notes` - Create a new note
- `GET /api/notes/{id}` - Get note by ID
- `PUT /api/notes/{id}` - Update note
- `DELETE /api/notes/{id}` - Delete note

### Categories

- `GET /api/categories` - Get all categories
- `POST /api/categories` - Create a new category
- `GET /api/categories/{id}` - Get category by ID
- `PUT /api/categories/{id}` - Update category
- `DELETE /api/categories/{id}` - Delete category

### Auth

- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Login and get JWT token

### Health

- `GET /health` - Health check endpoint

## 🔐 Authentication

The API uses JWT Bearer authentication with Azure AD:

```bash
# Get token from Azure AD
curl -X POST https://login.microsoftonline.com/{tenant-id}/oauth2/v2.0/token \
  -d "client_id={client-id}" \
  -d "client_secret={client-secret}" \
  -d "grant_type=client_credentials" \
  -d "scope=api://{client-id}/.default"

# Use token in requests
curl -H "Authorization: Bearer {token}" https://localhost:7001/api/notes
```

## 📦 Azure Services Configuration

### Required Azure Resources

1. **Azure SQL Database**
   - Connection string in `appsettings.json`

2. **Azure Blob Storage**
   - For file uploads
   - Managed Identity authentication

3. **Azure Redis Cache**
   - For distributed caching
   - Connection string in `appsettings.json`

4. **Azure Service Bus**
   - For message queuing
   - Connection string in `appsettings.json`

5. **Azure Key Vault** (optional)
   - For secrets management

6. **Application Insights**
   - For telemetry and monitoring

## 🔄 CI/CD Pipeline

The project includes Azure DevOps pipelines for automated deployment:

### Environments

- **DEV** - Triggered on `dev` branch
- **QA** - Triggered on `qa` branch
- **PRD** - Triggered on `main` branch

### Pipeline Features

- ✅ Docker image build
- ✅ Push to Azure Container Registry
- ✅ Automated testing
- ✅ Multi-environment support

## 📝 Project Structure

```
AdminServices/
├── Domain Layer
│   ├── Entities (Note, Category, User)
│   ├── Primitives (Base Entity)
│   ├── Repositories (Interfaces)
│   └── Shared (Enums, Value Objects)
│
├── Application Layer
│   ├── Commands (Create, Update, Delete)
│   ├── Queries (GetAll, GetById)
│   ├── Validators (FluentValidation)
│   ├── Common (Interfaces, Models)
│   └── DependencyInjection
│
├── Infrastructure Layer
│   ├── Persistence (EF Core, DbContext)
│   ├── Services (Blob, Cache, ServiceBus)
│   ├── Common (HttpContext)
│   └── Options (Configuration)
│
├── Presentation Layer
│   ├── Modules (Endpoint definitions)
│   ├── Middlewares (Exception handling)
│   ├── Authentication (JWT setup)
│   ├── Authorization (Policies)
│   └── Filters (Swagger filters)
│
└── WebApi Layer
    ├── Program.cs (Entry point)
    ├── appsettings.json
    └── launchSettings.json
```

## 🤝 Contributing

This is a personal project, but feel free to fork and adapt it for your own use.

## 📄 License

This project is licensed under the MIT License.

## 👤 Author

Personal project for learning and demonstration purposes.

---

**Happy Coding! 🚀**
