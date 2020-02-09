using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagementService.Data.CosmosDb.Entities;

namespace DocumentManagementService.Data
{
    public interface IPdfDocumentsRepository
    {
        IEnumerable<PdfDocumentEntity> GetPdfDocuments();

        Task InsertOrReplacePdfDocumentAsync(PdfDocumentEntity document);
    }
}