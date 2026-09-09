using System.Collections.Concurrent;
using System.Diagnostics;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Tests.WebSockets.TestUtils;

public sealed class ActivityRecorder : IDisposable
{
    private readonly ActivityListener _listener;
    private readonly ConcurrentBag<Activity> _activities = [];

    public IReadOnlyList<Activity> Activities => [.. _activities];

    public ActivityRecorder()
    {
        _listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == WebSocketTelemetry.ActivitySourceName,
            Sample = (ref _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = activity => _activities.Add(activity),
        };

        ActivitySource.AddActivityListener(_listener);
    }

    public void Dispose()
    {
        _listener.Dispose();
    }
}
