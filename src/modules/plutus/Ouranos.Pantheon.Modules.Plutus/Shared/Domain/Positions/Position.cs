using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;

public class Position : BaseEntity<Id<Position>>
{
    protected Position(Id<Position> id)
        : base(id)
    {
        Status = PositionStatus.Pending;
    }

    public PositionSide Side { get; init; }

    public PositionStatus Status { get; private set; }

    public Id<Market> MarketId { get; init; }

    public Id<Symbol> SymbolId { get; init; }

    private Symbol? _symbol;
    public Symbol Symbol => _symbol ?? throw new NavigationPropertyNotLoadedException<Position>();

    public decimal Cost { get; private set; }

    public decimal Quantity { get; private set; }

    public Id<Position>? LinkedBuyPositionId { get; private set; }

    private Position? _linkedBuyPosition;

    public Position? LinkedBuyPosition =>
        LinkedBuyPositionId is null
            ? null
            : _linkedBuyPosition ?? throw new NavigationPropertyNotLoadedException<Position>();

    public Id<Strategy>? StrategyId { get; init; }

    public string? Notes { get; private set; }

    public static Position Create(
        PositionSide side,
        Id<Market> marketId,
        Id<Symbol> symbolId,
        decimal cost,
        decimal quantity,
        Id<Strategy>? strategyId = null,
        string? notes = null,
        Position? linkedBuyPosition = null,
        Symbol? symbol = null
    )
    {
        Guard.Against.NegativeOrZero(cost);
        Guard.Against.NegativeOrZero(quantity);

        if (linkedBuyPosition is not null)
        {
            Guard.Against.InvalidInput(
                linkedBuyPosition,
                nameof(linkedBuyPosition),
                p => p.CanBeLinkedAsTarget(),
                "Linked buy position must be a Bought Buy position."
            );
        }

        if (symbol is not null)
        {
            Guard.Against.InvalidInput(symbol, nameof(symbol), s => s.Id == symbolId);
        }

        return new Position(DatabaseExtensions.CreateId<Position>())
        {
            Side = side,
            MarketId = marketId,
            SymbolId = symbolId,
            Cost = cost,
            Quantity = quantity,
            StrategyId = strategyId,
            Notes = notes,
            LinkedBuyPositionId = linkedBuyPosition?.Id,
            _linkedBuyPosition = linkedBuyPosition,
            _symbol = symbol,
        };
    }

    public void Modify(decimal cost, decimal quantity, string? notes)
    {
        if (Status != PositionStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Cannot modify a position with status '{Status}'."
            );
        }

        Guard.Against.NegativeOrZero(cost);
        Guard.Against.NegativeOrZero(quantity);

        Cost = cost;
        Quantity = quantity;
        Notes = notes;

        Update();
    }

    public void Close(PositionStatus closeStatus)
    {
        if (!CanCloseWith(closeStatus))
        {
            throw new InvalidOperationException(
                $"Cannot close a {Side} position with status '{closeStatus}'."
            );
        }

        if (Status != PositionStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot close a position with status '{Status}'.");
        }

        Status = closeStatus;
        Update();
    }

    public bool CanCloseWith(PositionStatus closeStatus)
    {
        return Side switch
        {
            PositionSide.Buy => closeStatus is PositionStatus.DidNotBuy or PositionStatus.Bought,
            PositionSide.Sell => closeStatus is PositionStatus.DidNotSell or PositionStatus.Sold,
            _ => false,
        };
    }

    public bool CanBeLinkedAsTarget()
    {
        return Side == PositionSide.Buy && Status == PositionStatus.Bought;
    }

    public void LinkPosition(Id<Position> buyPositionId)
    {
        if (Side != PositionSide.Sell)
        {
            throw new InvalidOperationException(
                $"Cannot link a '{Side}' position to a buy position."
            );
        }

        Guard.Against.NullOrWhiteSpace(buyPositionId.Value, nameof(buyPositionId));
        LinkedBuyPositionId = buyPositionId;
        Update();
    }
}
