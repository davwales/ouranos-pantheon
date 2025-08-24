using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;

namespace Ouranos.Pantheon.Plutus.DataLoader.Migration.Migrators;

public class MarketMigrator
{
    public Dictionary<Id<Market>, Id<Market>> Migrate()
    {
        // Markets were migrated as static data previously.
        return new Dictionary<Id<Market>, Id<Market>>
        {
            {
                new Id<Market>("65678cc3a579e897dee76113"), new Id<Market>("d71d7207-e30b-404f-8797-0148ad88cf9e")
            }, // OSRS
            {
                new Id<Market>("65565d09d4f9e2fd3aefe674"), new Id<Market>("411b954f-5834-462e-9887-26d3ad76c924")
            }, // FFXIV
            {
                new Id<Market>("65650c286ae59a057449b04c"), new Id<Market>("daebf0a1-b54d-44f4-9c21-6654c505169a")
            } // Stock Market
        };
    }
}
