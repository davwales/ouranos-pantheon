using Ouranos.Pantheon.DataLoader.Plutus.Consumer.Startup;

var host = Host.CreateApplicationBuilder(args).ConfigureBuilder();
await host.RunAsync();