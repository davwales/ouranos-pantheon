namespace Ouranos.Pantheon.Modules.Shared.Infra.Observability;

public sealed record ObservabilityOptions(
    string ServiceName,
    string OtlpEndpoint,
    string OtlpProtocol,
    double SamplingRatio,
    string[] ExcludedRootSpanNames
)
{
    public const string SectionName = "Ouranos:Observability";

    public ObservabilityOptions()
        : this(
            ServiceName: "ouranos-pantheon-gateway",
            OtlpEndpoint: string.Empty,
            OtlpProtocol: "grpc",
            SamplingRatio: 1.0,
            ExcludedRootSpanNames:
            [
                "postgresql",
                "CONNECT *",
                "wolverine_node_assignments",
                "rabbitmq connect",
            ]
        ) { }
}
