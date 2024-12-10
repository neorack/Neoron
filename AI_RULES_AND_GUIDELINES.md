# AI Rules and Guidelines for the Neoron Project

This document outlines the rules, best practices, and guidelines that AI must follow when assisting with the development of the Neoron project. These guidelines ensure consistency, quality, and maintainability of the codebase.

## 1. Version Control and Documentation

### 1.1 Memlog System
- Always create/verify the 'memlog' folder when starting any project
- Required files in memlog folder:
  - tasks.log: Track ongoing and completed tasks
  - changelog.md: Document all code changes
  - stability_checklist.md: Track system stability and known issues
  - url_debug_checklist.md: Document API endpoints and their status
- Verify and update these files before any actions
- Use logs to maintain context between conversations

### 1.2 Git Practices
- Write clear, descriptive commit messages
- Follow conventional commits format (feat:, fix:, docs:, etc.)
- Keep commits atomic and focused
- Branch naming convention: feature/, bugfix/, hotfix/

## 2. Development Workflow

### 2.1 Task Management
- Break down user instructions into clear, numbered steps
- Document both actions and reasoning for each step
- Flag potential issues proactively
- Verify completion before proceeding
- Document errors and recovery steps in tasks.log

### 2.2 Code Review Process
- Review all changes against coding standards
- Verify test coverage for new code
- Check for security implications
- Ensure proper error handling
- Validate documentation updates

### 2.3 Credential Management
- Document credential purpose and usage
- Guide users through credential acquisition
- Validate credentials before use
- Use environment variables or secure vaults
- Implement token refresh mechanisms
- Never store sensitive data in code or logs

## 3. Code Quality Standards

### 3.1 Code Structure
- Maximum file size: 300 lines
- Maximum method size: 20 lines
- Maximum class size: 200 lines
- Use meaningful namespaces
- Follow SOLID principles
- Implement interface segregation
- Use dependency injection

### 3.2 Naming Conventions
- PascalCase for classes and public members
- camelCase for private members and parameters
- Use descriptive, intention-revealing names
- Prefix interfaces with 'I'
- Avoid abbreviations

### 3.3 Error Handling
- Use custom exception types
- Include stack traces and context
- Log errors with correlation IDs
- Implement retry mechanisms
- Document recovery procedures
- Use structured logging

## 4. Integration and Dependencies

### 4.1 Third-Party Services
- Verify setup requirements
- Document required permissions
- Test connections thoroughly
- Version compatibility matrix
- Implement circuit breakers
- Monitor service health
- Fallback strategies

### 4.2 Package Management
- Use explicit version numbers
- Document breaking changes
- Maintain dependency tree
- Regular security audits
- Update strategy documentation
- Lock file maintenance

## 5. Documentation Standards

### 5.1 Code Documentation
- Write clear, concise comments
- Use XML documentation tags for C# code
- Document parameters and return values
- Include usage examples
- Document exceptions and edge cases
- Keep comments up-to-date with code changes

### 5.2 Change Management
- Document impact analysis
- Update affected documentation
- Record changes in changelog.md
- Include rollback procedures
- Track dependencies affected

### 5.3 Problem-Solving Framework
- Follow systematic debugging approach
- Document attempted solutions
- Consider performance implications
- Evaluate security impacts
- Test edge cases thoroughly

## 6. Quality Assurance

### 6.1 Testing Strategy
- Minimum 80% code coverage
- Unit tests for all business logic
- Integration tests for API endpoints
- End-to-end tests for critical paths
- Performance benchmarks
- Load testing requirements

### 6.2 Security Standards
- OWASP compliance checks
- Regular security audits
- Input validation rules
- Authentication protocols
- Authorization matrix
- Data encryption requirements

## 7. Performance and Optimization

### 7.1 Performance Standards
- Response time targets
- Database query optimization
- Caching strategies
- Async operations usage
- Resource utilization limits
- Performance monitoring

### 7.2 Scalability Guidelines
- Horizontal scaling approach
- Load balancing configuration
- Database partitioning
- Microservices boundaries
- API versioning strategy

## 8. Compliance and Standards

### 8.1 Regulatory Compliance
- GDPR requirements
- CCPA compliance
- Data retention policies
- Privacy by design
- Audit trail requirements

### 8.2 Technical Standards
- REST API guidelines
- GraphQL best practices
- OpenAPI documentation
- Code style enforcement
- Accessibility (WCAG 2.1)

## 9. System Documentation

### 9.1 Technical Documentation
- Architecture diagrams
- API documentation
- Database schemas
- Deployment guides
- Troubleshooting guides

### 9.2 User Documentation
- Setup instructions
- Configuration guides
- FAQ maintenance
- Known issues log
- Feature documentation

Remember: These guidelines ensure consistency and quality across the project. Refer to this document when making decisions or providing assistance during development.
