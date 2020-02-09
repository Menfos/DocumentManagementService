using System.IO;

namespace DocumentManagementService.Domain.Dtos
{
    public class DownloadInformationDto
    {
        public string Status { get; set; }

        public string ContentType { get; set; }

        public Stream Content { get; set; }
    }
}