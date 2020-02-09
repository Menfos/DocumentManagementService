using Azure.Storage.Blobs;

namespace DocumentManagementService.FileStorage.AzureBlobStorage.ClientFactories
{
    public interface IAzureBlobClientFactory
    {
        BlobContainerClient GetContainerClient(string containerName);
    }
}