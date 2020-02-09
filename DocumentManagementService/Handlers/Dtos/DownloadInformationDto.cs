using System.IO;

namespace DocumentManagementService.Handlers.Dtos
{
    public class DownloadInformationDto
    {
        public string ContentType { get; set; }

        public Stream Content { get; set; }
    }
}