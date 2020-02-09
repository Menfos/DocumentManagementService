using System.IO;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using DocumentManagementService.Domain;
using DocumentManagementService.Domain.Dtos;
using DocumentManagementService.Logger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace DocumentManagementService.Controllers
{
    [Route("documentManagement/pdf")]
    [ApiController]
    public class PdfDocumentController : ControllerBase
    {
        private const string PdfDocumentAllowedSizeLimitKey = "PdfDocumentAllowedSizeLimitInBytes";

        private readonly IPdfDocumentHandler _pdfDocumentHandler;
        private readonly IConfiguration _configuration;
        private readonly IServiceLogger _logger;

        public PdfDocumentController(
            IPdfDocumentHandler pdfDocumentHandler,
            IConfiguration configuration,
            IServiceLogger logger)
        {
            _pdfDocumentHandler = pdfDocumentHandler;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Get available documents for downloading.
        /// </summary>
        /// <param name="orderBy">Optional parameter for ordering results.</param>
        /// <returns>Available documents.</returns>
        [HttpGet]
        public IActionResult GetAvailableDocuments([FromQuery]string orderBy = "")
        {
            var pdfDocuments = _pdfDocumentHandler.GetAvailablePdfDocuments(orderBy);
            return Ok(pdfDocuments);
        }

        /// <summary>
        /// Download specified pdf document.
        /// </summary>
        /// <param name="fileName">Pdf document name.</param>
        /// <returns>Pdf document content.</returns>
        [HttpGet("{fileName}")]
        public async Task<IActionResult> Download([FromRoute]string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                _logger.LogWarning("Attempt to download a file with empty name.");
                return BadRequest("File name should not be empty");
            }

            var downloadInfo = await _pdfDocumentHandler.DownloadAsync(fileName);
            if (downloadInfo.Status == HttpStatusCode.NotFound.ToString("G"))
            {
                _logger.LogWarning("Attempt to download not existing file.");
                return NotFound($"File with name '{fileName}' does not exist");
            }

            return File(downloadInfo.Content, downloadInfo.ContentType);
        }

        /// <summary>
        /// Upload new pdf document to the system.
        /// </summary>
        /// <param name="fileToUpload">Document upload information.</param>
        /// <returns>Uploaded pdf document.</returns>
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile fileToUpload)
        {
            if (fileToUpload == null)
            {
                _logger.LogWarning("Invalid file request");
                return BadRequest("File request is invalid");
            }

            if (fileToUpload.ContentType != MediaTypeNames.Application.Pdf)
            {
                _logger.LogWarning($"Uploaded content type '{fileToUpload.ContentType}' requested which is not pdf.");
                return BadRequest($"File '{fileToUpload.Name}' type is not pdf");
            }

            var pdfSizeLimit = _configuration.GetValue<long>(PdfDocumentAllowedSizeLimitKey);
            if (fileToUpload.Length > pdfSizeLimit)
            {
                _logger.LogWarning($"Uploaded file size '{fileToUpload.Length} KB' is exceeding threshold of allowed size limit {pdfSizeLimit} KB'");
                return BadRequest($"File '{fileToUpload.FileName}' size is more than {pdfSizeLimit} KB");
            }

            var downloadFilePath = Url.ActionLink(action: "Download", values: new { fileName = fileToUpload.FileName });

            await using var fileContentStream = fileToUpload.OpenReadStream();
            var uploadedData = await _pdfDocumentHandler
                .UploadAsync(new FileUploadInfoDto
                {
                    FileContent = fileContentStream,
                    FileName = Path.GetFileName(fileToUpload.FileName),
                    FileSizeInBytes = fileToUpload.Length,
                    DownloadFilePath = downloadFilePath
                });

            return Created(downloadFilePath, uploadedData);
        }

        /// <summary>
        /// Remove pdf document from the system.
        /// </summary>
        /// <param name="fileName">Pdf document name to remove.</param>
        /// <returns></returns>
        [HttpDelete("{fileName}")]
        public async Task<IActionResult> Delete([FromRoute]string fileName)
        {
            var removalInfo = await _pdfDocumentHandler.RemoveAsync(fileName);

            if (removalInfo.Status == HttpStatusCode.NotFound.ToString("G"))
            {
                _logger.LogWarning("Attempted to delete not existing file");
                return NotFound($"File with name '{fileName}' is not found");
            }

            return Accepted();
        }
    }
}
