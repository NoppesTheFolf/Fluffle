using Noppes.E621;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Index
{
    public class E621DownloadClient : DownloadClient
    {
        private readonly E621Client _e621Client;

        public E621DownloadClient(E621Client e621Client)
        {
            _e621Client = e621Client;
        }

        protected override Task<Stream> GetStreamAsync(string url, CancellationToken cancellationToken = default)
        {
            return _e621Client.GetStreamAsync(url);
        }
    }
}
