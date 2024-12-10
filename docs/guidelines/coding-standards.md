# Development Standards

## Core Principles
- Reliability First: Every change must improve system stability
- Defensive Programming: Validate inputs, handle errors gracefully
- Maintainable Code: Clear structure, well-documented, easily testable

## Code Style and Organization
- Follow Microsoft's C# coding conventions
- Use meaningful and descriptive names
- Keep methods focused (≤20 lines)
- Keep classes concise (≤200 lines)
- Keep files manageable (≤300 lines)
- Document public APIs with XML comments
- Use async/await consistently
- Follow SOLID principles

## Architecture
- Follow Clean Architecture principles
- Keep controllers thin, business logic in services
- Use dependency injection
- Implement repository pattern
- Group by feature when possible
- Maintain clear separation of concerns

## Database Guidelines
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
