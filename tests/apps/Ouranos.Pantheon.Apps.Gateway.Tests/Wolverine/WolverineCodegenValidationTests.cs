using JasperFx;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Ouranos.Pantheon.Apps.Gateway.Startup;

namespace Ouranos.Pantheon.Apps.Gateway.Tests.Wolverine;

public sealed class WolverineCodegenValidationTests
{
    private static WebApplication BuildHost()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Ouranos:Postgres:Host"] = "localhost",
                ["Ouranos:Postgres:Database"] = "dummy",
                ["Ouranos:Postgres:Username"] = "dummy",
                ["Ouranos:Postgres:Password"] = "dummy",
                ["Ouranos:OuranosMl:ConnectionString"] = "http://localhost",
                ["Ouranos:OuranosMl:ApiKey"] = "dummy",
            }
        );

        return HostingExtensions.ConfigureBuilder(builder);
    }

    [Fact]
    public async Task Codegen_Test_Should_Pass_For_All_Discovered_Handlers()
    {
        // Arrange
        await using var app = BuildHost();

        // Act
        var exitCode = await app.RunJasperFxCommands(["codegen", "test"]);

        // Assert
        exitCode.ShouldBe(0);
    }
}
