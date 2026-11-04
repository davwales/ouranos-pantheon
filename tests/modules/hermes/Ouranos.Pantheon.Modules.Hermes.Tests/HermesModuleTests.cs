using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Hermes.Shared.PublicMode;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests;

public sealed class HermesModuleTests
{
    [Fact]
    public void MapEndpoints_ShouldRegisterAllEndpointsWithoutThrowing()
    {
        // Arrange
        var app = WebApplication
            .CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" })
            .Build();

        // Act
        var act = () => new HermesModule().MapEndpoints(app);

        // Assert
        act.ShouldNotThrow();
    }

    [Fact]
    public void MapEndpoints_ShouldGuardEveryWriteEndpointAgainstPublicMode()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Testing" }
        );
        builder.Services.AddSingleton(Substitute.For<IMessageBus>());
        var app = builder.Build();

        // Act
        new HermesModule().MapEndpoints(app);

        // Assert
        var endpoints = ((IEndpointRouteBuilder)app)
            .DataSources.SelectMany(d => d.Endpoints)
            .OfType<RouteEndpoint>()
            .ToList();
        var writes = endpoints
            .Where(e =>
                e.Metadata.GetMetadata<IHttpMethodMetadata>()
                    ?.HttpMethods.Any(m => !HttpMethods.IsGet(m))
                ?? false
            )
            .ToList();

        writes.ShouldNotBeEmpty();
        writes.ShouldAllBe(e => e.Metadata.GetMetadata<PublicModeWriteMetadata>() != null);
        endpoints
            .Except(writes)
            .ShouldAllBe(e => e.Metadata.GetMetadata<PublicModeWriteMetadata>() == null);
    }

    [Fact]
    public void Build_ShouldRegisterServicesWithoutThrowing()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Testing" }
        );

        // Act
        var act = () => new HermesModule().Build(builder);

        // Assert
        act.ShouldNotThrow();
    }
}
