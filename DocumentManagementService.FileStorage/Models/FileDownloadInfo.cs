using System.IO;

namespace DocumentManagementService.FileStorage.Models
{
    public class FileDownloadInfo
    {
        public string Status { get; set; }

        public string ContentType { get; set; }

        public Stream Content { get; set; }
    }
}