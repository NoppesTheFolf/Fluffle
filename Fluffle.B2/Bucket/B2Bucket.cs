using Flurl.Http;
using Noppes.Fluffle.Http;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace Noppes.Fluffle.B2
{
    /// <summary>
    /// Represents a bucket on B2.
    /// </summary>
    public class B2Bucket
    {
        /// <summary>
        /// Unique identifier of the bucket.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The user-provided name of the bucket.
        /// </summary>
        public string Name { get; }

        private readonly B2Client _client;
        private readonly ConcurrentStack<B2BucketUploadClient> _uploadClients;

        public B2Bucket(string bucketId, string bucketName, B2Client client)
        {
            Id = bucketId;
            Name = bucketName;
            _client = client;
            _uploadClients = new ConcurrentStack<B2BucketUploadClient>();
        }

        /// <summary>
        /// Deletes the file with the given name and ID.
        /// </summary>
        public Task DeleteFileVersionAsync(string fileName, string fileId)
        {
            return _client.AuthorizedRequestAsync(request => request.PostJsonAsync(new
            {
                fileName,
                fileId
            }), B2Endpoints.DeleteFileVersion);
        }

        /// <summary>
        /// Retrieves a list of files contained in the bucket. Every 1000 file names retrieved cost
        /// a single class C transaction.
        /// </summary>
        public Task<B2ListFileNamesResponse> ListFileNamesAsync(int maxFileCount = 1000, string startFileName = null)
        {
            return _client.AuthorizedRequestAsync(request => request.PostJsonReceiveJsonAsync<B2ListFileNamesResponse>(new
            {
                bucketId = Id,
                maxFileCount,
                startFileName
            }), B2Endpoints.ListFileNames);
        }

        /// <summary>
        /// Uploads a file to the bucket with the given contents, name and content type. Makes use
        /// of a pool of upload clients to make parallel uploading possible. This method is
        /// thread-safe. The stream containing the content must be reusable in the sense that it can
        /// be opened multiple times. This is due to the HTTP client disposing the stream if a
        /// requests fail even though it should be retried because a new upload URL should be
        /// requested. If the stream gets disposed, this isn't possible anymore.
        /// </summary>
        public async Task<B2UploadResponse> UploadAsync(Func<Stream> openStream, string fileName, string contentType)
        {
            if (!_uploadClients.TryPop(out var uploadClient))
                uploadClient = new B2BucketUploadClient(_client, this);

            try
            {
                var response = await uploadClient.UploadAsync(openStream, fileName, contentType);
                var downloadUri = new Uri(_client.DownloadUri, $"file/{Name}/{response.FileName}");

                response.DownloadUrl = downloadUri.AbsoluteUri;
                return response;
            }
            finally
            {
                _uploadClients.Push(uploadClient);
            }
        }
    }
}
