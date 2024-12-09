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
    
    public static class Tags
    {
        public const string LongRunning = "LongRunning";
        public const string RequiresDatabase = "RequiresDatabase";
        public const string RequiresNetwork = "RequiresNetwork";
        public const string Flaky = "Flaky";
    }
}
