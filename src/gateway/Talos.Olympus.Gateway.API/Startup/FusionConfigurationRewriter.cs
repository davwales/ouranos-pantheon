using HotChocolate.Fusion.Metadata;

namespace Talos.Olympus.Gateway.API.Startup;

public class FusionConfigurationRewriter : ConfigurationRewriter
{
    private readonly ILogger<FusionConfigurationRewriter> _logger;
    private readonly Dictionary<string, string> _schemas;

    public FusionConfigurationRewriter(
        ILogger<FusionConfigurationRewriter> logger,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(configuration);

        _logger = logger;
        _schemas = configuration.GetSection("Schemas")
            .Get<Dictionary<string, string>>() ?? [];
    }

    protected override ValueTask<HttpClientConfiguration> RewriteAsync(
        HttpClientConfiguration configuration,
        CancellationToken cancellationToken
    )
    {
        if (_schemas.TryGetValue(configuration.SubgraphName, out var uri))
        {
            _logger.LogInformation("Overriding endpoint for subgraph '{subgraphName}'.", configuration.SubgraphName);
            configuration = configuration with { EndpointUri = new Uri(uri) };
        }

        return base.RewriteAsync(configuration, cancellationToken);
    }
}
