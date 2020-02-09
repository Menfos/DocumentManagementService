using System.IO;

namespace DocumentManagementService.BlobStorageService.Models
{
    public class BlobDownloadInfo
    {
        public string ContentType { get; set; }

        public Stream Content { get; set; }
    }
}