# Code Documentation Standards

## Overview
This document outlines the documentation standards for the Neoron project. Following these standards ensures consistency and maintainability across the codebase.

## General Principles

1. **Purpose Over Implementation**
   - Document WHY code exists, not WHAT it does
   - Implementation details should be self-evident from clean code
   - Focus on business logic and architectural decisions

2. **Clarity and Conciseness** 
   - Use clear, precise language
   - Avoid redundant or obvious comments
   - Keep comments up-to-date with code changes

3. **Documentation Levels**

   File Headers:
   ```csharp
   /*
    * Brief description of the file's purpose
    * Key responsibilities and patterns used
    * Any important architectural decisions
    * Author(s) and creation date if relevant
    */
   ```

   Class Documentation:
   ```csharp
   /// <summary>
   /// Describes the class's purpose and responsibilities
   /// </summary>
   /// <remarks>
   /// Design patterns used
   /// Threading considerations
   /// Performance characteristics
   /// </remarks>
   ```

   Method Documentation:
   ```csharp
   /// <summary>
   /// Describes what the method does, not how it does it
   /// </summary>
   /// <param name="paramName">Parameter purpose and valid values</param>
   /// <returns>Description of return value</returns>
   /// <exception cref="ExceptionType">When/why exceptions are thrown</exception>
   ```

4. **When to Comment**

   Always Comment:
   - Public APIs and interfaces
   - Complex business logic and algorithms
   - Non-obvious design decisions and architectural choices
   - Performance implications and optimizations
   - Threading/concurrency details and synchronization
   - Security considerations and validation
   - Workarounds or temporary solutions (with ticket references)
   - Database operations and transaction boundaries
   - Retry logic and error handling strategies
   - Cache invalidation strategies
   - Rate limiting implementations

   Avoid Commenting:
   - Obvious implementations
   - Self-documenting code
   - Information better expressed in code
   - Redundant information

5. **Code Examples**

   Good Comments:
   ```csharp
   // Retry logic needed because external service is occasionally unreliable
   // Exponential backoff prevents overwhelming the service
   await RetryWithBackoff(async () => await externalService.CallAsync());

   // Cache results for 1 hour since data changes infrequently
   // and we want to minimize database load
   [ResponseCache(Duration = 3600)]
   public async Task<IEnumerable<Data>> GetData()

   // Thread-safe singleton implementation using lazy initialization
   private static readonly Lazy<MyService> instance = 
       new Lazy<MyService>(() => new MyService());
   ```

   Bad Comments:
   ```csharp
   // Increment counter
   counter++;

   // Check if user exists
   if (user != null)

   // Loop through list
   foreach (var item in items)
   ```

6. **XML Documentation Tags**

   Essential Tags:
   - `<summary>` - Brief description
   - `<param>` - Parameter descriptions
   - `<returns>` - Return value description
   - `<exception>` - Documented exceptions
   - `<remarks>` - Additional details
   - `<example>` - Usage examples
   - `<see>` - Cross-references
   - `<inheritdoc>` - Inherit documentation

7. **Maintenance Guidelines**

   - Review comments during code reviews
   - Update comments when code changes
   - Remove obsolete comments
   - Convert comments to code when possible
   - Use TODO/HACK/FIXME with tracking numbers

8. **Testing Documentation**

   Test Class:
   ```csharp
   /// <summary>
   /// Tests for UserService functionality
   /// </summary>
   [TestFixture]
   public class UserServiceTests
   {
       /// <summary>
       /// Verifies user creation with valid data succeeds
       /// </summary>
       [Test]
       public async Task CreateUser_WithValidData_Succeeds()
   ```

9. **Interface Documentation**

   ```csharp
   /// <summary>
   /// Defines core user management operations
   /// </summary>
   /// <remarks>
   /// Implementations should be thread-safe
   /// All methods should validate inputs
   /// </remarks>
   public interface IUserService
   {
       /// <summary>
       /// Creates a new user account
       /// </summary>
       /// <param name="user">User details (must not be null)</param>
       /// <returns>Created user with generated ID</returns>
       /// <exception cref="ValidationException">Invalid user data</exception>
       Task<User> CreateUserAsync(User user);
   }
   ```

10. **Configuration and Constants**

    ```csharp
    /// <summary>
    /// Application-wide configuration constants
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Maximum items per page for pagination
        /// Larger values impact performance
        /// </summary>
        public const int MaxPageSize = 100;
    }
    ```

## Best Practices Summary

1. Write comments before implementing code
2. Keep comments close to relevant code
3. Use complete sentences
4. Update or remove outdated comments
5. Document assumptions and limitations
6. Include units for numeric values
7. Reference issue/ticket numbers
8. Document null handling
9. Explain regex patterns
10. Document threading implications

Remember: The best comment is often no comment - make the code self-documenting first.
