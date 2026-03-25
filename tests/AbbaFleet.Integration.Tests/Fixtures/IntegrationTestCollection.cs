using Xunit;

namespace AbbaFleet.Integration.Tests.Fixtures;

[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
{
}
