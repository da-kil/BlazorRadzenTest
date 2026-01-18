# ti8m BeachBreak - Questionnaire Management System

A .NET 10 application implementing a CQRS/Event Sourcing architecture for questionnaire management with Blazor WebAssembly frontend and PostgreSQL backend.

## Architecture Overview

- **Domain-Driven Design** with Event Sourcing using Marten
- **CQRS** with separate Command and Query APIs
- **Blazor WebAssembly** frontend with Radzen UI components
- **PostgreSQL** database for event store and read models
- **.NET Aspire** for local development orchestration

## Prerequisites

### Required Software

1. **[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)** - Latest version
2. **[Docker Desktop](https://www.docker.com/products/docker-desktop/)** - For PostgreSQL database
3. **[Visual Studio 2022](https://visualstudio.microsoft.com/)** or **[JetBrains Rider](https://www.jetbrains.com/rider/)** - Recommended IDEs
4. **[Git](https://git-scm.com/)** - Version control

### Optional Tools

- **[pgAdmin](https://www.pgadmin.org/)** - Database administration (included in Aspire setup)
- **[Postman](https://www.postman.com/)** - API testing

## Initial Setup

### 1. Clone and Restore

```bash
# Clone the repository
git clone <your-repo-url>
cd BlazorRadzenTest

# Restore NuGet packages
dotnet restore ti8m.BeachBreak.sln
```

### 2. Install Required .NET Tools

```bash
# Install JSON serializer registration generator
dotnet tool install --global ProgrammerAL.JsonSerializerRegistrationGenerator.Runner --version 1.0.0.46

# Install .NET Aspire workload
dotnet workload install aspire

# Install Roslyn SDK (required for source generation)
dotnet workload install microsoft-net-sdk-roslyn
```

### 3. Generate JSON Serialization Code

The solution uses automated JSON serialization code generation for AOT compatibility:

```bash
# Run the JSON serialization generator
JsonSerializerRegistrationGenerator.Runner generate --input . --output .
```

This tool generates the necessary JSON serialization contexts for all DTOs and domain objects.

### 4. Build the Solution

```bash
# Build entire solution to verify setup
dotnet build ti8m.BeachBreak.sln
```

## Running the Application

### Option 1: Using .NET Aspire (Recommended)

.NET Aspire provides orchestrated local development with automatic database setup:

```bash
# Navigate to Aspire host project
cd Aspire/ti8m.BeachBreak.AppHost

# Run the application stack
dotnet run
```

This will start:
- **Command API** (backend for write operations)
- **Query API** (backend for read operations)
- **Frontend** (Blazor WebAssembly application)
- **PostgreSQL** database with automatic migration
- **pgAdmin** for database management
- **Aspire Dashboard** for monitoring and logging

The Aspire dashboard will open in your browser showing all running services and their endpoints.

### Option 2: Manual Setup (Development/Debugging)

If you need to run individual components for debugging:

#### 1. Start Database

```bash
# Start PostgreSQL using Docker
docker run --name beachbreak-postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:15

# Or use the provided docker-compose file
docker-compose -f docker-compose.db.yml up -d
```

#### 2. Run APIs

```bash
# Terminal 1: Command API
cd 03_Infrastructure/ti8m.BeachBreak.CommandApi
dotnet run

# Terminal 2: Query API
cd 03_Infrastructure/ti8m.BeachBreak.QueryApi
dotnet run

# Terminal 3: Frontend
cd 05_Frontend/ti8m.BeachBreak
dotnet run
```

## Database Setup

### Automatic Setup (Aspire)

When using Aspire, the database is automatically:
- Created in Docker container
- Migrated to latest schema
- Seeded with initial data
- Accessible via pgAdmin

### Manual Setup

If running manually, set up the database:

```bash
# Run setup script (Windows)
.\setup-database.ps1

# Or run setup script (Linux/Mac)
./setup-database.sh

# Or run SQL directly
psql -h localhost -U postgres -f setup-database.sql
```

### Database Connection Strings

Default connection strings (modify in `appsettings.Development.json` if needed):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ti8m_beachbreak;Username=postgres;Password=postgres"
  }
}
```

## Application URLs

When running via Aspire, check the Aspire Dashboard for current URLs. Default endpoints:

- **Frontend**: `https://localhost:7001`
- **Command API**: `https://localhost:7002`
- **Query API**: `https://localhost:7003`
- **pgAdmin**: `http://localhost:8080`
- **Aspire Dashboard**: `https://localhost:15000`

## Development Workflow

### 1. Making Changes

```bash
# Make your code changes

# Regenerate JSON serialization if you added/modified DTOs
JsonSerializerRegistrationGenerator.Runner generate --input . --output .

# Build and test
dotnet build ti8m.BeachBreak.sln
dotnet test
```

### 2. Database Changes

Event sourcing means no traditional migrations. New events are automatically handled.

For read model changes:
1. Update read model classes
2. Run projection replay if needed (see admin panel)

### 3. Frontend Development

The frontend uses Radzen UI components. For UI changes:

```bash
# Navigate to frontend
cd 05_Frontend/ti8m.BeachBreak.Client

# Watch for changes during development
dotnet watch run
```

## Authentication Setup

The application uses Entra ID (Azure AD) authentication:

1. **Local Development**: Uses development authentication with preset users
2. **Production**: Requires Entra ID configuration

### Development Users

Default development users (no authentication required locally):
- **Admin**: Full system access
- **HR Manager**: HR operations
- **Team Lead**: Team management
- **Employee**: Basic access

## Translation Support

The application supports English and German:

### Adding Translations

1. Add translation keys to your Razor components:
```razor
<RadzenText>@T("sections.my-new-section")</RadzenText>
```

2. Add entries to `TestDataGenerator/test-translations.json`:
```json
{
  "key": "sections.my-new-section",
  "german": "Mein neuer Abschnitt",
  "english": "My New Section",
  "category": "sections"
}
```

3. Validate translations:
```bash
powershell -ExecutionPolicy Bypass -File TestDataGenerator\validate-translations.ps1
```

## Troubleshooting

### Common Issues

#### Build Errors

```bash
# Clear package cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore ti8m.BeachBreak.sln

# Rebuild
dotnet build ti8m.BeachBreak.sln
```

#### Database Connection Issues

1. Ensure PostgreSQL is running: `docker ps`
2. Check connection string in `appsettings.Development.json`
3. Verify PostgreSQL port (default: 5432)

#### Aspire Issues

```bash
# Update Aspire workload
dotnet workload update aspire

# Clear Aspire cache
dotnet aspire clean
```

#### JSON Serialization Errors

```bash
# Regenerate serialization code
JsonSerializerRegistrationGenerator.Runner generate --input . --output .

# Rebuild solution
dotnet build ti8m.BeachBreak.sln
```

### Logs and Debugging

- **Aspire Dashboard**: Real-time logging and metrics
- **Browser Console**: Frontend debugging
- **Application Insights**: (Production only)

## Project Structure

```
ti8m.BeachBreak/
├── 01_Domain/                 # Domain models, aggregates, events
├── 02_Application/            # Commands, queries, handlers
│   ├── Command/              # Write-side operations
│   └── Query/                # Read-side operations
├── 03_Infrastructure/         # APIs and data access
│   ├── CommandApi/           # Write API
│   ├── QueryApi/             # Read API
│   └── Infrastructure.Marten/ # Event store implementation
├── 04_Core/                   # Shared building blocks
├── 05_Frontend/               # Blazor WebAssembly frontend
│   ├── ti8m.BeachBreak/      # Host project
│   └── ti8m.BeachBreak.Client/ # WebAssembly client
├── Aspire/                    # .NET Aspire orchestration
└── Tests/                     # Unit and integration tests
```

## Contributing

### Development Guidelines

1. **Follow CQRS patterns** - separate commands and queries
2. **Use event sourcing** - domain events are source of truth
3. **Apply DDD principles** - rich domain models
4. **Write tests** - especially for domain logic
5. **Follow coding standards** - see `CLAUDE.md` for detailed patterns

### Code Review Checklist

- [ ] Follows established patterns from `CLAUDE.md`
- [ ] All new translations added to `test-translations.json`
- [ ] JSON serialization regenerated if DTOs changed
- [ ] Tests pass locally
- [ ] No hardcoded connection strings or secrets

## Resources

- **[.NET Aspire Documentation](https://docs.microsoft.com/aspire)**
- **[Marten Documentation](https://martendb.io/)**
- **[Radzen Components](https://blazor.radzen.com/)**
- **[Event Sourcing Patterns](https://docs.microsoft.com/azure/architecture/patterns/event-sourcing)**

## Support

For issues or questions:
1. Check this README and `CLAUDE.md`
2. Review existing GitHub issues
3. Create a new issue with detailed description and logs

---

**Happy coding!** 🚀