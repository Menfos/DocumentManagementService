using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagementService.Data.CosmosDb;
using DocumentManagementService.Data.CosmosDb.Entities;

namespace DocumentManagementService.Data
{
    public interface IPdfDocumentsRepository
    {
        IEnumerable<DocumentEntity> GetPdfDocuments(OrderType orderBy = OrderType.Name);

        Task InsertOrReplacePdfDocumentAsync(DocumentEntity document);

        Task RemovePdfDocumentAsync(string documentId);
    }
}