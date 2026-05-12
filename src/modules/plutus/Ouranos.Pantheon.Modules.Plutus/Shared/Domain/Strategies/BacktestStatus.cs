namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

public enum BacktestStatus
{
    Pending = 1,
    Running = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5,
}
