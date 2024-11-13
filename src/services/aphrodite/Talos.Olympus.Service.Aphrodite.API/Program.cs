using Talos.Olympus.Service.Aphrodite.API.Startup;

var app = WebApplication.CreateBuilder(args)
    .ConfigureBuilder()
    .ConfigureApp();

await app.RunWithGraphQLCommandsAsync(args);