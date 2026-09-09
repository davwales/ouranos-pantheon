using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets;

namespace Ouranos.Pantheon.Modules.Shared.Infra.Observability;

public static class ObservabilityModule
{
    private const string WolverineActivitySource = "Wolverine";
    private const string WolverineMeterName = "Wolverine*";
    private const string MartenActivitySource = "Marten";
    private const string MartenMeterName = "Marten";
    private const string TickerQActivitySource = "TickerQ";
    private const string NpgsqlMeterName = "Npgsql";
    private const string EfCoreMeterName = "Microsoft.EntityFrameworkCore";
    private const string KestrelMeterName = "Microsoft.AspNetCore.Server.Kestrel";
    private const string OtlpEndpointEnvVar = "OTEL_EXPORTER_OTLP_ENDPOINT";
    private const string HttpProtobufProtocol = "http/protobuf";
    private const string DeploymentEnvironmentNameAttribute = "deployment.environment.name";

    private static readonly List<string> ExcludedPathSegments = ["/health", "/tickerq"];

    public static IServiceCollection AddCoreObservabilityModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        Guard.Against.Null(services);
        Guard.Against.Null(configuration);
        Guard.Against.Null(environment);

        var options =
            configuration.GetSection(ObservabilityOptions.SectionName).Get<ObservabilityOptions>()
            ?? new ObservabilityOptions();

        var otelBuilder = services
            .AddOpenTelemetry()
            .ConfigureResource(resource =>
                resource
                    .AddService(options.ServiceName)
                    .AddAttributes([
                        new KeyValuePair<string, object>(
                            DeploymentEnvironmentNameAttribute,
                            environment.EnvironmentName
                        ),
                    ])
            )
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(aspNet =>
                    {
                        aspNet.RecordException = true;
                        aspNet.Filter = httpContext =>
                            !ExcludedPathSegments.Any(segment =>
                                httpContext.Request.Path.StartsWithSegments(segment)
                            );
                    })
                    .AddHttpClientInstrumentation(httpClient =>
                    {
                        httpClient.RecordException = true;
                    })
                    .AddNpgsql()
                    .AddSource(WolverineActivitySource)
                    .AddSource(MartenActivitySource)
                    .AddSource(TickerQActivitySource)
                    .AddSource(WebSocketTelemetry.ActivitySourceName)
                    .SetSampler(
                        new RootSpanExclusionSampler(
                            options.ExcludedRootSpanNames,
                            new ParentBasedSampler(
                                new TraceIdRatioBasedSampler(options.SamplingRatio)
                            )
                        )
                    );
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter(WolverineMeterName)
                    .AddMeter(MartenMeterName)
                    .AddMeter(NpgsqlMeterName)
                    .AddMeter(EfCoreMeterName)
                    .AddMeter(KestrelMeterName)
                    .AddMeter(WebSocketTelemetry.MeterName);
            });

        if (!string.IsNullOrWhiteSpace(options.OtlpEndpoint))
        {
            var endpoint = new Uri(options.OtlpEndpoint);
            var protocol = ParseProtocol(options.OtlpProtocol);

            otelBuilder
                .WithTracing(tracing =>
                    tracing.AddOtlpExporter(exporter =>
                        ConfigureOtlpExporter(exporter, endpoint, protocol)
                    )
                )
                .WithMetrics(metrics =>
                    metrics.AddOtlpExporter(exporter =>
                        ConfigureOtlpExporter(exporter, endpoint, protocol)
                    )
                );
        }
        else if (!string.IsNullOrWhiteSpace(configuration[OtlpEndpointEnvVar]))
        {
            otelBuilder
                .WithTracing(tracing => tracing.AddOtlpExporter())
                .WithMetrics(metrics => metrics.AddOtlpExporter());
        }

        return services;
    }

    private static void ConfigureOtlpExporter(
        OtlpExporterOptions exporter,
        Uri endpoint,
        OtlpExportProtocol protocol
    )
    {
        exporter.Endpoint = endpoint;
        exporter.Protocol = protocol;
    }

    internal static OtlpExportProtocol ParseProtocol(string protocol)
    {
        return string.Equals(protocol, HttpProtobufProtocol, StringComparison.OrdinalIgnoreCase)
            ? OtlpExportProtocol.HttpProtobuf
            : OtlpExportProtocol.Grpc;
    }
}
