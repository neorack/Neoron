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

## 8. Code Documentation

- Write clear, concise comments for all sections of code.
- Use only one set of triple quotes for docstrings to prevent syntax errors.
- Document the purpose and expected behavior of functions and modules.

## 9. Change Management

- Review all changes to assess their impact on other parts of the project.
- Test changes thoroughly to ensure consistency and prevent conflicts.
- Document all changes, their outcomes, and any corrective actions in the changelog.

## 10. Problem-Solving Approach

- Exhaust all options before determining an action is impossible.
- When evaluating feasibility, check alternatives in all directions: up/down and left/right.
- Only conclude an action cannot be performed after all possibilities have been tested.

## 11. Testing and Quality Assurance

- Implement comprehensive unit tests for all components.
- Perform integration testing to ensure different parts of the system work together.
- Conduct thorough end-to-end testing to validate user workflows.
- Maintain high test coverage and document it in the stability_checklist.md.

## 12. Security Best Practices

- Implement proper authentication and authorization mechanisms.
- Use secure communication protocols (HTTPS) for all network interactions.
- Sanitize and validate all user inputs to prevent injection attacks.
- Regularly update dependencies to patch known vulnerabilities.
- Follow the principle of least privilege in system design.

## 13. Performance Optimization

- Optimize database queries for efficiency.
- Implement caching strategies where appropriate.
- Minimize network requests and payload sizes.
- Use asynchronous operations for I/O-bound tasks.
- Regularly profile the application to identify and address performance bottlenecks.

## 14. Compliance and Standards

- Ensure the application complies with relevant data protection regulations (e.g., GDPR, CCPA).
- Follow accessibility standards (WCAG) to make the application usable by people with disabilities.
- Adhere to industry-standard coding conventions and style guides.

## 15. Documentation

- Maintain up-to-date API documentation.
- Provide clear, step-by-step guides for setup and deployment.
- Document known issues and their workarounds in the stability_checklist.md.
- Keep user guides and FAQs current with each feature update.

Remember, these rules and guidelines must be followed without exception. Always refer back to this document when making decisions or providing assistance during the development process.
