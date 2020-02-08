using Azure.Storage.Blobs;

namespace DocumentManagementService.BlobStorageService.ClientFactories
{
    public interface IBlobClientFactory
    {
        BlobContainerClient GetContainerClient(string containerName);
    }
}