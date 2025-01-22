using Microsoft.Extensions.Hosting;
using Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Extensions;

var host = Host.CreateDefaultBuilder().ConfigureBuilder();
await host.RunAsync();