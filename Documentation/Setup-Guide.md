# Neoron API Setup Guide

## Prerequisites
- .NET 8.0 SDK
- SQL Server 2019 or later
- Docker Desktop (for development)
- Visual Studio 2022 or VS Code
- Git

## Development Environment Setup

### 1. Clone Repository
```bash
git clone https://github.com/yourusername/neoron.git
cd neoron
```

### 2. Database Setup
#### Option A: Using Docker (Recommended for Development)
```bash
# Start SQL Server container
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Password" -p 1433:1433 --name neoron-sql -d mcr.microsoft.com/mssql/server:2019-latest

# Apply migrations
dotnet ef database update --project Source/Neoron.API
```

#### Option B: Local SQL Server
1. Create a new database named 'Neoron'
2. Update connection string in appsettings.json
3. Apply migrations:
```bash
dotnet ef database update --project Source/Neoron.API
```

### 3. Configuration
1. Copy `appsettings.Example.json` to `appsettings.Development.json`
2. Update the following settings:
   - Database connection string
   - Authentication settings
   - Rate limiting configuration
   - Logging options

### 4. Running the Application
```bash
# Build solution
dotnet build

# Run API
cd Source/Neoron.API
dotnet run

# Run tests
dotnet test --settings Source/Neoron.API.Tests/test.runsettings
```

## Production Deployment

### 1. Infrastructure Requirements
- Azure App Service or similar hosting platform
- Azure SQL Database
- Azure Key Vault for secrets
- Azure Application Insights for monitoring
- Redis Cache (optional)

### 2. Environment Setup
1. Create Azure resources using provided ARM templates
2. Configure environment variables:
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ConnectionStrings__DefaultConnection=<sql-connection-string>
   Auth__Authority=<auth-authority>
   Monitoring__ApplicationInsights__InstrumentationKey=<key>
   ```

### 3. Deployment Steps
1. Build and publish:
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. Deploy to Azure:
   ```bash
   az webapp deployment source config-zip --src publish.zip --resource-group <group-name> --name <app-name>
   ```

### 4. Post-Deployment Verification
1. Check application health endpoint
2. Verify database migrations
3. Test authentication flow
4. Monitor application insights

## Monitoring Setup

### 1. Application Insights
1. Create Application Insights resource
2. Add instrumentation key to configuration
3. Configure custom metrics and alerts

### 2. Health Checks
- `/health` - Basic health check
- `/health/ready` - Readiness probe
- `/health/live` - Liveness probe

### 3. Logging
- Structured logging with Serilog
- Log aggregation in Application Insights
- Custom metric tracking

## Troubleshooting

### Common Issues
1. Database Connection
   - Verify connection string
   - Check firewall rules
   - Validate SQL Server credentials

2. Authentication
   - Verify JWT configuration
   - Check token validity
   - Validate client credentials

3. Performance
   - Monitor connection pool
   - Check cache configuration
   - Review query performance

### Support
For additional support:
1. Check GitHub issues
2. Review documentation
3. Contact development team

## Security Considerations

### 1. Authentication
- Configure Azure AD integration
- Set up API key validation
- Implement rate limiting

### 2. Data Protection
- Enable TLS/SSL
- Configure data encryption
- Implement audit logging

### 3. Monitoring
- Set up alerts
- Monitor security events
- Track authentication failures
