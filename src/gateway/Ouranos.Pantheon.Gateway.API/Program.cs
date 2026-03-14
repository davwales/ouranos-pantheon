using Ouranos.Pantheon.Gateway.API.Startup;

var app = await WebApplication.CreateBuilder(args)
    .ConfigureBuilder()
    .ConfigureApp();

await app.RunWithGraphQLCommandsAsync(args);