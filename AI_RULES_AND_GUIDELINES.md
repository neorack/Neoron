# AI Development Guidelines - Neoron Project

## Core Principles
- Reliability First: Every change must improve system stability
- Defensive Programming: Validate inputs, handle errors gracefully
- Maintainable Code: Clear structure, well-documented, easily testable
- Performance Conscious: Consider scalability and efficiency
- Security-First Mindset: Protect data and system integrity

## Critical Requirements

### 1. Version Control
- Use conventional commits (feat:, fix:, docs:, test:, refactor:)
- One logical change per commit
- Branch naming: feature/, bugfix/, hotfix/, test/
- Include issue/ticket references
- Meaningful commit messages with context
- Clean commit history (squash when needed)

### 2. Code Quality
- Methods: ≤20 lines
- Classes: ≤200 lines
- Files: ≤300 lines
- Follow SOLID principles
- Use dependency injection
- Implement interface segregation

### 3. Error Handling
- Custom exception types
- Correlation IDs for tracing
- Structured logging
- Retry mechanisms for transient failures
- Documented recovery procedures

### 4. Testing
- 90% minimum code coverage for new code
- Unit tests for business logic
- Integration tests for APIs
- Performance benchmarks
- Load testing for critical paths
- Test data management
- Mocking strategies
- Edge case coverage
- Concurrent operation testing
- Security testing scenarios

### 5. Security
- OWASP compliance
- Input validation
- Secure credential management
- Data encryption
- Authorization checks

### 6. Documentation
- XML docs for public APIs
- Usage examples
- Error scenarios
- Recovery procedures
- Deployment guides

### 7. Performance
- Optimized queries
- Caching strategy
- Async operations
- Resource monitoring
- Scalability planning

### 8. Monitoring
- Health checks
- Performance metrics
- Error tracking
- Usage analytics
- System alerts

### 9. Change Management
- Impact analysis
- Rollback procedures
- Dependency tracking
- Breaking change notices
- Migration guides

### 10. Technology Stack Management
- Maintain consistent technology choices
- Avoid redundant frameworks solving same problems
- Document and justify technology selections
- Consider maintenance and support implications
- Evaluate total cost of ownership
- Limit number of different technologies used
- Ensure team expertise exists for chosen stack
- Plan migration paths for outdated technologies

### 11. API Design
- Follow REST principles
- Consistent endpoint naming
- Proper HTTP method usage
- Comprehensive error responses
- API versioning strategy
- Rate limiting implementation
- Documentation with examples
- Schema validation
- Response formatting standards

### 12. Database Management
- Query optimization
- Index strategy
- Migration procedures
- Backup policies
- Data archival strategy
- Connection management
- Transaction handling
- Concurrency control
- Data validation rules

Remember: Prioritize system stability and reliability in all changes.
Follow these guidelines for all AI-assisted development tasks.
