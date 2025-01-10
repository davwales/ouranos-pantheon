using Ouranos.Pantheon.DataLoader.Plutus.Osrs.Worker.Startup;

var host = Host.CreateApplicationBuilder(args).ConfigureBuilder();
await host.RunAsync();