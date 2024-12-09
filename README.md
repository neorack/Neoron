# Neoron

Discord message processing and storage API built with .NET 8.

## Prerequisites
- .NET 8.0 SDK
- PostgreSQL 15 or later
- Docker Desktop (for development)

## Getting Started

### Development Setup
1. Clone the repository
```bash
git clone https://github.com/yourusername/neoron.git
cd neoron
```

2. Run the database
```bash
docker-compose up -d
```

3. Run the application
```bash
dotnet run --project Source/Neoron.API
```

### Running Tests
```bash
dotnet test --settings Source/Neoron.API.Tests/test.runsettings
```

## Documentation
- [Setup Guide](Documentation/Setup-Guide.md)
- [API Documentation](Documentation/API.md)
- [Implementation Checklist](Documentation/Implementation-Checklist.md)

## Contributing
Please read our [Contributing Guidelines](CONTRIBUTING.md) before submitting changes.

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
