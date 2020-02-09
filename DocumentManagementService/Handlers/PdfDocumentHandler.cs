using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentManagementService.BlobStorageService;
using DocumentManagementService.BlobStorageService.Models;
using DocumentManagementService.Common;
using DocumentManagementService.Common.Exceptions;
using DocumentManagementService.Data;
using DocumentManagementService.Data.CosmosDb.Entities;
using DocumentManagementService.Extensions;
using DocumentManagementService.Handlers.Dtos;
using Microsoft.AspNetCore.Http;

namespace DocumentManagementService.Handlers
{
    public class PdfDocumentHandler : IPdfDocumentHandler
    {
        private readonly IPdfDocumentsRepository _pdfDocumentRepository;
        private readonly IBlobStorageService _blobStorageService;

        public PdfDocumentHandler(IPdfDocumentsRepository pdfDocumentRepository, IBlobStorageService blobStorageService)
        {
            _pdfDocumentRepository = pdfDocumentRepository;
            _blobStorageService = blobStorageService;
        }

        public IEnumerable<PdfDocumentDto> GetAvailablePdfDocuments()
        {
            var pdfDocumentEntities = _pdfDocumentRepository.GetPdfDocuments();
            return pdfDocumentEntities
                .Select(x => new PdfDocumentDto
                {
                    Name = x.Id,
                    FileSize = $"{x.FileSize} {x.SizeMeasurement}",
                    Path = x.Path
                });
        }

        public async Task<DownloadInformationDto> DownloadAsync(string fileName)
        {
            var downloadResult = await _blobStorageService.DownloadFileAsync(fileName);

            return downloadResult == null
                ? null
                : new DownloadInformationDto
                {
                    ContentType = downloadResult.ContentType,
                    Content = downloadResult.Content
                };
        }

        public async Task<PdfDocumentDto> UploadAsync(IFormFile fileToUpload, string downloadFilePath)
        {
            await using (var fileStream = fileToUpload.OpenReadStream())
            {
                var isUploaded = await _blobStorageService.UploadFileToStorageAsync(fileToUpload.FileName, fileStream);

                if (!isUploaded)
                    throw new DocumentUploadException("Provided pdf document failed to upload");
            }

            var insertEntity = new PdfDocumentEntity
            {
                Id = fileToUpload.FileName,
                FileSize = fileToUpload.Length.ConvertBytesToMegabytes(),
                SizeMeasurement = SizeMeasurementType.MB.ToString("G"),
                Path = downloadFilePath
            };
            await _pdfDocumentRepository.InsertOrReplacePdfDocumentAsync(insertEntity);

            return new PdfDocumentDto
            {
                Name = fileToUpload.FileName,
                FileSize = $"{insertEntity.FileSize} {insertEntity.SizeMeasurement}",
                Path = downloadFilePath
            };
        }
    }
}