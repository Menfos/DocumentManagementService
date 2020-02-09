using System;
using System.Threading.Tasks;
using DocumentManagementService.Data.CosmosDb.ClientFactories;
using DocumentManagementService.Data.CosmosDb.Initializers;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using Xunit;

namespace DocumentManagementService.Data.Tests.CosmosDb.Initializers
{
    public class CosmosDataInitializerTests
    {
        private readonly Mock<ICosmosDocumentClientFactory> _documentClientFactoryMock;

        private readonly ICosmosDataInitializer _sut;

        public CosmosDataInitializerTests()
        {
            _documentClientFactoryMock = new Mock<ICosmosDocumentClientFactory>();

            _sut = new CosmosDataInitializer(_documentClientFactoryMock.Object);
        }

        [Fact]
        public async Task InitializePdfDocumentCollectionIfNotExistsAsync()
        {
            //Arrange
            var documentClientMock = new Mock<IDocumentClient>();
            _documentClientFactoryMock
                .Setup(factory => factory.GetClient())
                .Returns(documentClientMock.Object);
            //Act
            await _sut.InitializePdfDocumentCollectionIfNotExistsAsync();

            //Assert
            documentClientMock
                .Verify(client => client.CreateDatabaseIfNotExistsAsync(
                        It.IsAny<Database>(),
                        It.IsAny<RequestOptions>()),
                    Times.Once);

            documentClientMock
                .Verify(client => client.CreateDocumentCollectionIfNotExistsAsync(
                        It.IsAny<Uri>(),
                        It.IsAny<DocumentCollection>(),
                        It.IsAny<RequestOptions>()),
                    Times.Once);
        }
    }
}