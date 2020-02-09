using System.IO;

namespace DocumentManagementService.Domain.Dtos
{
    public class FileUploadInfoDto
    {
        public Stream FileContent { get; set; }

        public string FileName { get; set; }

        public long FileSizeInBytes { get; set; }

        public string DownloadFilePath { get; set; }
    }
}