using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Plutus.DataLoader.Seed.Extensions;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Common;
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
var unitOfWork = services.GetRequiredService<IPlutusUnitOfWork>();

if (clearDatabase)
{
    Console.WriteLine("Clearing the database...");
    await unitOfWork.Trades.Delete(_ => true);
    await unitOfWork.Symbols.Delete(_ => true);
    await unitOfWork.Markets.Delete(_ => true);
    Console.WriteLine("Database cleared.");
}

// Create Market
Console.WriteLine("Creating market...");
var market = new Market(
    unitOfWork.Markets.CreateId(),
    marketName,
    new Taxes(new FlatTax(0.01m, 100m, 0.05m)),
    true,
    marketDescription
);

await unitOfWork.Markets.Create(market);
Console.WriteLine("Market created.");

// Create Symbols
Console.WriteLine("Creating symbols...");
var symbols = new List<Symbol>();
for (var i = 0; i < numberOfSymbols; i++)
{
    var symbol = new Symbol(
        unitOfWork.Symbols.CreateId(),
        $"SYM{i:D3}",
        null,
        $"Symbol {i}",
        market.Id,
        new AdditionalFields()
    );
    symbols.Add(symbol);
}

await unitOfWork.Symbols.CreateMany(symbols);
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
            unitOfWork.Trades.CreateId(),
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

await unitOfWork.Trades.CreateMany(trades);

Console.WriteLine("Trades created.");

Console.WriteLine("Saving changes to the database...");
await unitOfWork.SaveChanges();
Console.WriteLine("Database seeding complete.");