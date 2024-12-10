# Coding Standards

## Code Style
- Follow Microsoft's C# coding conventions
- Use meaningful and descriptive names for variables, methods, and classes
- Keep methods focused and concise (≤20 lines)
- Document public APIs with XML comments
- Use async/await consistently for asynchronous operations

## Architecture
- Follow Clean Architecture principles
- Keep controllers thin, business logic in services
- Use dependency injection
- Follow SOLID principles
- Use repository pattern for data access

## Project Structure
- Keep related files in appropriate namespaces
- Group by feature when possible
- Maintain clear separation of concerns
- Files should not exceed 300 lines
- Classes should not exceed 200 lines

## Database
- Use Entity Framework Core best practices
- Implement proper indexing strategies
- Use appropriate data types
- Include foreign key relationships
- Implement soft delete where appropriate

## Security
- Validate all inputs
- Use HTTPS
- Implement rate limiting
- Follow OWASP security guidelines
- Keep dependencies updated

## Performance
- Use async operations for I/O
- Implement caching where appropriate
- Monitor and optimize database queries
- Consider pagination for large datasets
- Profile and benchmark critical paths
