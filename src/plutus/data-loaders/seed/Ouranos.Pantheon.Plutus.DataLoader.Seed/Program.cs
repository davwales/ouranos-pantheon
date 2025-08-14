using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Plutus.DataLoader.Seed.Extensions;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;
using Ouranos.Pantheon.Plutus.Service.Domain.Trades;

// Configuration
const bool clearDatabase = true;
const int numberOfSymbols = 50;
const int numberOfTradesPerSymbol = 1000;
const string marketName = "Test Market";
const string marketDescription = "A test market for seeding data.";
const double startingPrice = 100.0;
const double startingVolume = 1000.0;
const double priceVolatility = 0.5;
const double volumeVolatility = 5.0;
var tradeInterval = TimeSpan.FromMinutes(5);

var services = StartupExtensions.GetServices();
var marketRepository = services.GetRequiredService<IRepository<Market>>();
var symbolRepository = services.GetRequiredService<IRepository<Symbol>>();
var tradeRepository = services.GetRequiredService<IRepository<Trade>>();

if (clearDatabase)
{
    Console.WriteLine("Clearing the database...");
    await tradeRepository.Delete(_ => true);
    await symbolRepository.Delete(_ => true);
    await marketRepository.Delete(_ => true);
    await tradeRepository.SaveChanges();
    await symbolRepository.SaveChanges();
    await marketRepository.SaveChanges();
    Console.WriteLine("Database cleared.");
}

// Create Market
Console.WriteLine("Creating market...");
var market = new Market(
    marketRepository.CreateId(),
    marketName,
    new Taxes(new FlatTax(0.01m, 100m, 0.05m)),
    true,
    marketDescription
);
await marketRepository.Create(market);
await marketRepository.SaveChanges();
Console.WriteLine("Market created.");

// Create Symbols
Console.WriteLine("Creating symbols...");
var symbols = new List<Symbol>();
for (var i = 0; i < numberOfSymbols; i++)
{
    var symbol = new Symbol(
        symbolRepository.CreateId(),
        $"SYM{i:D3}",
        null,
        $"Symbol {i}",
        market.Id,
        new AdditionalFields()
    );
    symbols.Add(symbol);
}

await symbolRepository.CreateMany(symbols);
await symbolRepository.SaveChanges();
Console.WriteLine("Symbols created.");

// Create Trades
Console.WriteLine("Creating trades...");
var random = new Random();
var trades = new List<Trade>();
foreach (var symbol in symbols)
{
    var currentPrice = startingPrice;
    var currentVolume = startingVolume;
    var currentTimestamp = DateTimeOffset.UtcNow;

    for (var i = 0; i < numberOfTradesPerSymbol; i++)
    {
        currentPrice += (random.NextDouble() * 2 - 1) * priceVolatility;
        currentVolume += (random.NextDouble() * 2 - 1) * volumeVolatility;

        if (currentPrice < 0)
        {
            currentPrice = 0;
        }

        if (currentVolume < 0)
        {
            currentVolume = 0;
        }

        var trade = new Trade(
            tradeRepository.CreateId(),
            symbol.Id,
            (decimal)currentPrice,
            (decimal)currentVolume,
            currentTimestamp
        )
        {
            Symbol = symbol
        };
        trades.Add(trade);

        currentTimestamp = currentTimestamp.Add(tradeInterval);
    }
}

await tradeRepository.CreateMany(trades);
await tradeRepository.SaveChanges();

Console.WriteLine("Trades created.");
Console.WriteLine("Database seeding complete.");