using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagementService.Domain.Dtos;

namespace DocumentManagementService.Domain
{
    public interface IPdfDocumentHandler
    {
        IEnumerable<DocumentDto> GetAvailablePdfDocuments(string orderBy);

        Task<DownloadInformationDto> DownloadAsync(string fileName);

        Task<DocumentDto> UploadAsync(FileUploadInfoDto fileUploadInfo);

        Task<RemovalInfo> RemoveAsync(string fileName);
    }
}