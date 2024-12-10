namespace Neoron.API.Tests;

public static class TestCategories
{
    public const string Integration = "Integration";
    public const string Unit = "Unit";
    public const string Performance = "Performance";
    public const string Security = "Security";
    public const string Database = "Database";
    public const string API = "API";
    public const string Authentication = "Authentication";
    public const string Authorization = "Authorization";
    public const string Controllers = "Controllers";
    public const string Services = "Services";
    public const string Repositories = "Repositories";
    public const string Middleware = "Middleware";
    public const string Validation = "Validation";
    public const string ErrorHandling = "ErrorHandling";
    
    public static class Tags
    {
        public const string LongRunning = "LongRunning";
        public const string RequiresDatabase = "RequiresDatabase";
        public const string RequiresNetwork = "RequiresNetwork";
        public const string Flaky = "Flaky";
        public const string CriticalPath = "CriticalPath";
        public const string DataAccess = "DataAccess";
        public const string ExternalDependency = "ExternalDependency";
        public const string Configuration = "Configuration";
    }
}
