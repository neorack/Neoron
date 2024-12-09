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
# Project Guidelines and Development Tasks

## Project Overview
Neoron is a .NET-based system for:
- Discord message processing and storage
- User relationship and ideology tracking
- Group and activity management
- Influence metrics calculation

## Architecture
- Clean layered architecture with repository pattern
- SQL Server database with EF Core
- OpenTelemetry for observability
- Azure AD authentication
- Rate limiting and validation

## Current Status (December 2024)
- Core database schema implemented
- Basic Discord message handling
- Initial test infrastructure
- Basic rate limiting

## Immediate Tasks (High Priority)
1. Controller Testing
   - Complete MessageController test coverage
   - Add authentication test scenarios
   - Implement performance tests
   - Add concurrency tests

2. Security Implementation
   - Complete Azure AD integration
   - Add role-based authorization
   - Implement API key validation
   - Add input sanitization

3. Message Processing
   - Add message search capabilities
   - Implement thread handling
   - Add message history tracking
   - Enhance validation rules

## Medium Priority Tasks
1. Infrastructure
   - Set up Redis caching
   - Configure monitoring dashboards
   - Add custom metrics
   - Implement alert rules

2. User Management
   - Implement user profile endpoints
   - Add relationship management
   - Add ideology tracking
   - Implement influence calculations

3. Documentation
   - API documentation
   - Deployment guide
   - Database migration guide
   - Security documentation

## Low Priority Tasks
1. Administrative Features
   - Admin dashboard
   - User management interface
   - System configuration UI
   - Analytics dashboard

2. Integration Features
   - Webhook support
   - External API integrations
   - Batch processing
   - Export capabilities

## Development Standards
1. Code Quality
   - All new code must have tests
   - Maintain >80% test coverage
   - Follow StyleCop rules
   - Use static analysis

2. Security
   - Input validation on all endpoints
   - Proper error handling
   - Rate limiting
   - Audit logging

3. Performance
   - Response time <200ms
   - Efficient database queries
   - Proper caching
   - Regular performance testing

## Success Criteria
- All critical paths tested
- Security compliance verified
- Performance targets met
- Documentation complete

## Next Steps
1. Complete controller test implementation
2. Finish security integration
3. Add message search functionality
4. Implement caching
# Project Guidelines

## Code Organization

### Architecture
- Follow clean layered architecture principles
- Use repository pattern for data access
- Implement dependency injection
- Follow SOLID principles

### Project Structure
- Keep related files in appropriate namespaces
- Group by feature when possible
- Maintain clear separation of concerns

## Coding Standards

### General
- Use C# latest language features appropriately
- Follow Microsoft's C# coding conventions
- Use async/await consistently
- Implement proper exception handling
- Use cancellation tokens for async operations

### Documentation
- Provide XML documentation for public APIs
- Keep documentation up-to-date with changes
- Include examples in complex scenarios
- Document non-obvious implementation details

### Testing
- Write unit tests for business logic
- Create integration tests for data access
- Test error scenarios
- Use meaningful test names
- Follow Arrange-Act-Assert pattern

## Database
- Use Entity Framework Core best practices
- Implement proper indexing strategies
- Use appropriate data types
- Include foreign key relationships
- Implement soft delete where appropriate

## Security
- Validate all input data
- Implement proper authentication/authorization
- Use parameterized queries
- Implement rate limiting
- Follow OWASP security guidelines

## Performance
- Use async operations appropriately
- Implement caching where beneficial
- Use pagination for large datasets
- Optimize database queries
- Monitor and log performance metrics

## Source Control
- Write meaningful commit messages
- Keep commits focused and atomic
- Use feature branches
- Review code before merging
- Keep main branch stable

## Deployment
- Use continuous integration
- Implement automated testing
- Follow semantic versioning
- Document deployment procedures
- Maintain deployment scripts

## Monitoring
- Implement structured logging
- Set up appropriate alerting
- Monitor application health
- Track key metrics
- Use proper logging levels

## Review Process
- Conduct code reviews
- Use pull requests
- Check for security issues
- Verify test coverage
- Ensure documentation is updated
