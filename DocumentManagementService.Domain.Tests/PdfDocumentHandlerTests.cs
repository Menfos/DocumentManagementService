using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using DocumentManagementService.Data;
using DocumentManagementService.Data.CosmosDb;
using DocumentManagementService.Data.CosmosDb.Entities;
using DocumentManagementService.Domain.Dtos;
using DocumentManagementService.Domain.Exceptions;
using DocumentManagementService.FileStorage;
using DocumentManagementService.FileStorage.Models;
using DocumentManagementService.Logger;
using Moq;
using Shouldly;
using Xunit;

namespace DocumentManagementService.Domain.Tests
{
    public class PdfDocumentHandlerTests
    {
        private Mock<IPdfDocumentsRepository> _documentsRepositoryMock;
        private Mock<IFileStorageHandler> _fileStorageHandlerMock;
        private Mock<IServiceLogger> _serviceLoggerMock;

        private readonly IFixture _fixture;

        private readonly IPdfDocumentHandler _sut;

        public PdfDocumentHandlerTests()
        {
            _documentsRepositoryMock = new Mock<IPdfDocumentsRepository>();
            _fileStorageHandlerMock = new Mock<IFileStorageHandler>();
            _serviceLoggerMock = new Mock<IServiceLogger>();

            _fixture = new Fixture();

            _sut = new PdfDocumentHandler(
                _documentsRepositoryMock.Object,
                _fileStorageHandlerMock.Object,
                _serviceLoggerMock.Object);
        }

        [Theory]
        [InlineData("")]
        [InlineData("1")]
        [InlineData("test")]
        public void GetAvailablePdfDocuments_UseDefaultOrder_ReturnsDocumentEntities(string orderBy)
        {
            //Arrange
            var expectedPdfDocuments = _fixture.CreateMany<DocumentEntity>();
            _documentsRepositoryMock
                .Setup(repository => repository.GetPdfDocuments(OrderType.Name))
                .Returns(expectedPdfDocuments);

            //Act
            var actualPdfDocuments = _sut.GetAvailablePdfDocuments(orderBy);

            //Assert
            actualPdfDocuments.Count()
                .ShouldBe(expectedPdfDocuments.Count());
            foreach (var actualPdfDocument in actualPdfDocuments)
            {
                var expectedPdfDocument = expectedPdfDocuments
                    .FirstOrDefault(entity => entity.Id == actualPdfDocument.Name);
                expectedPdfDocument.ShouldNotBeNull();
                actualPdfDocument.FileSize.ShouldBe($"{expectedPdfDocument.FileSizeInKilobytes / 1000} MB");
                actualPdfDocument.Path.ShouldBe(expectedPdfDocument.Path);
            }
        }

        [Theory]
        [InlineData("Path")]
        [InlineData("Name")]
        [InlineData("Size")]
        public void GetAvailablePdfDocuments_UseCustomOrder_ReturnsDocumentEntities(string orderBy)
        {
            //Arrange
            var expectedPdfDocuments = _fixture.CreateMany<DocumentEntity>();
            _documentsRepositoryMock
                .Setup(repository => repository.GetPdfDocuments(Enum.Parse<OrderType>(orderBy)))
                .Returns(expectedPdfDocuments);

            //Act
            var actualPdfDocuments = _sut.GetAvailablePdfDocuments(orderBy);

            //Assert
            actualPdfDocuments.Count()
                .ShouldBe(expectedPdfDocuments.Count());
            foreach (var actualPdfDocument in actualPdfDocuments)
            {
                var expectedPdfDocument = expectedPdfDocuments
                    .FirstOrDefault(entity => entity.Id == actualPdfDocument.Name);
                expectedPdfDocument.ShouldNotBeNull();
                actualPdfDocument.FileSize.ShouldBe($"{expectedPdfDocument.FileSizeInKilobytes / 1000} MB");
                actualPdfDocument.Path.ShouldBe(expectedPdfDocument.Path);
            }
        }

        [Fact]
        public async Task DownloadAsync_ReturnsDownloadInformation()
        {
            //Arrange
            await using var fileContentStream = new MemoryStream();
            var expectedDownloadInformation = _fixture.Build<FileDownloadInfo>()
                .With(info => info.Content, fileContentStream)
                .Create();
            var fileName = _fixture.Create<string>();
            _fileStorageHandlerMock
                .Setup(handler => handler.DownloadFileAsync(fileName))
                .ReturnsAsync(expectedDownloadInformation);

            //Act
            var actualDownloadInformation = await _sut.DownloadAsync(fileName);

            //Assert
            actualDownloadInformation.Status.ShouldBe(expectedDownloadInformation.Status);
            actualDownloadInformation.Content.ShouldBe(expectedDownloadInformation.Content);
            actualDownloadInformation.ContentType.ShouldBe(expectedDownloadInformation.ContentType);
        }

        [Fact]
        public async Task UploadAsync_UploadToFileStorageFailed_DocumentUploadExceptionIsThrown()
        {
            //Arrange
            await using var fileContentStream = new MemoryStream();
            var fileUploadDto = _fixture.Build<FileUploadInfoDto>()
                .With(dto => dto.FileContent, fileContentStream)
                .Create();

            _fileStorageHandlerMock
                .Setup(handler => handler.UploadFileToStorageAsync(fileUploadDto.FileName, fileUploadDto.FileContent))
                .ReturnsAsync(false);

            //Act
            await Should.ThrowAsync<DocumentUploadException>(_sut.UploadAsync(fileUploadDto));
        }

        [Fact]
        public async Task UploadAsync_ReturnsUploadedPdfDocument()
        {
            //Arrange
            await using var fileContentStream = new MemoryStream();
            var fileUploadDto = _fixture.Build<FileUploadInfoDto>()
                .With(dto => dto.FileContent, fileContentStream)
                .Create();

            _fileStorageHandlerMock
                .Setup(handler => handler.UploadFileToStorageAsync(fileUploadDto.FileName, fileUploadDto.FileContent))
                .ReturnsAsync(true);

            //Act
            var actualUploadedDocumentInformation = await _sut.UploadAsync(fileUploadDto);

            //Assert
            actualUploadedDocumentInformation.Name.ShouldBe(fileUploadDto.FileName);
            actualUploadedDocumentInformation.FileSize.ShouldBe($"{fileUploadDto.FileSizeInBytes / 1000} MB");
            actualUploadedDocumentInformation.Path.ShouldBe(fileUploadDto.DownloadFilePath);

            _documentsRepositoryMock.Verify(
                repository => repository.InsertOrReplacePdfDocumentAsync(It.IsAny<DocumentEntity>()),
                Times.Once);

        }

        [Fact]
        public async Task RemoveAsync_ReturnsRemovalInfo()
        {
            //Arrange
            var fileName = _fixture.Create<string>();
            var fileRemovalInfo = _fixture.Build<FileRemovalInfo>()
                .With(info => info.IsRemoved, true)
                .Create();
            _fileStorageHandlerMock
                .Setup(handler => handler.RemoveFileFromStorageAsync(fileName))
                .ReturnsAsync(fileRemovalInfo);

            //Act
            var actualRemovalInfo = await _sut.RemoveAsync(fileName);

            //Assert
            actualRemovalInfo.Status.ShouldBe(fileRemovalInfo.Status);
            _documentsRepositoryMock
                .Verify(repository => repository.RemovePdfDocumentAsync(fileName), Times.Once);
        }
    }
}