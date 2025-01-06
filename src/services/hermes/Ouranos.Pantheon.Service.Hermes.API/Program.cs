using Ouranos.Pantheon.Service.Hermes.API.Startup;

var app = WebApplication.CreateBuilder(args)
    .ConfigureBuilder()
    .ConfigureApp();

await app.RunWithGraphQLCommandsAsync(args);