using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Shared.Contract;

public interface IPantheonModule
{
    IHostApplicationBuilder Build(IHostApplicationBuilder builder);

    Task<IHost> Configure(IHost host);

    void MapEndpoints(WebApplication app) { }

    void ConfigureWolverine(WolverineOptions opts, IConfiguration configuration) { }
}
