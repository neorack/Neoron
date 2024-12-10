using Xunit;

namespace Neoron.API.Tests.Fixtures;

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}

[CollectionDefinition("Integration")] 
public class IntegrationCollection : ICollectionFixture<TestWebApplicationFactory<Program>>
{
}
