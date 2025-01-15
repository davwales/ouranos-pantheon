using MediatR;
using Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application.Commands.Trades.ProcessTrades;
using Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application.Queries.Trades.GetTrades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Osrs.Producer;

public class Worker : BackgroundService
{
    private readonly TimeSpan? _interval;
    private readonly ILogger<Worker> _logger;
    private readonly IMediator _mediator;

    public Worker(
        ILogger<Worker> logger,
        IMediator mediator,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        ArgumentNullException.ThrowIfNull(configuration);

        _logger = logger;
        _mediator = mediator;

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
        var trades = await _mediator.Send(getTradesRequest, cancellationToken);

        var processTradesRequest = new ProcessTradesInput(trades);
        await _mediator.Send(processTradesRequest, cancellationToken);
    }
}