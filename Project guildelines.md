# Project Guidelines

## Code Style

- Follow Microsoft's C# coding conventions
- Use meaningful and descriptive names for variables, methods, and classes
- Keep methods focused and concise
- Document public APIs with XML comments
- Use async/await consistently for asynchronous operations

## Architecture

- Follow Clean Architecture principles
- Keep controllers thin, business logic in services
- Use dependency injection
- Follow SOLID principles
- Use repository pattern for data access

## Testing

- Write unit tests for business logic
- Write integration tests for repositories and APIs
- Use meaningful test names that describe the scenario
- Follow Arrange-Act-Assert pattern
- Mock external dependencies

## Git Workflow

- Use feature branches
- Write meaningful commit messages
- Keep commits focused and atomic
- Squash commits before merging
- Use pull requests for code review

## API Design

- Follow REST principles
- Use proper HTTP methods and status codes
- Version APIs appropriately
- Validate inputs
- Handle errors consistently

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

## Documentation

- Keep README up to date
- Document setup and deployment steps
- Document API endpoints
- Include examples where helpful
- Document configuration options
