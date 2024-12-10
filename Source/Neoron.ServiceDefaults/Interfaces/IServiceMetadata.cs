namespace Neoron.ServiceDefaults.Interfaces
{
    /// <summary>
    /// Defines metadata about a service.
    /// </summary>
    public interface IServiceMetadata
    {
        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the version of the service.
        /// </summary>
        string Version { get; }
    }
}
