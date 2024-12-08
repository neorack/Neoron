using Xunit;

namespace Neoron.API.Tests.Collections;

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionDefinition
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionDefinition interfaces.
}
