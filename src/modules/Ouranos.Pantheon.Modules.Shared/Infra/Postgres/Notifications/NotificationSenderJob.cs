using System.Collections.Concurrent;
using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.Domain.Notifications;
using TickerQ.Utilities.Base;

namespace Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Notifications;

public sealed class NotificationSenderJob
{
    private readonly ILogger<NotificationSenderJob> _logger;
    private readonly IDbContextFactory<SharedDbContext> _dbContextFactory;
    private readonly NotificationOptions _options;
    private readonly ConcurrentDictionary<NotificationChannel, INotificationSender> _senders;

    public NotificationSenderJob(
        ILogger<NotificationSenderJob> logger,
        IDbContextFactory<SharedDbContext> dbContextFactory,
        IEnumerable<INotificationSender> senders,
        IOptions<NotificationOptions> options
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContextFactory);
        Guard.Against.Null(senders);
        Guard.Against.Null(options);

        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _options = options.Value;
        _senders = new ConcurrentDictionary<NotificationChannel, INotificationSender>(
            senders.ToDictionary(s => s.Channel, s => s)
        );
    }

    [TickerFunction("NotificationSender", "* * * * * *")]
    public async Task Execute(TickerFunctionContext _, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _logger.LogTrace("Starting notification sender job.");

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);

        var pending = await dbContext
            .Notifications.Where(n => n.Status == NotificationStatus.Pending)
            .ToListAsync(ct);

        if (pending.Count == 0)
        {
            _logger.LogDebug("No pending notifications to send.");
            return;
        }

        _logger.LogDebug("Found {Count} pending notification(s) to send.", pending.Count);

        foreach (var notification in pending)
        {
            ct.ThrowIfCancellationRequested();
            await SendNotificationAsync(notification, ct);
        }

        await dbContext.SaveChangesAsync(ct);
        _logger.LogDebug("Notification sender job completed.");
    }

    private async Task SendNotificationAsync(Notification notification, CancellationToken ct)
    {
        if (!_senders.TryGetValue(notification.Channel, out INotificationSender? sender))
        {
            var error = $"No sender registered for channel '{notification.Channel}'.";
            _logger.LogError(error);
            notification.MarkFailed(error);
            return;
        }

        if (notification.RetryCount >= _options.MaxRetries)
        {
            _logger.LogWarning(
                "Notification '{NotificationId}' exceeded max retries ({MaxRetries}). Marking as permanently failed.",
                notification.Id,
                _options.MaxRetries
            );
            return;
        }

        try
        {
            await sender.SendAsync(
                notification.Recipient,
                notification.Subject,
                notification.Message,
                ct
            );
            notification.MarkSent();
            _logger.LogDebug(
                "Notification '{NotificationId}' sent successfully via '{Channel}'.",
                notification.Id,
                notification.Channel
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send notification '{NotificationId}' via '{Channel}'.",
                notification.Id,
                notification.Channel
            );
            notification.MarkFailed(ex.Message);
        }
    }
}
