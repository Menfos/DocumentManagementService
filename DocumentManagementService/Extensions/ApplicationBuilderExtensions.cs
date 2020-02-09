using System.Collections.ObjectModel;
using Azure.Storage.Blobs.Models;
using DocumentManagementService.BlobStorageService;
using DocumentManagementService.BlobStorageService.ClientFactories;
using DocumentManagementService.Data.CosmosDb;
using DocumentManagementService.Data.CosmosDb.ClientFactories;
using DocumentManagementService.Data.CosmosDb.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentManagementService.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder InitializeCosmosDbStorage(this IApplicationBuilder applicationBuilder)
        {
            var blobClientFactory = applicationBuilder.ApplicationServices.GetService<IBlobClientFactory>();
            if (blobClientFactory != null)
            {
                var containerClient = blobClientFactory.GetContainerClient(BlobConstants.BlobDocumentsContainerName);
                containerClient.CreateIfNotExists();
            }

            return applicationBuilder;
        }

        public static IApplicationBuilder InitializeBlobStorage(this IApplicationBuilder applicationBuilder)
        {
            var cosmosDocumentClientFactory = applicationBuilder.ApplicationServices.GetService<ICosmosDocumentClientFactory>();
            if (cosmosDocumentClientFactory != null)
            {
                var documentClient = cosmosDocumentClientFactory.GetClient();
                var database = new Database { Id = CosmosDbConstants.DocumentsDatabaseId };
                documentClient
                    .CreateDatabaseIfNotExistsAsync(database)
                    .GetAwaiter()
                    .GetResult();

                var databasePath = UriFactory.CreateDatabaseUri(CosmosDbConstants.DocumentsDatabaseId);
                var documentCollection = new DocumentCollection
                {
                    Id = CosmosDbConstants.PdfDocumentsCollectionId,
                    PartitionKey = new PartitionKeyDefinition
                    {
                        Paths = new Collection<string> { $"/{nameof(PdfDocumentEntity.Id)}" }
                    }
                };

                documentClient
                    .CreateDocumentCollectionIfNotExistsAsync(databasePath, documentCollection)
                    .GetAwaiter()
                    .GetResult();
            }

            return applicationBuilder;
        }
    }
}