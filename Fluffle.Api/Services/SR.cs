using System;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Api.Services
{
    /// <summary>
    /// The result produced by a <see cref="Service"/>. SR is an abbreviation for ServiceResult to
    /// improve readability due to common usage.
    /// </summary>
    public class SR<TOutput>
    {
        protected SE Error { get; set; }

        protected TOutput Output { get; }

        public SR(TOutput output)
        {
            Output = output;
        }

        public SR(SE error)
        {
            Error = error;
        }

        public T Handle<T>(Func<SE, T> error, Func<TOutput, T> success)
        {
            return Error != null ? error(Error) : success(Output);
        }

        public async Task<T> HandleAsync<T>(Func<SE, Task<T>> error, Func<TOutput, Task<T>> success)
        {
            return Error != null ? await error(Error) : await success(Output);
        }
    }
}
