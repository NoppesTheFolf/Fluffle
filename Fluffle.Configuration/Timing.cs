using SerilogTimings;
using System;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Configuration
{
    /// <summary>
    /// Provides an async wrapper around <see cref="Operation"/> objects to make them easier to work
    /// with in an asynchronous context.
    /// </summary>
    public static class LogEx
    {
        public static async Task TimeAsync(Func<Task> func, string messageTemplate, params object[] args)
        {
            using (Operation.Time(messageTemplate, args))
            {
                await func();
            }
        }

        public static async Task<T> TimeAsync<T>(Func<Task<T>> func, string messageTemplate, params object[] args)
        {
            using (Operation.Time(messageTemplate, args))
            {
                return await func();
            }
        }
    }
}
