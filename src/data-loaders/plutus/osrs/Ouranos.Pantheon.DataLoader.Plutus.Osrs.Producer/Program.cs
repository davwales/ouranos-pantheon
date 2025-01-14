using Ouranos.Pantheon.DataLoader.Plutus.Osrs.Producer.Startup;

var host = Host.CreateApplicationBuilder(args).ConfigureBuilder();
await host.RunAsync();