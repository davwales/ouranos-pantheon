using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.Domain.Common;
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

// Create Market
Console.WriteLine("Creating market...");

var marketId = new Id<Market>("a653e3b4-f7e8-4b16-a215-8c44b1341871");

if (clearDatabase)
{
    await unitOfWork.Trades.Delete(t => t.Symbol.MarketId == marketId);
    await unitOfWork.Symbols.Delete(s => s.MarketId == marketId);
}

var market = await unitOfWork.Markets.FirstOrDefault(m => m.Id == marketId);
if (market is null)
{
    market = Market.Create(
        new Id<Market>("a653e3b4-f7e8-4b16-a215-8c44b1341871"),
        marketName,
        new Taxes(new FlatTax(0.01m, 100m, 0.05m)),
        true,
        marketDescription
    );

    await unitOfWork.Markets.Create(market);
    Console.WriteLine("Market created.");
}

// Create Symbols
Console.WriteLine("Creating symbols...");
var symbols = new List<Symbol>();
for (var i = 0; i < numberOfSymbols; i++)
{
    var symbol = Symbol.Create(
        unitOfWork.Symbols.CreateId(),
        $"SYM{i:D3}",
        null,
        $"Symbol {i}",
        market,
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

        var trade = Trade.Create(
            unitOfWork.Trades.CreateId(),
            symbol,
            (decimal)currentPrice,
            (decimal)currentVolume,
            currentTimestamp
        );

        trades.Add(trade);

        currentTimestamp = currentTimestamp.Add(tradeInterval);
    }
}

await unitOfWork.Trades.CreateMany(trades);

Console.WriteLine("Trades created.");

Console.WriteLine("Saving changes to the database...");
await unitOfWork.SaveChanges();
Console.WriteLine("Database seeding complete.");