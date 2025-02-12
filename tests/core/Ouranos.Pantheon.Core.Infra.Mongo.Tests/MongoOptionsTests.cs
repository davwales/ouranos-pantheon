namespace Ouranos.Pantheon.Core.Infra.Mongo.Tests;

public sealed class MongoOptionsTests
{
    [Fact]
    public void SectionName_ShouldMatchExpectedValue()
    {
        // Assert
        MongoOptions.SectionName.ShouldBe("Ouranos:Mongo");
    }

    [Fact]
    public void Constructor_ShouldSetExpectedValues()
    {
        // Arrange
        var fixture = new Fixture();
        var expectedConnectionString = fixture.Create<string>();
        var expectedAssemblyDatabases = fixture.Create<Dictionary<string, string>>();
        var expectedTypeDatabases = fixture.Create<Dictionary<string, string>>();

        // Act
        var options = new MongoOptions(
            expectedConnectionString,
            expectedAssemblyDatabases,
            expectedTypeDatabases
        );

        // Assert
        options.ConnectionString.ShouldBe(expectedConnectionString);
        options.AssemblyDatabases.ShouldBe(expectedAssemblyDatabases);
        options.TypeDatabases.ShouldBe(expectedTypeDatabases);
    }
}