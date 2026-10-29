using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;

public sealed class Position : BaseEntity<Id<Position>>
{
    private Position(Id<Position> id)
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

    public decimal? TargetPrice { get; private set; }

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
        Symbol? symbol = null,
        decimal? targetPrice = null,
        decimal committedSellQuantity = 0
    )
    {
        Guard.Against.NegativeOrZero(cost);
        Guard.Against.NegativeOrZero(quantity);
        GuardTargetPrice(side, targetPrice);

        if (linkedBuyPosition is not null)
        {
            GuardLink(
                side,
                marketId,
                symbolId,
                strategyId,
                quantity,
                linkedBuyPosition,
                committedSellQuantity
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
            TargetPrice = targetPrice,
            StrategyId = strategyId,
            Notes = notes,
            LinkedBuyPositionId = linkedBuyPosition?.Id,
            _linkedBuyPosition = linkedBuyPosition,
            _symbol = symbol,
        };
    }

    public void Modify(decimal cost, decimal quantity, string? notes, decimal? targetPrice)
    {
        if (Status != PositionStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Cannot modify a position with status '{Status}'."
            );
        }

        Guard.Against.NegativeOrZero(cost);
        Guard.Against.NegativeOrZero(quantity);
        GuardTargetPrice(Side, targetPrice);

        Cost = cost;
        Quantity = quantity;
        Notes = notes;
        TargetPrice = targetPrice;

        Update();
    }

    public void SetTargetPrice(decimal? targetPrice)
    {
        if (
            Side != PositionSide.Buy
            || Status is not (PositionStatus.Pending or PositionStatus.Bought)
        )
        {
            throw new InvalidOperationException(
                $"Cannot set a target price on a {Side} position with status '{Status}'."
            );
        }

        GuardTargetPrice(Side, targetPrice);

        TargetPrice = targetPrice;
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

    /// <param name="buyPosition">The Bought buy position this sell closes out.</param>
    /// <param name="committedSellQuantity">
    /// Quantity of the buy already covered by its other linked sells (excluding this one and any
    /// that did not sell).
    /// </param>
    public void LinkPosition(Position buyPosition, decimal committedSellQuantity)
    {
        Guard.Against.Null(buyPosition);

        if (buyPosition.Id == Id)
        {
            throw new BadRequestException("Cannot link a position to itself.");
        }

        GuardLink(
            Side,
            MarketId,
            SymbolId,
            StrategyId,
            Quantity,
            buyPosition,
            committedSellQuantity
        );

        LinkedBuyPositionId = buyPosition.Id;
        Update();
    }

    private static void GuardLink(
        PositionSide side,
        Id<Market> marketId,
        Id<Symbol> symbolId,
        Id<Strategy>? strategyId,
        decimal quantity,
        Position buyPosition,
        decimal committedSellQuantity
    )
    {
        if (side != PositionSide.Sell)
        {
            throw new BadRequestException($"Cannot link a '{side}' position to a buy position.");
        }

        if (!buyPosition.CanBeLinkedAsTarget())
        {
            throw new BadRequestException(
                $"Cannot link to a '{buyPosition.Side}' position with status '{buyPosition.Status}'; it must be a Bought buy position."
            );
        }

        if (buyPosition.MarketId != marketId || buyPosition.SymbolId != symbolId)
        {
            throw new BadRequestException(
                "The buy position must be for the same market and symbol as the sell position."
            );
        }

        if (
            strategyId is not null
            && buyPosition.StrategyId is not null
            && buyPosition.StrategyId != strategyId
        )
        {
            throw new BadRequestException(
                "The buy position belongs to a different strategy than the sell position."
            );
        }

        var remainingQuantity = buyPosition.Quantity - committedSellQuantity;
        if (quantity > remainingQuantity)
        {
            throw new BadRequestException(
                $"The sell quantity {quantity} exceeds the buy position's remaining quantity {remainingQuantity}."
            );
        }
    }

    private static void GuardTargetPrice(PositionSide side, decimal? targetPrice)
    {
        if (targetPrice is not { } price)
        {
            return;
        }

        if (side != PositionSide.Buy)
        {
            throw new InvalidOperationException("Only buy positions can have a target price.");
        }

        Guard.Against.NegativeOrZero(price, nameof(targetPrice));
    }
}
