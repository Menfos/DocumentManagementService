using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagementService.Handlers.Dtos;
using Microsoft.AspNetCore.Http;

namespace DocumentManagementService.Handlers
{
    public interface IPdfDocumentHandler
    {
        IEnumerable<PdfDocumentDto> GetAvailablePdfDocuments();

        Task<DownloadInformationDto> DownloadAsync(string fileName);

        Task<PdfDocumentDto> UploadAsync(IFormFile fileToUpload, string downloadFilePath);
    }
}