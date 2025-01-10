using MongoDB.Bson;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;

namespace Ouranos.Pantheon.Service.Plutus.Infra.Mongo.Extensions;

public static class TaxesExtensions
{
    public static List<BsonDocument> ToAggregateStages(this Taxes taxes, string salePriceField = "$max_price")
    {
        if (taxes is null || taxes.Flat is null)
        {
            return [new BsonDocument("$addFields", new BsonDocument("tax", 0.0))];
        }

        var appliedTaxes = new List<string>();
        var calculatedTaxes = new BsonDocument();

        if (taxes.Flat is not null)
        {
            appliedTaxes.Add("$flat_tax");
            var flatTax = CalculateFlatTax(taxes.Flat, salePriceField);
            calculatedTaxes.Merge(flatTax, true);
        }

        if (appliedTaxes.Count == 0)
        {
            return [new BsonDocument("$addFields", new BsonDocument("tax", 0.0))];
        }

        return
        [
            new BsonDocument("$addFields", calculatedTaxes),
            new BsonDocument("$addFields",
                new BsonDocument("tax", new BsonDocument("$add", new BsonArray(appliedTaxes))))
        ];
    }

    private static BsonDocument CalculateFlatTax(FlatTax flatTax, string salePriceField)
    {
        return new BsonDocument("flat_tax", new BsonDocument("$cond", new BsonArray
        {
            new BsonDocument("$gte", new BsonArray { salePriceField, flatTax.Minimum }),
            new BsonDocument("$multiply", new BsonArray { salePriceField, flatTax.Rate }),
            0.0
        }));
    }
}