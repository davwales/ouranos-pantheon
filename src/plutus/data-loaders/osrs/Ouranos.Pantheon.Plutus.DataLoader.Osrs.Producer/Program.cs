using Ouranos.Pantheon.Plutus.DataLoader.Osrs.Producer.Startup;

var host = Host.CreateApplicationBuilder(args).ConfigureBuilder();
await host.RunAsync();