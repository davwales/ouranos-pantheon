using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Infra.Postgres;

public sealed class PostgresOptionsTests
{
    [Fact]
    public void GetConnectionString_WhenBasicOptions_ShouldContainHostAndDatabase()
    {
        // Arrange
        var options = new PostgresOptions(
            Host: "localhost",
            Port: 5432,
            Database: "testdb",
            Username: "user",
            Password: "pass",
            SearchPath: null,
            IncludeErrorDetail: false,
            CommandTimeout: 30,
            MaxRetries: 3,
            MaxRetryDelaySeconds: 5,
            EnableSensitiveDataLogging: false
        );

        // Act
        var cs = options.GetConnectionString();

        // Assert
        cs.ShouldContain("Host=localhost");
        cs.ShouldContain("Database=testdb");
        cs.ShouldContain("Username=user");
        cs.ShouldContain("Command Timeout=30");
        cs.ShouldNotContain("Search Path");
        cs.ShouldNotContain("Include Error Detail");
    }

    [Fact]
    public void GetConnectionString_WhenSearchPathSet_ShouldIncludeSearchPath()
    {
        // Arrange
        var options = new PostgresOptions(
            Host: "localhost",
            Port: 5432,
            Database: "testdb",
            Username: "user",
            Password: "pass",
            SearchPath: "public",
            IncludeErrorDetail: false,
            CommandTimeout: 30,
            MaxRetries: 3,
            MaxRetryDelaySeconds: 5,
            EnableSensitiveDataLogging: false
        );

        // Act
        var cs = options.GetConnectionString();

        // Assert
        cs.ShouldContain("Search Path=public");
    }

    [Fact]
    public void GetConnectionString_WhenIncludeErrorDetailTrue_ShouldIncludeIt()
    {
        // Arrange
        var options = new PostgresOptions(
            Host: "localhost",
            Port: 5432,
            Database: "testdb",
            Username: "user",
            Password: "pass",
            SearchPath: null,
            IncludeErrorDetail: true,
            CommandTimeout: 30,
            MaxRetries: 3,
            MaxRetryDelaySeconds: 5,
            EnableSensitiveDataLogging: false
        );

        // Act
        var cs = options.GetConnectionString();

        // Assert
        cs.ShouldContain("Include Error Detail=true");
    }

    [Fact]
    public void DefaultConstructor_ShouldHaveDefaultValues()
    {
        // Act
        var options = new PostgresOptions();

        // Assert
        options.Port.ShouldBe(5432);
        options.CommandTimeout.ShouldBe(30);
        options.MaxRetries.ShouldBe(3);
    }
}
