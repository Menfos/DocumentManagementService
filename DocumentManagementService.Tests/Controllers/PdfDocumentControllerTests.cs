using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using AutoFixture;
using DocumentManagementService.Controllers;
using DocumentManagementService.Domain;
using DocumentManagementService.Domain.Dtos;
using DocumentManagementService.Logger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Moq;
using Shouldly;
using Xunit;

namespace DocumentManagementService.Tests.Controllers
{
    public class PdfDocumentControllerTests
    {
        private const string PdfDocumentAllowedSizeLimitKey = "PdfDocumentAllowedSizeLimitInBytes";

        private readonly Mock<IUrlHelper> _urlHelperMock;
        private readonly Mock<IPdfDocumentHandler> _documentHandlerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IServiceLogger> _serviceLoggerMock;

        private readonly IFixture _fixture;

        private readonly PdfDocumentController _sut;

        public PdfDocumentControllerTests()
        {
            _urlHelperMock = new Mock<IUrlHelper>();
            _documentHandlerMock = new Mock<IPdfDocumentHandler>();
            _configurationMock = new Mock<IConfiguration>();
            _serviceLoggerMock = new Mock<IServiceLogger>();

            _fixture = new Fixture();

            _sut = new PdfDocumentController(_documentHandlerMock.Object, _configurationMock.Object, _serviceLoggerMock.Object)
            {
                Url = _urlHelperMock.Object
            };
        }

        [Fact]
        public void GetAvailableDocuments_ReturnedOkResponseWithAvailableDocuments()
        {
            //Arrange
            var expectedAvailableDocuments = _fixture
                .CreateMany<DocumentDto>();

            _documentHandlerMock
                .Setup(handler => handler.GetAvailablePdfDocuments(It.IsAny<string>()))
                .Returns(expectedAvailableDocuments);

            //Act
            var actionResult = _sut.GetAvailableDocuments();

            //Assert
            var okObjectResult = actionResult.ShouldBeOfType<OkObjectResult>();
            var actualAvailableObjects = okObjectResult.Value as IEnumerable<DocumentDto>;
            actualAvailableObjects.ShouldNotBeNull();
            actualAvailableObjects.ShouldBe(expectedAvailableDocuments);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task Download_EmptyFileName_ReturnedBadRequestResponse(string fileName)
        {
            //Act
            var actionResult = await _sut.Download(fileName);

            //Assert
            actionResult.ShouldBeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Download_NotExistingFile_ReturnedNotFoundResposne()
        {
            //Arrange
            var fileName = _fixture.Create<string>();

            var expectedDownloadInformation = _fixture
                .Build<DownloadInformationDto>()
                .With(dto => dto.Status, HttpStatusCode.NotFound.ToString("G"))
                .Without(dto => dto.Content)
                .Create();
            _documentHandlerMock
                .Setup(handler => handler.DownloadAsync(fileName))
                .ReturnsAsync(expectedDownloadInformation);

            //Act
            var actionResult = await _sut.Download(fileName);

            //Assert
            actionResult.ShouldBeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Download_ReturnedFileResponse()
        {
            //Arrange
            var fileName = _fixture.Create<string>();

            await using var fileStream = new MemoryStream();
            var expectedDownloadInformation = _fixture
                .Build<DownloadInformationDto>()
                .With(dto => dto.Content, fileStream)
                .With(dto => dto.ContentType, MediaTypeNames.Application.Pdf)
                .Create();
            _documentHandlerMock
                .Setup(handler => handler.DownloadAsync(fileName))
                .ReturnsAsync(expectedDownloadInformation);

            //Act
            var actionResult = await _sut.Download(fileName);

            //Assert
            var fileStreamResult = actionResult.ShouldBeOfType<FileStreamResult>();
            fileStreamResult.ContentType.ShouldBe(expectedDownloadInformation.ContentType);
            fileStreamResult.FileStream.ShouldBe(expectedDownloadInformation.Content);
        }

        [Fact]
        public async Task Upload_NullFileToUpload_ReturnedBadRequestResponse()
        {
            //Act
            var actionResult = await _sut.Upload(null);

            //Assert
            actionResult.ShouldBeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Upload_InvalidContentType_ReturnedBadRequestResponse()
        {
            //Arrange
            var formFileMock = new Mock<IFormFile>();
            formFileMock
                .SetupGet(file => file.ContentType)
                .Returns("test");

            //Act
            var actionResult = await _sut.Upload(formFileMock.Object);

            //Assert
            actionResult.ShouldBeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Upload_FileSizeExceededSizeLimit_ReturnedBadRequestResponse()
        {
            //Arrange
            var fileSize = 5000;
            var formFileMock = new Mock<IFormFile>();
            formFileMock
                .SetupGet(file => file.ContentType)
                .Returns(MediaTypeNames.Application.Pdf);
            formFileMock
                .SetupGet(file => file.Length)
                .Returns(fileSize);

            var fileSizeLimit = 1000;
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock
                .SetupGet(section => section.Value)
                .Returns(fileSizeLimit.ToString());
            _configurationMock
                .Setup(c => c.GetSection(PdfDocumentAllowedSizeLimitKey))
                .Returns(configurationSectionMock.Object);

            //Act
            var actionResult = await _sut.Upload(formFileMock.Object);

            //Assert
            actionResult.ShouldBeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Upload_ReturnedCreatedResponse()
        {
            //Arrange
            var fileSize = 5000;
            var fileName = _fixture.Create<string>();
            await using var fileReadStream = new MemoryStream();
            var formFileMock = new Mock<IFormFile>();
            formFileMock
                .SetupGet(file => file.ContentType)
                .Returns(MediaTypeNames.Application.Pdf);
            formFileMock
                .SetupGet(file => file.Length)
                .Returns(fileSize);
            formFileMock
                .SetupGet(file => file.FileName)
                .Returns(fileName);
            formFileMock
                .Setup(file => file.OpenReadStream())
                .Returns(fileReadStream);

            var fileSizeLimit = 6000;
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock
                .SetupGet(section => section.Value)
                .Returns(fileSizeLimit.ToString());
            _configurationMock
                .Setup(c => c.GetSection(PdfDocumentAllowedSizeLimitKey))
                .Returns(configurationSectionMock.Object);

            var expectedFileUploadLocation = _fixture.Create<string>();
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            _urlHelperMock
                .SetupGet(helper => helper.ActionContext)
                .Returns(actionContext);
            _urlHelperMock
                .Setup(helper => helper.Action(It.IsAny<UrlActionContext>()))
                .Returns(expectedFileUploadLocation);

            var expectedPdfDocumentUploadInfo = _fixture.Create<DocumentDto>();
            _documentHandlerMock
                .Setup(handler => handler.UploadAsync(It.IsAny<FileUploadInfoDto>()))
                .ReturnsAsync(expectedPdfDocumentUploadInfo);

            //Act
            var actionResult = await _sut.Upload(formFileMock.Object);

            //Assert
            var createdResult = actionResult.ShouldBeOfType<CreatedResult>();
            createdResult.Location.ShouldBe(expectedFileUploadLocation);
            var actualPdfDocumentUploadInfo = createdResult.Value as DocumentDto;
            actualPdfDocumentUploadInfo.ShouldNotBeNull();
            actualPdfDocumentUploadInfo.ShouldBe(expectedPdfDocumentUploadInfo);
        }

        [Fact]
        public async Task Delete_NotExistingFile_ReturnedNotFoundResponse()
        {
            //Arrange
            var fileName = _fixture.Create<string>();
            var removalInfo = _fixture.Build<RemovalInfo>()
                .With(info => info.Status, HttpStatusCode.NotFound.ToString("G"))
                .Create();
            _documentHandlerMock
                .Setup(handler => handler.RemoveAsync(fileName))
                .ReturnsAsync(removalInfo);

            //Act
            var actionResult = await _sut.Delete(fileName);

            //Assert
            actionResult.ShouldBeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnedAcceptedResponse()
        {
            //Arrange
            var fileName = _fixture.Create<string>();
            var removalInfo = _fixture.Create<RemovalInfo>();
            _documentHandlerMock
                .Setup(handler => handler.RemoveAsync(fileName))
                .ReturnsAsync(removalInfo);

            //Act
            var actionResult = await _sut.Delete(fileName);

            //Assert
            actionResult.ShouldBeOfType<AcceptedResult>();
        }
    }
}