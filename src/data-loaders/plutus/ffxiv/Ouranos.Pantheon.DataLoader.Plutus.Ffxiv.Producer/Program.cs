using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Producer.Startup;

var host = Host.CreateApplicationBuilder(args).ConfigureBuilder();
await host.RunAsync();