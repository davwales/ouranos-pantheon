using Talos.Olympus.Gateway.API.Startup;

var app = WebApplication.CreateBuilder(args)
    .ConfigureBuilder()
    .ConfigureApp();

await app.RunWithGraphQLCommandsAsync(args);
