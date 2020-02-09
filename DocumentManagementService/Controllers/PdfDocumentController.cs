using System.Net.Mime;
using System.Security.Policy;
using System.Threading.Tasks;
using DocumentManagementService.Handlers;
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

        public PdfDocumentController(IPdfDocumentHandler pdfDocumentHandler, IConfiguration configuration)
        {
            _pdfDocumentHandler = pdfDocumentHandler;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var pdfDocuments = _pdfDocumentHandler.GetAvailablePdfDocuments();
            return Ok(pdfDocuments);
        }

        [HttpGet("download")]
        public async Task<IActionResult> Download([FromQuery]string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return BadRequest("File name should not be empty");

            var downloadInfo = await _pdfDocumentHandler.DownloadAsync(fileName);

            if (downloadInfo == null)
            {
                return NotFound($"File with name '{fileName}' does not exist");
            }

            return File(downloadInfo.Content, downloadInfo.ContentType);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile fileToUpload)
        {
            if (fileToUpload == null)
                return BadRequest("File request is invalid");

            if (fileToUpload.ContentType != MediaTypeNames.Application.Pdf)
                return BadRequest($"File '{fileToUpload.Name}' type is not pdf");

            var pdfSizeLimit = long.Parse(_configuration[PdfDocumentAllowedSizeLimitKey]);
            if (fileToUpload.Length > pdfSizeLimit)
                return BadRequest($"File '{fileToUpload.Name}' size is more than {pdfSizeLimit} KB");
            
            var downloadFilePath = Url.ActionLink(action: "Download", values: new { fileName = fileToUpload.FileName });
            var uploadedData = await _pdfDocumentHandler.UploadAsync(fileToUpload, downloadFilePath);

            return Created(downloadFilePath, uploadedData);
        }

        // PUT: api/PdfDocument/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
