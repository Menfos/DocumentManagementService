using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using DocumentManagementService.Data.CosmosDb;
using DocumentManagementService.Data.CosmosDb.ClientFactories;
using DocumentManagementService.Data.CosmosDb.Entities;
using DocumentManagementService.Logger;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using Shouldly;
using Xunit;

namespace DocumentManagementService.Data.Tests.CosmosDb
{
    public class PdfDocumentsRepositoryTests
    {
        private readonly Mock<ICosmosDocumentClientFactory> _documentClientFactoryMock;
        private readonly Mock<IServiceLogger> _serviceLoggerMock;

        private readonly IFixture _fixture;

        private readonly IPdfDocumentsRepository _sut;

        public PdfDocumentsRepositoryTests()
        {
            _documentClientFactoryMock = new Mock<ICosmosDocumentClientFactory>();
            _serviceLoggerMock = new Mock<IServiceLogger>();

            _fixture = new Fixture();

            _sut = new PdfDocumentsRepository(_documentClientFactoryMock.Object, _serviceLoggerMock.Object);
        }

        [Theory]
        [InlineData(OrderType.Path)]
        [InlineData(OrderType.Name)]
        [InlineData(OrderType.Size)]
        public void GetPdfDocuments_OrderByType_ReturnsDocumentEntities(OrderType orderType)
        {
            //Arrange
            var documentEntities = _fixture
                .CreateMany<DocumentEntity>()
                .ToList();

            var orderedQueryable = documentEntities.AsQueryable().OrderByDescending(entity => entity.Id);
            var documentClientMock = new Mock<IDocumentClient>();
            documentClientMock
                .Setup(client => client.CreateDocumentQuery<DocumentEntity>(It.IsAny<Uri>(), It.IsAny<FeedOptions>()))
                .Returns(orderedQueryable);
            _documentClientFactoryMock
                .Setup(factory => factory.GetClient())
                .Returns(documentClientMock.Object);

            List<DocumentEntity> expectedResult = null;
            switch (orderType)
            {
                case OrderType.Name:
                    expectedResult = documentEntities.OrderBy(documentEntity => documentEntity.Id).ToList();
                    break;
                case OrderType.Path:
                    expectedResult = documentEntities.OrderBy(documentEntity => documentEntity.Path).ToList();
                    break;
                case OrderType.Size:
                    expectedResult = documentEntities.OrderBy(documentEntity => documentEntity.FileSizeInKilobytes).ToList();
                    break;
            }

            //Act
            var actualDocumentEntities = _sut.GetPdfDocuments(orderType);

            //Assert
            actualDocumentEntities.ShouldBe(expectedResult);
        }

        [Fact]
        public async Task InsertOrReplacePdfDocumentAsync_DocumentUpserted()
        {
            //Arrange
            var documentEntity = _fixture.Create<DocumentEntity>();

            var documentClientMock = new Mock<IDocumentClient>();
            _documentClientFactoryMock
                .Setup(factory => factory.GetClient())
                .Returns(documentClientMock.Object);

            //Act
            await _sut.InsertOrReplacePdfDocumentAsync(documentEntity);

            //Assert
            documentClientMock.Verify(
                client => client.UpsertDocumentAsync(
                    It.IsAny<Uri>(),
                    It.IsAny<object>(),
                    It.IsAny<RequestOptions>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task RemovePdfDocumentAsync_DocumentRemoved()
        {
            //Arrange
            var documentId = _fixture.Create<string>();

            var documentClientMock = new Mock<IDocumentClient>();
            _documentClientFactoryMock
                .Setup(factory => factory.GetClient())
                .Returns(documentClientMock.Object);

            //Act
            await _sut.RemovePdfDocumentAsync(documentId);

            //Assert
            documentClientMock.Verify(
                client => client.DeleteDocumentAsync(
                    It.IsAny<Uri>(),
                    It.IsAny<RequestOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}