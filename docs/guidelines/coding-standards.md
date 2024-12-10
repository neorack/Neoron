# Development Standards

## Code Style and Organization
- Follow Microsoft's C# coding conventions
- Use meaningful and descriptive names
- Keep methods focused (≤20 lines)
- Keep classes concise (≤200 lines)
- Keep files manageable (≤300 lines)
- Document public APIs with XML comments
- Use async/await consistently
- Follow SOLID principles

## Project Structure
- Organize by feature
- Maintain clear separation of concerns
- Group related files in appropriate namespaces
- Use Clean Architecture principles
- Implement repository pattern
- Use dependency injection

## Database Practices
- Follow Entity Framework Core best practices
- Implement proper indexing strategies
- Use appropriate data types
- Define clear relationships
- Implement soft delete where appropriate
- Optimize query performance

## Security Requirements
- Validate all inputs
- Use HTTPS everywhere
- Implement rate limiting
- Follow OWASP guidelines
- Keep dependencies updated
- Use proper authentication
- Implement authorization

## Performance Guidelines
- Use async operations for I/O
- Implement strategic caching
- Optimize database queries
- Use pagination for large datasets
- Profile critical paths
- Monitor performance metrics

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
