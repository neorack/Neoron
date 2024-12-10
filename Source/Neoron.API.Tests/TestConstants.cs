namespace Neoron.API.Tests;

public static class TestConstants
{
    public static class Database
    {
        public const string TestConnectionString = "Server=localhost;Database=NeoronTest;Trusted_Connection=True;TrustServerCertificate=True;";
        public const string ContainerPrefix = "neoron_test";
    }

    public static class Auth
    {
        public const string TestUserId = "test-user-id";
        public const string TestUserName = "Test User";
        public const string TestUserRole = "admin";
        public const string TestAuthScheme = "Test";
    }

    public static class Messages
    {
        public const int MaxContentLength = 2000;
        public const int MaxEmbedLength = 4000;
        public const int DefaultPageSize = 100;
    }

    public static class Performance
    {
        public const int MaxResponseTimeMs = 500;
        public const int MaxMemoryGrowthMb = 50;
        public const int MaxConcurrentRequests = 100;
        public const int BulkOperationBatchSize = 100;
        public const int PerformanceTestTimeout = 300; // seconds
    }
}
