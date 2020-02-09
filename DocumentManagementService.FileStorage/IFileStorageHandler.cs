using System.IO;
using System.Threading.Tasks;
using DocumentManagementService.FileStorage.Models;

namespace DocumentManagementService.FileStorage
{
    public interface IFileStorageHandler
    {
        Task<FileDownloadInfo> DownloadFileAsync(string fileName);

        Task<bool> UploadFileToStorageAsync(string fileName, Stream fileStream);

        Task<FileRemovalInfo> RemoveFileFromStorageAsync(string fileName);
    }
}