using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Plutus.DataLoader.Migration;
using Ouranos.Pantheon.Plutus.DataLoader.Migration.Extensions;

var cts = new CancellationTokenSource();
var provider = StartupExtensions.GetServices();

var migration = provider.GetRequiredService<IMigration>();
await migration.Migrate(cts.Token);