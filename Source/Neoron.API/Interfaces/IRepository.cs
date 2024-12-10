namespace Neoron.API.Interfaces
{
    /// <summary>
    /// Generic repository interface for data access operations.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public interface IRepository<T>
        where T : class
    {
        /// <summary>
        /// Gets all entities asynchronously.
        /// </summary>
        /// <returns>A collection of all entities.</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Gets an entity by its identifier asynchronously.
        /// </summary>
        /// <param name="id">The identifier of the entity.</param>
        /// <returns>The entity if found; otherwise, null.</returns>
        Task<T?> GetByIdAsync(object id);

        /// <summary>
        /// Adds a new entity asynchronously.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>The added entity.</returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Updates an existing entity asynchronously.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Deletes an entity by its identifier asynchronously.
        /// </summary>
        /// <param name="id">The identifier of the entity to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteAsync(object id);
    }
}
