using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.DataLoader.Plutus.Migration;
using Ouranos.Pantheon.DataLoader.Plutus.Migration.Extensions;

var cts = new CancellationTokenSource();
var provider = StartupExtensions.GetServices();

var migration = provider.GetRequiredService<IMigration>();
await migration.Migrate(cts.Token);