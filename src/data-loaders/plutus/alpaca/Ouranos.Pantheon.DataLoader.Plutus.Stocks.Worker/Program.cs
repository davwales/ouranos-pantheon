using Ouranos.Pantheon.DataLoader.Plutus.Stocks.Worker.Startup;

var host = Host.CreateApplicationBuilder(args).ConfigureBuilder();
await host.RunAsync();