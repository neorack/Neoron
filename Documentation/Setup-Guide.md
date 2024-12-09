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
- DigitalOcean Droplet (Ubuntu 22.04 LTS recommended)
- DigitalOcean Managed Database (PostgreSQL)
- DigitalOcean Spaces for storage (optional)
- Redis Cache (optional)
- Nginx for reverse proxy

### 2. Server Setup
1. Create Droplet and configure basic security:
   ```bash
   # Update system
   sudo apt update && sudo apt upgrade -y

   # Install .NET 8.0
   wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb
   sudo dpkg -i packages-microsoft-prod.deb
   sudo apt update
   sudo apt install -y dotnet-sdk-8.0

   # Install Nginx
   sudo apt install -y nginx

   # Configure firewall
   sudo ufw allow OpenSSH
   sudo ufw allow 'Nginx Full'
   sudo ufw enable
   ```

2. Configure environment variables:
   ```bash
   # Add to /etc/environment
   sudo nano /etc/environment

   ASPNETCORE_ENVIRONMENT=Production
   ConnectionStrings__DefaultConnection=<postgresql-connection-string>
   Auth__JwtSecret=<your-jwt-secret>
   Redis__ConnectionString=<redis-connection-string>
   ```

### 3. Deployment Steps
1. Build and publish locally:
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. Configure Nginx:
   ```bash
   # Create Nginx config
   sudo nano /etc/nginx/sites-available/neoron

   server {
       listen 80;
       server_name your-domain.com;

       location / {
           proxy_pass http://localhost:5000;
           proxy_http_version 1.1;
           proxy_set_header Upgrade $http_upgrade;
           proxy_set_header Connection keep-alive;
           proxy_set_header Host $host;
           proxy_cache_bypass $http_upgrade;
           proxy_set_header X-Real-IP $remote_addr;
           proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
           proxy_set_header X-Forwarded-Proto $scheme;
       }
   }

   # Enable site
   sudo ln -s /etc/nginx/sites-available/neoron /etc/nginx/sites-enabled/
   sudo nginx -t
   sudo systemctl restart nginx
   ```

3. Deploy application:
   ```bash
   # Create service user
   sudo useradd -m neoronservice
   sudo mkdir -p /var/www/neoron
   sudo chown neoronservice:neoronservice /var/www/neoron

   # Copy files to server
   scp -r ./publish/* neoronservice@your-server:/var/www/neoron/

   # Create service
   sudo nano /etc/systemd/system/neoron.service

   [Unit]
   Description=Neoron API
   After=network.target

   [Service]
   WorkingDirectory=/var/www/neoron
   ExecStart=/usr/bin/dotnet /var/www/neoron/Neoron.API.dll
   Restart=always
   RestartSec=10
   User=neoronservice
   Environment=ASPNETCORE_ENVIRONMENT=Production
   Environment=ASPNETCORE_URLS=http://localhost:5000

   [Install]
   WantedBy=multi-user.target

   # Start service
   sudo systemctl enable neoron
   sudo systemctl start neoron
   ```

### 4. Post-Deployment Verification
1. Check application health endpoint:
   ```bash
   curl http://localhost:5000/health
   ```
2. Verify database migrations
3. Test authentication flow
4. Monitor logs:
   ```bash
   sudo journalctl -u neoron -f
   ```

## Monitoring Setup

### 1. Logging
1. Configure centralized logging:
   ```bash
   # Install Elasticsearch
   wget -qO - https://artifacts.elastic.co/GPG-KEY-elasticsearch | sudo gpg --dearmor -o /usr/share/keyrings/elasticsearch-keyring.gpg
   echo "deb [signed-by=/usr/share/keyrings/elasticsearch-keyring.gpg] https://artifacts.elastic.co/packages/8.x/apt stable main" | sudo tee /etc/apt/sources.list.d/elastic-8.x.list
   sudo apt update && sudo apt install elasticsearch

   # Install Kibana
   sudo apt install kibana

   # Start services
   sudo systemctl enable elasticsearch kibana
   sudo systemctl start elasticsearch kibana
   ```
2. Configure log shipping
3. Set up monitoring dashboards

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
