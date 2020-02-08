using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagementService.Handlers.Dtos;
using Microsoft.AspNetCore.Http;

namespace DocumentManagementService.Handlers
{
    public interface IPdfDocumentHandler
    {
        Task<IEnumerable<PdfDocumentDto>> GetAvailablePdfDocumentsAsync();

        Task<PdfDocumentDto> Upload(IFormFile fileToUpload);
    }
}