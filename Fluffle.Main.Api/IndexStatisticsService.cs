using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using Noppes.Fluffle.Api.RunnableServices;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Main.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api
{
    public record IndexStatistics(
        IDictionary<(int, int), (int, int)> Total,
        IDictionary<int, IEnumerable<StatusModelHistory>> HistoryLast30Days,
        IDictionary<int, IEnumerable<StatusModelHistory>> HistoryLast24Hours
    );

    public class IndexStatisticsScope : IDisposable
    {
        private readonly IndexStatistics _statistics;
        private readonly IDisposable _disposableLock;

        public IndexStatisticsScope(IndexStatistics statistics, IDisposable disposableLock)
        {
            _statistics = statistics;
            _disposableLock = disposableLock;
        }

        public (int total, int indexed, IEnumerable<StatusModelHistory> historyLast30Days, IEnumerable<StatusModelHistory> historyLast24Hours) Get(int platformId, params int[] mediaTypeIds)
        {
            var total = 0;
            var indexed = 0;
            foreach (var result in mediaTypeIds.Select(x => Get(platformId, x)))
            {
                total += result.total;
                indexed += result.indexed;
            }

            var historyLast30Days = _statistics.HistoryLast30Days[platformId];
            var historyLast24Hours = _statistics.HistoryLast24Hours[platformId];

            return (total, indexed, historyLast30Days, historyLast24Hours);
        }

        public (int total, int indexed) Get(int platformId, int mediaTypeId)
        {
            var (total, indexed) = _statistics.Total.TryGetValue((platformId, mediaTypeId), out var statistics)
                ? statistics
                : (0, 0);

            return (total, indexed);
        }

        public void Dispose()
        {
            _disposableLock.Dispose();
        }
    }

    public class IndexStatisticsService : IService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<IndexStatisticsService> _logger;
        private IndexStatistics _statistics;
        private readonly ManualResetEventSlim _initMutex;
        private readonly AsyncReaderWriterLock _mutex;

        public IndexStatisticsService(IServiceProvider services, ILogger<IndexStatisticsService> logger)
        {
            _services = services;
            _logger = logger;
            _initMutex = new ManualResetEventSlim();
            _mutex = new AsyncReaderWriterLock();
        }

        public IndexStatisticsScope Scope()
        {
            _initMutex.Wait();
            var readerLock = _mutex.ReaderLock();

            return new IndexStatisticsScope(_statistics, readerLock);
        }

        public async Task RunAsync()
        {
            using var scope = _services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<FluffleContext>();

            _logger.LogInformation("Updating indexing statistics...");

            var total = await CalculateTotalStatistics(context);

            var now = DateTimeOffset.UtcNow;
            var historyLast30Days = CalculateHistory(context, now, 30, TimeSpan.FromDays, true, true, true, false);
            var historyLast24Hours = CalculateHistory(context, now, 24, TimeSpan.FromHours, true, true, true, true);

            using var _ = await _mutex.WriterLockAsync();
            _statistics = new IndexStatistics(total, historyLast30Days, historyLast24Hours);
            _initMutex.Set();

            _logger.LogInformation("Updated indexing statistics.");
        }

        private static async Task<IDictionary<(int, int), (int, int)>> CalculateTotalStatistics(FluffleContext context)
        {
            return await context.Content
                .Where(c => !c.IsDeleted)
                .GroupBy(c => new { c.PlatformId, c.MediaTypeId })
                .Select(cg => new
                {
                    cg.Key.PlatformId,
                    cg.Key.MediaTypeId,
                    Count = cg.Count(c => c.RequiresIndexing || c.IsIndexed),
                    IndexedCount = cg.Count(c => c.IsIndexed)
                }).ToDictionaryAsync(c => (c.PlatformId, c.MediaTypeId), c => (c.Count, c.IndexedCount));
        }

        private record CalculateHistoryData(int PlatformId, DateTime When);

        private record CalculateHistoryGroup(int PlatformId, DateTimeOffset When, int Count);

        private static IDictionary<int, IEnumerable<StatusModelHistory>> CalculateHistory(FluffleContext context, DateTimeOffset now, int span, Func<double, TimeSpan> createSpan, bool year, bool month, bool day, bool hour)
        {
            DateTimeOffset CreateUtcDateTimeOffset(DateTimeOffset from)
            {
                return new DateTimeOffset(year ? from.Year : 0, month ? from.Month : 0, day ? from.Day : 0,
                    hour ? from.Hour : 0, 0, 0, TimeSpan.Zero);
            }

            var results = new Dictionary<int, IEnumerable<StatusModelHistory>>();

            var start = CreateUtcDateTimeOffset(now).Subtract(createSpan(span));
            var dates = Enumerable.Range(1, span).Select(i => start.Add(createSpan(i))).ToList();
            var platformIds = Enum.GetValues<PlatformConstant>().Select(p => (int)p).ToList();

            var scrapeResults = CalculateHistory(context.Content, platformIds, start, CreateUtcDateTimeOffset, dates,
                c => c.CreatedAt, c => new CalculateHistoryData(c.PlatformId, c.CreatedAt));
            var indexResults = CalculateHistory(context.ImageHashes.Include(ih => ih.Image), platformIds, start,
                CreateUtcDateTimeOffset, dates, c => c.CreatedAt,
                c => new CalculateHistoryData(c.Image.PlatformId, c.CreatedAt));
            var errorResults = CalculateHistory(context.ContentErrors.Include(ce => ce.Content), platformIds, start,
                CreateUtcDateTimeOffset, dates, c => c.CreatedAt,
                ce => new CalculateHistoryData(ce.Content.PlatformId, ce.CreatedAt));

            foreach (var platformId in platformIds)
            {
                var history = dates
                    .Select(date => new StatusModelHistory
                    {
                        When = date,
                        ScrapedCount = scrapeResults[platformId][date],
                        IndexedCount = indexResults[platformId][date],
                        ErrorCount = errorResults[platformId][date],
                    })
                    .ToList();

                results.Add(platformId, history);
            }

            return results;
        }

        private static IDictionary<int, Dictionary<DateTimeOffset, int>> CalculateHistory<TEntity>(IQueryable<TEntity> queryable, IEnumerable<int> platformIds, DateTimeOffset start, Func<DateTimeOffset, DateTimeOffset> createUtcDateTimeOffset, IEnumerable<DateTimeOffset> dates, Expression<Func<TEntity, DateTime>> memberExpression, Expression<Func<TEntity, CalculateHistoryData>> selector) where TEntity : class
        {
            var propertyName = ((MemberExpression)memberExpression.Body).Member.Name;
            var expressionParameter = Expression.Parameter(typeof(TEntity));
            var expressionLeft = Expression.Property(expressionParameter, propertyName);
            var expressionRight = Expression.Constant(start.Date, typeof(DateTime));
            var predicate = Expression.Lambda<Func<TEntity, bool>>(
                Expression.GreaterThanOrEqual(expressionLeft, expressionRight),
                expressionParameter
            );

            var queryResults = queryable
                .Where(predicate)
                .Select(selector)
                .AsEnumerable()
                .GroupBy(x => new
                {
                    x.PlatformId,
                    When = createUtcDateTimeOffset(x.When)
                }).Select(xg => new CalculateHistoryGroup(xg.Key.PlatformId, xg.Key.When, xg.Count()))
                .ToDictionary(cgh => (cgh.PlatformId, cgh.When), cgh => cgh.Count);

            var results = platformIds.ToDictionary(id => id, _ => dates.ToDictionary(date => date, _ => 0));
            foreach (var ((platformId, dateTime), value) in queryResults)
                results[platformId][dateTime] = value;

            return results;
        }
    }
}
