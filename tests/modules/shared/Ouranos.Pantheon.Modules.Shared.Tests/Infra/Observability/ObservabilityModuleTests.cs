using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Ouranos.Pantheon.Modules.Shared.Infra.Observability;
using Ouranos.Pantheon.Tests.Utils.Shouldly;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Infra.Observability;

public sealed class ObservabilityModuleTests
{
    [Fact]
    public void AddCoreObservabilityModule_WithEndpoint_ShouldRegisterTracerProvider()
    {
        // Arrange
        var configuration = CreateConfiguration(
            (
                $"{ObservabilityOptions.SectionName}:{nameof(ObservabilityOptions.OtlpEndpoint)}",
                "http://localhost:4317"
            )
        );
        var environment = CreateEnvironment();
        var services = new ServiceCollection();

        // Act
        services.AddCoreObservabilityModule(configuration, environment);

        // Assert
        services.ShouldContainService<TracerProvider>(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddCoreObservabilityModule_WithoutEndpoint_ShouldRegisterTracerProvider()
    {
        // Arrange
        var configuration = CreateConfiguration();
        var environment = CreateEnvironment();
        var services = new ServiceCollection();

        // Act
        services.AddCoreObservabilityModule(configuration, environment);

        // Assert
        services.ShouldContainService<TracerProvider>(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddCoreObservabilityModule_WhenEndpointConfigured_ShouldResolveTracerProvider()
    {
        // Arrange
        var configuration = CreateConfiguration(
            (
                $"{ObservabilityOptions.SectionName}:{nameof(ObservabilityOptions.OtlpEndpoint)}",
                "http://localhost:4317"
            )
        );
        var environment = CreateEnvironment();
        var services = new ServiceCollection();
        services.AddCoreObservabilityModule(configuration, environment);

        // Act
        var provider = services.BuildServiceProvider();
        var tracerProvider = provider.GetService<TracerProvider>();

        // Assert
        tracerProvider.ShouldNotBeNull();
    }

    [Fact]
    public void AddCoreObservabilityModule_WhenResolvingProviderWithoutEndpoint_ShouldResolveTracerProvider()
    {
        // Arrange
        var configuration = CreateConfiguration();
        var environment = CreateEnvironment();
        var services = new ServiceCollection();
        services.AddCoreObservabilityModule(configuration, environment);

        // Act
        var provider = services.BuildServiceProvider();
        var tracerProvider = provider.GetService<TracerProvider>();

        // Assert
        tracerProvider.ShouldNotBeNull();
    }

    [Fact]
    public void AddCoreObservabilityModule_WithEndpoint_ShouldRegisterMeterProvider()
    {
        // Arrange
        var configuration = CreateConfiguration(
            (
                $"{ObservabilityOptions.SectionName}:{nameof(ObservabilityOptions.OtlpEndpoint)}",
                "http://localhost:4317"
            )
        );
        var environment = CreateEnvironment();
        var services = new ServiceCollection();

        // Act
        services.AddCoreObservabilityModule(configuration, environment);

        // Assert
        services.ShouldContainService<MeterProvider>(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddCoreObservabilityModule_WithoutEndpoint_ShouldRegisterMeterProvider()
    {
        // Arrange
        var configuration = CreateConfiguration();
        var environment = CreateEnvironment();
        var services = new ServiceCollection();

        // Act
        services.AddCoreObservabilityModule(configuration, environment);

        // Assert
        services.ShouldContainService<MeterProvider>(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddCoreObservabilityModule_WhenEndpointConfigured_ShouldResolveMeterProvider()
    {
        // Arrange
        var configuration = CreateConfiguration(
            (
                $"{ObservabilityOptions.SectionName}:{nameof(ObservabilityOptions.OtlpEndpoint)}",
                "http://localhost:4317"
            )
        );
        var environment = CreateEnvironment();
        var services = new ServiceCollection();
        services.AddCoreObservabilityModule(configuration, environment);

        // Act
        var provider = services.BuildServiceProvider();
        var meterProvider = provider.GetService<MeterProvider>();

        // Assert
        meterProvider.ShouldNotBeNull();
    }

    [Fact]
    public void AddCoreObservabilityModule_WhenHttpProtobufConfigured_ShouldResolveProviders()
    {
        // Arrange
        var configuration = CreateConfiguration(
            (
                $"{ObservabilityOptions.SectionName}:{nameof(ObservabilityOptions.OtlpEndpoint)}",
                "http://localhost:4318"
            ),
            (
                $"{ObservabilityOptions.SectionName}:{nameof(ObservabilityOptions.OtlpProtocol)}",
                "http/protobuf"
            )
        );
        var environment = CreateEnvironment();
        var services = new ServiceCollection();
        services.AddCoreObservabilityModule(configuration, environment);

        // Act
        var provider = services.BuildServiceProvider();

        // Assert
        provider.GetService<TracerProvider>().ShouldNotBeNull();
        provider.GetService<MeterProvider>().ShouldNotBeNull();
    }

    [Fact]
    public void AddCoreObservabilityModule_WhenEndpointIsWhitespace_ShouldResolveProvidersWithoutExporter()
    {
        // Arrange
        var configuration = CreateConfiguration(
            (
                $"{ObservabilityOptions.SectionName}:{nameof(ObservabilityOptions.OtlpEndpoint)}",
                "   "
            )
        );
        var environment = CreateEnvironment();
        var services = new ServiceCollection();
        services.AddCoreObservabilityModule(configuration, environment);

        // Act
        var provider = services.BuildServiceProvider();

        // Assert
        provider.GetService<TracerProvider>().ShouldNotBeNull();
        provider.GetService<MeterProvider>().ShouldNotBeNull();
    }

    [Fact]
    public void AddCoreObservabilityModule_WhenEnvVarEndpoint_ShouldResolveProviders()
    {
        // Arrange
        var configuration = CreateConfiguration(
            ("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:4317")
        );
        var environment = CreateEnvironment();
        var services = new ServiceCollection();
        services.AddCoreObservabilityModule(configuration, environment);

        // Act
        var provider = services.BuildServiceProvider();

        // Assert
        provider.GetService<TracerProvider>().ShouldNotBeNull();
        provider.GetService<MeterProvider>().ShouldNotBeNull();
    }

    [Fact]
    public void ParseProtocol_WhenHttpProtobuf_ShouldReturnHttpProtobuf()
    {
        // Act
        var protocol = ObservabilityModule.ParseProtocol("http/protobuf");

        // Assert
        protocol.ShouldBe(OtlpExportProtocol.HttpProtobuf);
    }

    [Fact]
    public void ParseProtocol_WhenGrpc_ShouldReturnGrpc()
    {
        // Act
        var protocol = ObservabilityModule.ParseProtocol("grpc");

        // Assert
        protocol.ShouldBe(OtlpExportProtocol.Grpc);
    }

    [Fact]
    public void ParseProtocol_WhenUnknown_ShouldReturnGrpc()
    {
        // Act
        var protocol = ObservabilityModule.ParseProtocol("carrier-pigeon");

        // Assert
        protocol.ShouldBe(OtlpExportProtocol.Grpc);
    }

    private static IConfiguration CreateConfiguration(params (string Key, string Value)[] values)
    {
        var entries = new Dictionary<string, string?>();
        foreach (var (key, value) in values)
        {
            entries[key] = value;
        }

        return new ConfigurationBuilder().AddInMemoryCollection(entries).Build();
    }

    private static IHostEnvironment CreateEnvironment()
    {
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns("Testing");
        return environment;
    }
}
