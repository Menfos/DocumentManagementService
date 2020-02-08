using System.IO;
using System.Threading.Tasks;
using DocumentManagementService.BlobStorageService.Models;

namespace DocumentManagementService.BlobStorageService
{
    public interface IBlobStorageService
    {
        Task<BlobUploadResult> UploadFileToStorageAsync(string fileName, Stream fileStream);
    }
}