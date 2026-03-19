using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Ouranos.Pantheon.Modules.Shared;

public interface IPantheonModule
{
    IHostApplicationBuilder Build(IHostApplicationBuilder builder);

    Task<IHost> Configure(IHost host);

    void MapEndpoints(WebApplication app)
    {
    }
}
