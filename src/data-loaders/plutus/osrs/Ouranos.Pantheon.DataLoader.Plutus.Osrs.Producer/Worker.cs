using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application.Commands.Trades.ProcessTrades;
using Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application.Queries.Trades.GetTrades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Osrs.Producer;

public class Worker : BackgroundService
{
    private readonly IDispatcher _dispatcher;
    private readonly TimeSpan? _interval;
    private readonly ILogger<Worker> _logger;

    public Worker(
        ILogger<Worker> logger,
        IDispatcher dispatcher,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(configuration);

        _logger = logger;
        _dispatcher = dispatcher;

        var intervalSeconds = configuration.GetValue<int?>("Ouranos:IntervalSeconds", null);
        _interval = intervalSeconds.HasValue ? TimeSpan.FromSeconds(intervalSeconds.Value) : null;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Worker starting.");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await ProcessTrades(cancellationToken);

                if (_interval.HasValue)
                {
                    await Task.Delay(_interval.Value, cancellationToken);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An unexpected error occured during worker execution.");

                // Add a delay so we do not continuously spam our dependencies with requests on failures.
                var errorDelay = _interval ?? TimeSpan.FromSeconds(30);
                await Task.Delay(errorDelay, cancellationToken);
            }
        }

        _logger.LogInformation("Cancellation requested, worker stopped.");
    }

    private async Task ProcessTrades(CancellationToken cancellationToken)
    {
        var getTradesRequest = new GetTradesInput();
        var tradeWrapper = await _dispatcher.Send(getTradesRequest, cancellationToken);

        var processTradesRequest = new ProcessTradesInput(tradeWrapper.Value);
        await _dispatcher.Send(processTradesRequest, cancellationToken);
    }
}