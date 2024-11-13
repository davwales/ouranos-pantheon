using Talos.Olympus.Service.Plutus.API.Startup;

var app = WebApplication.CreateBuilder(args)
    .ConfigureBuilder()
    .ConfigureApp();

await app.RunWithGraphQLCommandsAsync(args);