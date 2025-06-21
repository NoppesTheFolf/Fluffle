using Microsoft.ApplicationInsights;
using System.Diagnostics;

namespace Fluffle.Ingestion.Worker.Telemetry;

public static class TimingExtensions
{
    public static async Task Timed(this Task task, TelemetryClient telemetryClient, string name)
    {
        var metric = telemetryClient.GetMetric(name);
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await task;
        }
        finally
        {
            metric.TrackValue(stopwatch.ElapsedMilliseconds);
        }
    }

    public static async Task<T> Timed<T>(this Task<T> task, TelemetryClient telemetryClient, string name)
    {
        var metric = telemetryClient.GetMetric(name);
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await task;
            return result;
        }
        finally
        {
            metric.TrackValue(stopwatch.ElapsedMilliseconds);
        }
    }
}
