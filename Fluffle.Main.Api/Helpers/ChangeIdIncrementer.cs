using Nito.AsyncEx;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Main.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Noppes.Fluffle.Main.Api.Helpers;

public class ChangeIdIncrementer<T> where T : class, ITrackable
{
    private readonly IDictionary<PlatformConstant, AsyncLock> _locks;
    private readonly IDictionary<PlatformConstant, ChangeIdIncrementerLock<T>> _ciis;

    public ChangeIdIncrementer()
    {
        _locks = Enum.GetValues<PlatformConstant>()
            .ToDictionary(pc => pc, pc => new AsyncLock());

        _ciis = Enum.GetValues<PlatformConstant>()
            .ToDictionary(pc => pc, pc => new ChangeIdIncrementerLock<T>(pc));
    }

    public void Initialize(FluffleContext context)
    {
        foreach (var changeIdIncrementerLock in _ciis.Values)
        {
            changeIdIncrementerLock.Initialize(context);
        }
    }

    public IDisposable Lock(PlatformConstant platform, out ChangeIdIncrementerLock<T> cii)
    {
        cii = _ciis[platform];

        return _locks[platform].Lock();
    }
}

public class ChangeIdIncrementerLock<T> where T : class, ITrackable
{
    private readonly PlatformConstant _platform;
    private bool _initialized;
    private long _changeId;

    public ChangeIdIncrementerLock(PlatformConstant platform)
    {
        _platform = platform;
    }

    public void Next(T target)
    {
        target.ChangeId = Next();
    }

    public long Next()
    {
        if (!_initialized)
            throw new InvalidOperationException();

        return ++_changeId;
    }

    public void Initialize(FluffleContext context)
    {
        if (_initialized)
            throw new InvalidOperationException();

        var set = context.Set<T>();
        var platformId = (int)_platform;
        var maxChangeId = set.Where(e => e.PlatformId == platformId).Max(i => i.ChangeId) ?? 0;

        _changeId = maxChangeId;
        _initialized = true;
    }
}
