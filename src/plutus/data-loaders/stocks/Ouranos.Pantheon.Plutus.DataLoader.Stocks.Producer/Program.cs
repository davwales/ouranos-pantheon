using Ouranos.Pantheon.Plutus.DataLoader.Stocks.Producer.Startup;

var host = Host.CreateApplicationBuilder(args).ConfigureBuilder();
await host.RunAsync();