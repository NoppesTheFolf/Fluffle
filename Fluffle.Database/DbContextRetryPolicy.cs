using Microsoft.EntityFrameworkCore;
using Npgsql;
using Polly;
using System;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Database
{
    public static class DbContextRetryPolicy
    {
        /// <summary>
        /// Catches exceptions of type <see cref="NpgsqlException"/> when running <paramref
        /// name="action"/> and retries running <paramref name="action"/> if the exception is deemed transient.
        /// </summary>
        public static async Task<T> ResilientAsync<TContext, T>(this TContext context, Func<TContext, Task<T>> action, Action onRetry) where TContext : DbContext
        {
            return await BuildRetryPolicy<T>(onRetry).Invoke(() => action(context));
        }

        /// <summary>
        /// Catches exceptions of type <see cref="NpgsqlException"/> when running <paramref
        /// name="action"/> and retries running <paramref name="action"/> if the exception is deemed transient.
        /// </summary>
        public static async Task ResilientAsync<TContext>(this TContext context, Func<TContext, Task> action, Action onRetry) where TContext : DbContext
        {
            await BuildRetryPolicy<bool>(onRetry).Invoke(async () =>
            {
                await action(context);

                return true;
            });
        }

        private static Func<Func<Task<TResult>>, Task<TResult>> BuildRetryPolicy<TResult>(Action onRetry = null)
        {
            return Policy<TResult>
                .Handle<NpgsqlException>(IsTransient)
                .OrInner<NpgsqlException>(IsTransient)
                .WaitAndRetryForeverAsync(retryAttempt =>
                {
                    onRetry?.Invoke();

                    return TimeSpan.FromSeconds(5);
                }).ExecuteAsync;
        }

        /// <summary>
        /// Whether or not the provided <see cref="NpgsqlException"/> should be considered transient
        /// or not.
        /// </summary>
        private static bool IsTransient(NpgsqlException exception)
        {
            if (exception.IsTransient)
                return true;

            // We should expect the database to come online eventually again if it has been manually shutdown
            if (exception is PostgresException postgresException && postgresException.SqlState == PostgresErrorCodes.AdminShutdown)
                return true;

            return false;
        }
    }
}
