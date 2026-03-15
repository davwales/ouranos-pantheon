using Ouranos.Pantheon.Apps.Gateway.Startup;

var app = await WebApplication.CreateBuilder(args)
    .ConfigureBuilder()
    .ConfigureApp();

await app.RunWithGraphQLCommandsAsync(args);