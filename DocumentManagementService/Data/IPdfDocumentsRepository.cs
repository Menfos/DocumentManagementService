using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagementService.Data.CosmosDb.Entities;

namespace DocumentManagementService.Data
{
    public interface IPdfDocumentsRepository
    {
        Task<IEnumerable<PdfDocumentEntity>> GetPdfDocumentsAsync();

        Task InsertOrReplacePdfDocumentAsync(PdfDocumentEntity document);
    }
}