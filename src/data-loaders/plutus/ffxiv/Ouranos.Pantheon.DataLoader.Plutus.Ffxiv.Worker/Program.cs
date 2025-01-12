using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Worker.Startup;

var host = Host.CreateApplicationBuilder(args).ConfigureBuilder();
await host.RunAsync();