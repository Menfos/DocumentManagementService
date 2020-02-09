using System.IO;
using System.Net;
using System.Threading.Tasks;
using DocumentManagementService.BlobStorageService.ClientFactories;
using DocumentManagementService.BlobStorageService.Models;

namespace DocumentManagementService.BlobStorageService
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly IBlobClientFactory _blobClientFactory;

        public BlobStorageService(IBlobClientFactory blobClientFactory)
        {
            _blobClientFactory = blobClientFactory;
        }

        public async Task<BlobDownloadInfo> DownloadFileAsync(string fileName)
        {
            var client = _blobClientFactory.GetContainerClient(BlobConstants.BlobDocumentsContainerName);

            var lowerFileName = fileName.ToLower();
            var blobClient = client.GetBlobClient(lowerFileName);

            if (!await blobClient.ExistsAsync())
                return null;

            var downloadResult = await blobClient.DownloadAsync();

            using var downloadInfo = downloadResult.Value;
            return new BlobDownloadInfo
            {
                ContentType = downloadInfo.ContentType,
                Content = downloadInfo.Content
            };
        }

        public async Task<bool> UploadFileToStorageAsync(string fileName, Stream fileStream)
        {
            var client = _blobClientFactory.GetContainerClient(BlobConstants.BlobDocumentsContainerName);

            var lowerFileName = fileName.ToLower();
            var blobClient = client.GetBlobClient(lowerFileName);
            var uploadResult = await blobClient.UploadAsync(fileStream, overwrite: true);

            using var rawResponse = uploadResult.GetRawResponse();
            return rawResponse.Status == (int)HttpStatusCode.Created;
        }
    }
}