using System.Net.Mime;
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
        public async Task<IActionResult> Get()
        {
            var pdfDocuments = await _pdfDocumentHandler.GetAvailablePdfDocumentsAsync();
            return Ok(pdfDocuments);
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

            var uploadedData = await _pdfDocumentHandler.Upload(fileToUpload);
            return Created(uploadedData.Path, uploadedData);
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
