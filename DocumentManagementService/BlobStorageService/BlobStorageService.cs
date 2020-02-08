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

        public async Task<BlobUploadResult> UploadFileToStorageAsync(string fileName, Stream fileStream)
        {
            var client = _blobClientFactory.GetContainerClient(BlobConstants.BlobDocumentsContainerName);

            var lowerFileName = fileName.ToLower();
            var blobClient = client.GetBlobClient(lowerFileName);
            var uploadResult = await blobClient.UploadAsync(fileStream, overwrite: true);

            using var rawResponse = uploadResult.GetRawResponse();
            var isSuccess = rawResponse.Status == (int)HttpStatusCode.Created;
            var uploadedPath = isSuccess
                ? blobClient.Uri.AbsoluteUri
                : string.Empty;
            
            return new BlobUploadResult
            {
                IsSuccess = isSuccess,
                Path = uploadedPath
            };
        }
    }
}