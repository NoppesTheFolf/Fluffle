using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;

namespace Noppes.Fluffle.Utils;

public class CheckpointStopwatchScope<T> : IDisposable where T : class, ITiming
{
    public CheckpointStopwatch<T> Stopwatch { get; }
    private Expression<Func<T, int?>> _expression;

    public CheckpointStopwatchScope(CheckpointStopwatch<T> stopwatch, Expression<Func<T, int?>> expression)
    {
        Stopwatch = stopwatch;
        _expression = expression;
    }

    public void Next(Expression<Func<T, int?>> expression)
    {
        Stopwatch.SetCheckpoint(_expression);
        _expression = expression;
    }

    public void Dispose()
    {
        Stopwatch.SetCheckpoint(_expression);
    }
}

public static class CheckpointStopwatch
{
    internal static readonly ConcurrentDictionary<Type, int> CountDictionary = new();

    public static CheckpointStopwatch<T> StartNew<T>(T obj) where T : class, ITiming
    {
        var checkpointStopwatch = new CheckpointStopwatch<T>(obj);
        checkpointStopwatch.Start();

        return checkpointStopwatch;
    }
}

public interface ITiming
{
    public DateTime StartedAt { get; set; }

    public int Sequence { get; set; }
}

public class CheckpointStopwatch<T> where T : class, ITiming
{
    private readonly T _obj;
    private readonly SemaphoreSlim _mutex;
    private readonly Stopwatch _stopwatch;

    public long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds;

    public CheckpointStopwatch(T obj)
    {
        _obj = obj;
        _mutex = new SemaphoreSlim(1);
        _stopwatch = new Stopwatch();
    }

    public void Start()
    {
        _obj.StartedAt = DateTime.UtcNow;
        _obj.Sequence = CheckpointStopwatch.CountDictionary.AddOrUpdate(typeof(T), _ => 1, (_, i) => i + 1);

        _stopwatch.Start();
    }

    public void SetCheckpoint(Expression<Func<T, int?>> expression)
    {
        if (expression.Body.NodeType != ExpressionType.MemberAccess)
            throw new ArgumentException("Given expression does not select a property.", nameof(expression));

        var parameter = expression.Parameters[0];
        var variable = Expression.Variable(typeof(int?));
        var assign = Expression.Assign(expression.Body, variable);
        var set = Expression.Lambda<Action<T, int?>>(assign, parameter, variable);
        var func = set.Compile();

        _mutex.Wait();
        try
        {
            func(_obj, (int)_stopwatch.ElapsedMilliseconds);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public CheckpointStopwatchScope<T> ForCheckpoint(Expression<Func<T, int?>> expression) => new(this, expression);
}
