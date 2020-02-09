using System;
using Serilog;
using Serilog.Events;
using ISerilogLogger = Serilog.ILogger;
using SerilogLogger = Serilog.Core.Logger;

namespace DocumentManagementService.Logger.Serilog.Factories
{
    public class SerilogServiceLoggerFactory : ISerilogServiceLoggerFactory, IDisposable
    {
        private readonly SerilogLogger _serilogLogger;

        public SerilogServiceLoggerFactory()
        {
            _serilogLogger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
        }

        public SerilogServiceLoggerFactory(string logFilePath, RollingInterval fileRollingInterval)
        {
            _serilogLogger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(logFilePath, rollingInterval: fileRollingInterval)
                .CreateLogger();
        }

        public ISerilogLogger GetLogger() => _serilogLogger;

        public void Dispose()
        {
            _serilogLogger?.Dispose();
        }
    }
}