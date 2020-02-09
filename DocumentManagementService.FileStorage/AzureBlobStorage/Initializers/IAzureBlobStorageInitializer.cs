namespace DocumentManagementService.FileStorage.AzureBlobStorage.Initializers
{
    public interface IAzureBlobStorageInitializer
    {
        void InitializeDocumentContainerIfNotExists();
    }
}