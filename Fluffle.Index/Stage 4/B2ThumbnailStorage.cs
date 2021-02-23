using Noppes.Fluffle.B2;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Utils;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Index
{
    public class B2ThumbnailStorage
    {
        private readonly B2Client _client;
        private readonly string _salt;

        public B2ThumbnailStorage(B2Client client, string salt)
        {
            _client = client;

            if (string.IsNullOrWhiteSpace(salt))
                throw new ArgumentNullException(nameof(salt));

            if (salt.Length != 32)
                throw new ArgumentOutOfRangeException(nameof(salt));

            _salt = salt;
        }

        public async Task<B2UploadResponse> PutAsync(Func<Stream> openStream, PlatformConstant platform, string id, string discriminator, ImageFormatConstant imageFormat)
        {
            var bucket = await _client.GetBucketAsync();
            var fileName = GetFilename(platform, id, discriminator, imageFormat);

            var result = await HttpResiliency.RunAsync(() => bucket.UploadAsync(openStream, fileName, imageFormat.GetMimeType()), () =>
            {
                Log.Warning("Request to Backblaze B2 timed out.");
            }, onRetry: timeout =>
            {
                Log.Warning("Retrying request to Backblaze B2 in {timeout}", timeout);
            });

            return result;
        }

        public string GetFilename(PlatformConstant platform, string id, string discriminator, ImageFormatConstant imageFormat)
        {
            var identifierString = $"{_salt}_{(int)platform}_{id}_{discriminator}";
            var hash = Hashing.Md5(identifierString);

            return $"{hash}.{imageFormat.GetFileExtension()}";
        }
    }
}
