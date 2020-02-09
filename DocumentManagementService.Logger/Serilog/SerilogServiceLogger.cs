using System;
using DocumentManagementService.Logger.Serilog.Factories;
using Serilog.Events;

namespace DocumentManagementService.Logger.Serilog
{
    public class SerilogServiceLogger : IServiceLogger
    {
        private readonly ISerilogServiceLoggerFactory _loggerFactory;

        public SerilogServiceLogger(ISerilogServiceLoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public void LogInfo(string message)
        {
            var logger = _loggerFactory.GetLogger();
            logger.Write(LogEventLevel.Information, message);
        }

        public void LogWarning(string message, Exception exception = null)
        {
            var logger = _loggerFactory.GetLogger();
            if (exception == null)
                logger.Write(LogEventLevel.Warning, message);
            logger.Write(LogEventLevel.Warning, message, exception);
        }

        public void LogError(string message, Exception exception = null)
        {
            var logger = _loggerFactory.GetLogger();
            if (exception == null)
                logger.Write(LogEventLevel.Error, message);
            logger.Write(LogEventLevel.Error, message, exception);
        }
    }
}
