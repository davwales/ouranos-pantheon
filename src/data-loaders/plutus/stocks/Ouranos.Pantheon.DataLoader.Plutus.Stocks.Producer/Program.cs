using Ouranos.Pantheon.DataLoader.Plutus.Stocks.Producer.Startup;

var host = Host.CreateApplicationBuilder(args).ConfigureBuilder();
await host.RunAsync();