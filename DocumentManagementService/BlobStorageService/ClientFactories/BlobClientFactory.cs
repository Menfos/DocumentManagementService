using Azure.Storage.Blobs;

namespace DocumentManagementService.BlobStorageService.ClientFactories
{
    public class BlobClientFactory : IBlobClientFactory
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobClientFactory(string connectionString)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
        }
        public BlobContainerClient GetContainerClient(string containerName)
        {
            return _blobServiceClient.GetBlobContainerClient(containerName);
        }
    }
}