using ISerilogLogger = Serilog.ILogger;

namespace DocumentManagementService.Logger.Serilog.Factories
{
    public interface ISerilogServiceLoggerFactory
    {
        ISerilogLogger GetLogger();
    }
}