using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Core.Infra.Mongo.Tests;

public sealed class MongoDatabaseManagerTests
{
    private readonly IMongoClient _mongoClient = Substitute.For<IMongoClient>();

    [Fact]
    public void GetDatabase_WhenGivenAssemblyMapping_ShouldReturnDatabase()
    {
        // Arrange
        const string databaseName = "TestDatabase";

        var expectedDatabase = Substitute.For<IMongoDatabase>();
        var mongoDatabaseManager = GivenDatabaseManagerWithOptions(new MongoOptions(
            "mongodb://localhost:27017",
            new Dictionary<string, string>
            {
                {
                    typeof(TestEntity).Assembly.GetName().Name ?? string.Empty,
                    databaseName
                }
            },
            []
        ));

        _mongoClient.GetDatabase(databaseName).Returns(expectedDatabase);

        // Act
        var actualDatabase = mongoDatabaseManager.GetDatabase<TestEntity>();

        // Assert
        actualDatabase.ShouldBe(expectedDatabase);
    }

    [Fact]
    public void GetDatabase_WhenGivenTypeMapping_ShouldReturnDatabase()
    {
        // Arrange
        const string databaseName = "TestDatabase";
        var expectedDatabase = Substitute.For<IMongoDatabase>();
        var mongoDatabaseManager = GivenDatabaseManagerWithOptions(new MongoOptions(
            "mongodb://localhost:27017",
            [],
            new Dictionary<string, string>
            {
                {
                    typeof(TestEntity).FullName ?? string.Empty,
                    databaseName
                }
            }
        ));

        _mongoClient.GetDatabase(databaseName).Returns(expectedDatabase);

        // Act
        var actualDatabase = mongoDatabaseManager.GetDatabase<TestEntity>();

        // Assert
        actualDatabase.ShouldBe(expectedDatabase);
    }

    [Fact]
    public void GetDatabase_WhenGivenNoMapping_ShouldReturnDefaultDatabase()
    {
        // Arrange
        var expectedDatabase = Substitute.For<IMongoDatabase>();
        var mongoDatabaseManager = GivenDatabaseManagerWithOptions(new MongoOptions(
            "mongodb://localhost:27017",
            [],
            []
        ));

        _mongoClient.GetDatabase("ouranos").Returns(expectedDatabase);

        // Act
        var actualDatabase = mongoDatabaseManager.GetDatabase<TestEntity>();

        // Assert
        actualDatabase.ShouldBe(expectedDatabase);
    }

    [Fact]
    public void GetDatabase_WhenCalledMultipleTimes_ShouldCreateOneDatabaseReference()
    {
        // Arrange
        var expectedDatabase = Substitute.For<IMongoDatabase>();
        var mongoDatabaseManager = GivenDatabaseManagerWithOptions(new MongoOptions(
            "mongodb://localhost:27017",
            [],
            []
        ));

        _mongoClient.GetDatabase(Arg.Any<string>()).Returns(expectedDatabase);

        // Act
        _ = mongoDatabaseManager.GetDatabase<TestEntity>();
        _ = mongoDatabaseManager.GetDatabase<TestEntity>();

        // Assert
        _mongoClient.GetDatabase(Arg.Any<string>()).Received(1);
    }

    private MongoDatabaseManager GivenDatabaseManagerWithOptions(MongoOptions options)
    {
        return new MongoDatabaseManager(_mongoClient, Options.Create(options));
    }
}