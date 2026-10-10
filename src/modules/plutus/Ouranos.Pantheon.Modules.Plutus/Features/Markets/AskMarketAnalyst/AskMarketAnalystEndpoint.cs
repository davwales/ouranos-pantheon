using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.API;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst;

public static class AskMarketAnalystEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapAssistant<MarketAnalystContext>("/api/plutus/markets/assistant/completions/stream")
            .WithTags("Plutus.Markets");
    }
}
