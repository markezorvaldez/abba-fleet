using Xunit;

namespace AbbaFleet.IntegrationTests.Fixtures;

[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture> { }
