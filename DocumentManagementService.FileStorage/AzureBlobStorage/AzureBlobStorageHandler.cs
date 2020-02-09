using System.IO;
using System.Net;
using System.Threading.Tasks;
using DocumentManagementService.FileStorage.AzureBlobStorage.ClientFactories;
using DocumentManagementService.FileStorage.Models;
using DocumentManagementService.Logger;

namespace DocumentManagementService.FileStorage.AzureBlobStorage
{
    public class AzureBlobStorageHandler : IFileStorageHandler
    {
        private readonly IAzureBlobClientFactory _blobClientFactory;
        private readonly IServiceLogger _serviceLogger;

        public AzureBlobStorageHandler(IAzureBlobClientFactory blobClientFactory, IServiceLogger serviceLogger)
        {
            _blobClientFactory = blobClientFactory;
            _serviceLogger = serviceLogger;
        }

        public async Task<FileDownloadInfo> DownloadFileAsync(string fileName)
        {
            var client = _blobClientFactory.GetContainerClient(AzureBlobConstants.BlobDocumentsContainerName);

            var lowerFileName = fileName.ToLower();
            var blobClient = client.GetBlobClient(lowerFileName);
            var isBlobClientExists = await blobClient.ExistsAsync();
            if (!isBlobClientExists)
            {
                _serviceLogger.LogWarning($"File '{fileName}' is not found in blob storage");
                return new FileDownloadInfo
                {
                    Status = HttpStatusCode.NotFound.ToString("G"),
                    Content = null,
                    ContentType = string.Empty
                };
            }

            var downloadResult = await blobClient.DownloadAsync();
            using var downloadInfo = downloadResult.Value;
            using var rawResponse = downloadResult.GetRawResponse();

            _serviceLogger.LogInfo($"File '{fileName}' download responded with status '{rawResponse.Status}-{rawResponse.ReasonPhrase}'");
            return new FileDownloadInfo
            {
                Status = rawResponse.ReasonPhrase,
                ContentType = downloadInfo.ContentType,
                Content = downloadInfo.Content
            };
        }

        public async Task<bool> UploadFileToStorageAsync(string fileName, Stream fileStream)
        {
            var client = _blobClientFactory.GetContainerClient(AzureBlobConstants.BlobDocumentsContainerName);

            var lowerFileName = fileName.ToLower();
            var blobClient = client.GetBlobClient(lowerFileName);

            var uploadResult = await blobClient.UploadAsync(fileStream, overwrite: true);
            using var rawResponse = uploadResult.GetRawResponse();

            _serviceLogger.LogInfo($"File '{fileName}' uploaded with status '{rawResponse.Status}-{rawResponse.ReasonPhrase}'");
            return rawResponse.Status == (int)HttpStatusCode.Created;
        }

        public async Task<FileRemovalInfo> RemoveFileFromStorageAsync(string fileName)
        {
            var client = _blobClientFactory.GetContainerClient(AzureBlobConstants.BlobDocumentsContainerName);

            var lowerFileName = fileName.ToLower();
            var blobClient = client.GetBlobClient(lowerFileName);
            if (!await blobClient.ExistsAsync())
            {
                _serviceLogger.LogWarning($"File '{fileName}' is not found in blob storage");
                return new FileRemovalInfo { IsRemoved = false, Status = HttpStatusCode.NotFound.ToString("G") };
            }

            using var removalInfo = await blobClient.DeleteAsync();
            _serviceLogger.LogInfo($"File '{fileName}' removed with status '{removalInfo.Status}-{removalInfo.ReasonPhrase}'");
            return new FileRemovalInfo
            {
                IsRemoved = removalInfo.Status == (int)HttpStatusCode.Accepted,
                Status = removalInfo.ReasonPhrase
            };
        }
    }
}