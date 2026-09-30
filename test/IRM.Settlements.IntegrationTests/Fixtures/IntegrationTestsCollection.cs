using Xunit;

namespace IRM.Settlements.IntegrationTests.Fixtures;

[CollectionDefinition(Name)]
public class IntegrationTestsCollection : ICollectionFixture<TestWebApplicationFactory>
{
    public const string Name = nameof(IntegrationTestsCollection);
}
