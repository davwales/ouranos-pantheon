using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Extensions;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

public class Backtest : BaseEntity<Id<Backtest>>
{
    protected Backtest(Id<Backtest> id) : base(id)
    {
    }

    public Id<Strategy> StrategyId { get; init; }

    public Id<Market> MarketId { get; init; }

    public DateTimeOffset StartDate { get; init; }

    public DateTimeOffset EndDate { get; init; }

    public decimal Budget { get; init; }

    public BacktestKind Kind { get; init; }

    public BacktestStatus Status { get; private set; } = BacktestStatus.Pending;

    public int ProgressPercent { get; private set; }

    public string? ProgressMessage { get; private set; }

    public BacktestResults? Results { get; private set; }

    public string? ErrorMessage { get; private set; }

    private Strategy? _strategy;
    public Strategy Strategy => _strategy ?? throw new NavigationPropertyNotLoadedException<Backtest>();

    public static Backtest Create(
        Id<Strategy> strategyId,
        Id<Market> marketId,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        decimal budget,
        Strategy? strategy = null,
        BacktestKind kind = BacktestKind.Backtest
    )
    {
        Guard.Against.NegativeOrZero(budget);
        Guard.Against.InvalidInput(endDate, nameof(endDate), d => d > startDate);

        if (strategy is not null)
        {
            Guard.Against.InvalidInput(strategy, nameof(strategy), s => s.Id == strategyId);
        }

        return new Backtest(DatabaseExtensions.CreateId<Backtest>())
        {
            StrategyId = strategyId,
            MarketId = marketId,
            StartDate = startDate.ToUniversalTime(),
            EndDate = endDate.ToUniversalTime(),
            Budget = budget,
            Kind = kind,
            _strategy = strategy
        };
    }

    public bool MarkRunning()
    {
        if (Status == BacktestStatus.Running)
        {
            return false;
        }

        if (Status != BacktestStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Cannot transition backtest '{Id}' from {Status} to {BacktestStatus.Running}."
            );
        }

        Status = BacktestStatus.Running;
        ProgressPercent = 0;
        ProgressMessage = "Loading market data...";
        Update();
        return true;
    }

    public void UpdateProgress(int percent, string message)
    {
        Guard.Against.NullOrWhiteSpace(message);

        if (Status != BacktestStatus.Running)
        {
            return;
        }

        ProgressPercent = Math.Clamp(percent, 0, 100);
        ProgressMessage = message;
        Update();
    }

    public bool Complete(BacktestResults results)
    {
        Guard.Against.Null(results);

        if (Status == BacktestStatus.Completed)
        {
            return false;
        }

        if (Status != BacktestStatus.Running)
        {
            throw new InvalidOperationException(
                $"Cannot transition backtest '{Id}' from {Status} to {BacktestStatus.Completed}."
            );
        }

        Results = results;
        Status = BacktestStatus.Completed;
        Update();
        return true;
    }

    public bool Fail(string errorMessage)
    {
        Guard.Against.NullOrWhiteSpace(errorMessage);

        if (Status == BacktestStatus.Failed)
        {
            return false;
        }

        if (Status != BacktestStatus.Running && Status != BacktestStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Cannot transition backtest '{Id}' from {Status} to {BacktestStatus.Failed}."
            );
        }

        ErrorMessage = errorMessage;
        Status = BacktestStatus.Failed;
        Update();
        return true;
    }

    public bool Cancel(string? reason = null)
    {
        if (Status == BacktestStatus.Cancelled)
        {
            return false;
        }

        if (Status != BacktestStatus.Pending && Status != BacktestStatus.Running)
        {
            throw new InvalidOperationException($"Cannot cancel backtest '{Id}' from {Status} state.");
        }

        ErrorMessage = reason ?? "Cancelled by user.";
        Status = BacktestStatus.Cancelled;
        Update();
        return true;
    }

    public bool Restart()
    {
        if (Status != BacktestStatus.Failed && Status != BacktestStatus.Cancelled)
        {
            throw new InvalidOperationException(
                $"Cannot restart backtest '{Id}' from {Status} state. Only Failed or Cancelled backtests can be restarted."
            );
        }

        Status = BacktestStatus.Pending;
        ProgressPercent = 0;
        ProgressMessage = null;
        Results = null;
        ErrorMessage = null;
        Update();
        return true;
    }
}