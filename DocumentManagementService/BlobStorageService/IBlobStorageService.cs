using System.IO;
using System.Threading.Tasks;
using DocumentManagementService.BlobStorageService.Models;

namespace DocumentManagementService.BlobStorageService
{
    public interface IBlobStorageService
    {
        Task<BlobDownloadInfo> DownloadFileAsync(string fileName);

        Task<bool> UploadFileToStorageAsync(string fileName, Stream fileStream);
    }
}