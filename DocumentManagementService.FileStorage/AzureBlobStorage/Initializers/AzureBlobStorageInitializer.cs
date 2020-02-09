using DocumentManagementService.FileStorage.AzureBlobStorage.ClientFactories;

namespace DocumentManagementService.FileStorage.AzureBlobStorage.Initializers
{
    public class AzureBlobStorageInitializer : IAzureBlobStorageInitializer
    {
        private readonly IAzureBlobClientFactory _blobClientFactory;

        public AzureBlobStorageInitializer(IAzureBlobClientFactory blobClientFactory)
        {
            _blobClientFactory = blobClientFactory;
        }

        public void InitializeDocumentContainerIfNotExists()
        {
            var containerClient = _blobClientFactory.GetContainerClient(AzureBlobConstants.BlobDocumentsContainerName);
            containerClient.CreateIfNotExists();
        }
    }
}