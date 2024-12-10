using Neoron.API.Interfaces;

namespace Neoron.API.Services
{
    public class ServiceMetadata : IServiceMetadata
    {
        public string Name => "Neoron.API";
        public string Version => GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0";
    }
}
