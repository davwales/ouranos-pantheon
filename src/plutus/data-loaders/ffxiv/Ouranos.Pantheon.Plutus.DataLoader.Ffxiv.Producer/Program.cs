using Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Producer.Startup;

var host = Host.CreateApplicationBuilder(args).ConfigureBuilder();
await host.RunAsync();