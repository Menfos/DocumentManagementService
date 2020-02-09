using Azure.Storage.Blobs;

namespace DocumentManagementService.FileStorage.AzureBlobStorage.ClientFactories
{
    public class AzureBlobClientFactory : IAzureBlobClientFactory
    {
        private readonly BlobServiceClient _blobServiceClient;

        public AzureBlobClientFactory(string connectionString)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public BlobContainerClient GetContainerClient(string containerName)
        {
            return _blobServiceClient.GetBlobContainerClient(containerName);
        }
    }
}