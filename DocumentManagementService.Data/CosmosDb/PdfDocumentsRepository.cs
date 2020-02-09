using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using DocumentManagementService.Data.CosmosDb.ClientFactories;
using DocumentManagementService.Data.CosmosDb.Entities;
using DocumentManagementService.Logger;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace DocumentManagementService.Data.CosmosDb
{
    public class PdfDocumentsRepository : IPdfDocumentsRepository
    {
        private readonly ICosmosDocumentClientFactory _documentClientFactory;
        private readonly IServiceLogger _serviceLogger;

        public PdfDocumentsRepository(ICosmosDocumentClientFactory documentClientFactory, IServiceLogger serviceLogger)
        {
            _documentClientFactory = documentClientFactory;
            _serviceLogger = serviceLogger;
        }

        public IEnumerable<DocumentEntity> GetPdfDocuments(OrderType orderBy = OrderType.Name)
        {
            var documentClient = _documentClientFactory.GetClient();
            var documentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                CosmosDbConstants.DocumentsDatabaseId,
                CosmosDbConstants.PdfDocumentsCollectionId);

            var query = documentClient.CreateDocumentQuery<DocumentEntity>(
                documentCollectionUri,
                new FeedOptions { EnableCrossPartitionQuery = true });
            query = orderBy switch
            {
                OrderType.Name => query.OrderBy(documentEntity => documentEntity.Id),
                OrderType.Path => query.OrderBy(documentEntity => documentEntity.Path),
                OrderType.Size => query.OrderBy(documentEntity => documentEntity.FileSizeInKilobytes),
                _ => query
            };

            return query.ToList();
        }

        public async Task InsertOrReplacePdfDocumentAsync(DocumentEntity document)
        {
            var documentClient = _documentClientFactory.GetClient();
            var documentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                CosmosDbConstants.DocumentsDatabaseId,
                CosmosDbConstants.PdfDocumentsCollectionId);

            _serviceLogger.LogInfo($"Upserting '{document.Id}' to data storage");
            await documentClient.UpsertDocumentAsync(documentCollectionUri, document, disableAutomaticIdGeneration: true);
        }

        public async Task RemovePdfDocumentAsync(string documentId)
        {
            var documentClient = _documentClientFactory.GetClient();
            var documentCollectionUri = UriFactory.CreateDocumentUri(
                CosmosDbConstants.DocumentsDatabaseId,
                CosmosDbConstants.PdfDocumentsCollectionId,
                documentId);

            _serviceLogger.LogInfo($"Removing '{documentId}' from data storage");
            await documentClient.DeleteDocumentAsync(
                documentCollectionUri,
                new RequestOptions
                {
                    PartitionKey = new PartitionKey(MediaTypeNames.Application.Pdf)
                });
        }
    }
}