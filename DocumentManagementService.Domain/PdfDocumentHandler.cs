using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using DocumentManagementService.Data;
using DocumentManagementService.Data.CosmosDb;
using DocumentManagementService.Data.CosmosDb.Entities;
using DocumentManagementService.Domain.Dtos;
using DocumentManagementService.Domain.Exceptions;
using DocumentManagementService.FileStorage;
using DocumentManagementService.Logger;

namespace DocumentManagementService.Domain
{
    public class PdfDocumentHandler : IPdfDocumentHandler
    {
        private readonly IPdfDocumentsRepository _pdfDocumentRepository;
        private readonly IFileStorageHandler _fileStorageHandler;
        private readonly IServiceLogger _logger;

        public PdfDocumentHandler(
            IPdfDocumentsRepository pdfDocumentRepository,
            IFileStorageHandler fileStorageHandler,
            IServiceLogger logger)
        {
            _pdfDocumentRepository = pdfDocumentRepository;
            _fileStorageHandler = fileStorageHandler;
            _logger = logger;
        }

        public IEnumerable<DocumentDto> GetAvailablePdfDocuments(string orderBy)
        {
            var pdfDocumentEntities = Enum.IsDefined(typeof(OrderType), orderBy)
                ? _pdfDocumentRepository.GetPdfDocuments(Enum.Parse<OrderType>(orderBy))
                : _pdfDocumentRepository.GetPdfDocuments();

            return pdfDocumentEntities
                .Select(documentEntity => new DocumentDto
                {
                    Name = documentEntity.Id,
                    FileSize = $"{documentEntity.FileSizeInKilobytes / 1000} MB",
                    Path = documentEntity.Path
                });
        }

        public async Task<DownloadInformationDto> DownloadAsync(string fileName)
        {
            var downloadResult = await _fileStorageHandler.DownloadFileAsync(fileName);
            return new DownloadInformationDto
            {
                Status = downloadResult.Status,
                ContentType = downloadResult.ContentType,
                Content = downloadResult.Content
            };
        }

        public async Task<DocumentDto> UploadAsync(FileUploadInfoDto fileUploadInfo)
        {
            var isUploaded = await _fileStorageHandler.UploadFileToStorageAsync(fileUploadInfo.FileName, fileUploadInfo.FileContent);
            if (!isUploaded)
            {
                _logger.LogError($"File storage failed to upload '{fileUploadInfo.FileName}' document");
                throw new DocumentUploadException("Provided pdf document failed to upload");
            }

            var insertEntity = new DocumentEntity
            {
                Id = fileUploadInfo.FileName,
                ContentType = MediaTypeNames.Application.Pdf,
                FileSizeInKilobytes = fileUploadInfo.FileSizeInBytes,
                Path = fileUploadInfo.DownloadFilePath
            };
            await _pdfDocumentRepository.InsertOrReplacePdfDocumentAsync(insertEntity);

            return new DocumentDto
            {
                Name = fileUploadInfo.FileName,
                FileSize = $"{fileUploadInfo.FileSizeInBytes / 1000} MB",
                Path = fileUploadInfo.DownloadFilePath
            };
        }

        public async Task<RemovalInfo> RemoveAsync(string fileName)
        {
            var removalInfo = await _fileStorageHandler.RemoveFileFromStorageAsync(fileName);
            if (removalInfo.IsRemoved)
                await _pdfDocumentRepository.RemovePdfDocumentAsync(fileName);

            return new RemovalInfo { Status = removalInfo.Status };
        }
    }
}