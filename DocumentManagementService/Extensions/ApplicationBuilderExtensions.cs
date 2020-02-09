using DocumentManagementService.Data.CosmosDb.Initializers;
using DocumentManagementService.FileStorage.AzureBlobStorage.Initializers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentManagementService.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseBlobInitializer(this IApplicationBuilder applicationBuilder)
        {
            var blobStorageInitializer = applicationBuilder.ApplicationServices.GetRequiredService<IAzureBlobStorageInitializer>();
            blobStorageInitializer.InitializeDocumentContainerIfNotExists();

            return applicationBuilder;
        }

        public static IApplicationBuilder UseCosmosDbInitializer(this IApplicationBuilder applicationBuilder)
        {
            var cosmosDataInitializer = applicationBuilder.ApplicationServices.GetRequiredService<ICosmosDataInitializer>();
            cosmosDataInitializer
                .InitializePdfDocumentCollectionIfNotExistsAsync()
                .GetAwaiter()
                .GetResult();

            return applicationBuilder;
        }
    }
}