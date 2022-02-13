using Noppes.Fluffle.Utils;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Noppes.Fluffle.B2
{
    public class B2UploadManagerItem : WorkSchedulerItem<B2UploadResponse>
    {
        public Func<Stream> OpenStream { get; set; }

        public string FileName { get; set; }

        public string ContentType { get; set; }
    }

    public class B2UploadManager : WorkScheduler<B2UploadManagerItem, int, B2UploadResponse>
    {
        private readonly B2Bucket _bucket;

        public B2UploadManager(int numberOfWorkers, B2Bucket bucket) : base(numberOfWorkers)
        {
            _bucket = bucket;
        }

        protected override async Task<B2UploadResponse> HandleAsync(B2UploadManagerItem item)
        {
            return await _bucket.UploadAsync(item.OpenStream, item.FileName, item.ContentType);
        }
    }
}
