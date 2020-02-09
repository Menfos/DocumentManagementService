using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DocumentManagementService.Data.CosmosDb.ClientFactories;
using DocumentManagementService.Data.CosmosDb.Entities;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace DocumentManagementService.Data.CosmosDb.Initializers
{
    public class CosmosDataInitializer : ICosmosDataInitializer
    {
        private readonly ICosmosDocumentClientFactory _cosmosDocumentClientFactory;

        public CosmosDataInitializer(ICosmosDocumentClientFactory cosmosDocumentClientFactory)
        {
            _cosmosDocumentClientFactory = cosmosDocumentClientFactory;
        }

        public async Task InitializePdfDocumentCollectionIfNotExistsAsync()
        {
            var documentClient = _cosmosDocumentClientFactory.GetClient();
            var database = new Database { Id = CosmosDbConstants.DocumentsDatabaseId };
            await documentClient.CreateDatabaseIfNotExistsAsync(database);

            var databasePath = UriFactory.CreateDatabaseUri(CosmosDbConstants.DocumentsDatabaseId);
            var documentCollection = new DocumentCollection
            {
                Id = CosmosDbConstants.PdfDocumentsCollectionId,
                PartitionKey = new PartitionKeyDefinition
                {
                    Paths = new Collection<string> { "/contentType" }
                }
            };

            await documentClient.CreateDocumentCollectionIfNotExistsAsync(databasePath, documentCollection);
        }
    }
}