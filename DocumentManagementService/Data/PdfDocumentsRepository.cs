using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentManagementService.Data.CosmosDb;
using DocumentManagementService.Data.CosmosDb.ClientFactories;
using DocumentManagementService.Data.CosmosDb.Entities;
using Microsoft.Azure.Documents.Client;

namespace DocumentManagementService.Data
{
    public class PdfDocumentsRepository : IPdfDocumentsRepository
    {
        private readonly ICosmosDocumentClientFactory _documentClientFactory;

        public PdfDocumentsRepository(ICosmosDocumentClientFactory documentClientFactory)
        {
            _documentClientFactory = documentClientFactory;
        }

        public IEnumerable<PdfDocumentEntity> GetPdfDocuments()
        {
            var documentClient = _documentClientFactory.GetClient();
            var documentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                CosmosDbConstants.DocumentsDatabaseId,
                CosmosDbConstants.PdfDocumentsCollectionId);

            return documentClient
                .CreateDocumentQuery<PdfDocumentEntity>(documentCollectionUri)
                .ToList();
        }

        public async Task InsertOrReplacePdfDocumentAsync(PdfDocumentEntity document)
        {
            var documentClient = _documentClientFactory.GetClient();
            var documentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                CosmosDbConstants.DocumentsDatabaseId,
                CosmosDbConstants.PdfDocumentsCollectionId);

            await documentClient.UpsertDocumentAsync(documentCollectionUri, document, disableAutomaticIdGeneration: true);
        }
    }
}