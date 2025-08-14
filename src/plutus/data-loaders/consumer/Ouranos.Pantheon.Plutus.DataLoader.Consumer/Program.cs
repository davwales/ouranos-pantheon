using Ouranos.Pantheon.Plutus.DataLoader.Consumer.Startup;

var host = Host.CreateApplicationBuilder(args).ConfigureBuilder();
await host.RunAsync();