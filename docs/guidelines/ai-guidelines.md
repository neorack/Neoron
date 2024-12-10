# AI Development Guidelines - Neoron Project

## Core Principles
- Reliability First: Every change must improve system stability
- Defensive Programming: Validate inputs, handle errors gracefully
- Maintainable Code: Clear structure, well-documented, easily testable

## Critical Requirements

### 1. Version Control
- Use conventional commits (feat:, fix:, docs:)
- One logical change per commit
- Branch naming: feature/, bugfix/, hotfix/

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
- 80% minimum code coverage
- Unit tests for business logic
- Integration tests for APIs
- Performance benchmarks
- Load testing for critical paths

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

Remember: Prioritize system stability and reliability in all changes.
