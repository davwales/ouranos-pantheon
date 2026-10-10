using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.API;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst;

public static class AskSymbolAnalystEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapAssistant<SymbolAnalystContext>("/api/plutus/symbols/assistant/completions/stream")
            .WithTags("Plutus.Symbols");
    }
}
