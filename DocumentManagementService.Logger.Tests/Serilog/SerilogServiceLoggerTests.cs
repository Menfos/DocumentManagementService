using AutoFixture;
using DocumentManagementService.Logger.Serilog;
using DocumentManagementService.Logger.Serilog.Factories;
using Moq;
using Serilog.Events;
using Xunit;
using ISerilogLogger = Serilog.ILogger;

namespace DocumentManagementService.Logger.Tests.Serilog
{
    public class SerilogServiceLoggerTests
    {
        private readonly Mock<ISerilogServiceLoggerFactory> _serviceLoggerFactoryMock;

        private readonly IFixture _fixture;

        private readonly IServiceLogger _sut;

        public SerilogServiceLoggerTests()
        {
            _serviceLoggerFactoryMock = new Mock<ISerilogServiceLoggerFactory>();

            _fixture = new Fixture();

            _sut = new SerilogServiceLogger(_serviceLoggerFactoryMock.Object);
        }

        [Fact]
        public void LogInfo_LogMessageSentWithInformationLogLevel()
        {
            //Arrange
            var message = _fixture.Create<string>();
            var serilogLoggerMock = new Mock<ISerilogLogger>();
            _serviceLoggerFactoryMock
                .Setup(factory => factory.GetLogger())
                .Returns(serilogLoggerMock.Object);

            //Act
            _sut.LogInfo(message);

            //Assert
            serilogLoggerMock
                .Verify(logger => logger.Write(LogEventLevel.Information, message), Times.Once);
        }

        [Fact]
        public void LogWarning_LogMessageSentWithWarningLogLevel()
        {
            //Arrange
            var message = _fixture.Create<string>();
            var serilogLoggerMock = new Mock<ISerilogLogger>();
            _serviceLoggerFactoryMock
                .Setup(factory => factory.GetLogger())
                .Returns(serilogLoggerMock.Object);

            //Act
            _sut.LogWarning(message);

            //Assert
            serilogLoggerMock
                .Verify(logger => logger.Write(LogEventLevel.Warning, message), Times.Once);
        }

        [Fact]
        public void LogError_LogMessageSentWithErrorLogLevel()
        {
            //Arrange
            var message = _fixture.Create<string>();
            var serilogLoggerMock = new Mock<ISerilogLogger>();
            _serviceLoggerFactoryMock
                .Setup(factory => factory.GetLogger())
                .Returns(serilogLoggerMock.Object);

            //Act
            _sut.LogError(message);

            //Assert
            serilogLoggerMock
                .Verify(logger => logger.Write(LogEventLevel.Error, message), Times.Once);
        }
    }
}
