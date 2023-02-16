using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;

namespace MG.Utils.Elk.Logging
{
    public class FileLoggerBuilder : ILoggerBuilder
    {
        private readonly LoggingMessageTemplate _messageTemplate;

        private readonly WriteToFilesConfig _writeToFiles;

        private readonly IConfiguration _configuration;

        private Logger _logger;

        private Exception _exception;

        public FileLoggerBuilder(
            LoggingMessageTemplate messageTemplate, WriteToFilesConfig writeToFiles, IConfiguration configuration)
        {
            _messageTemplate = messageTemplate;
            _writeToFiles = writeToFiles;
            _configuration = configuration;
        }

        public Logger Logger()
        {
            if (!IsCreated())
            {
                throw new InvalidOperationException(ExceptionMessage.LoggerNotCreated);
            }

            return _logger;
        }

        public Exception ThrownException()
        {
            if (_exception == null)
            {
                throw new InvalidOperationException(ExceptionMessage.LoggerNotInitiated);
            }

            return _exception;
        }

        public ILoggerBuilder TryCreateLogger()
        {
            try
            {
                var loggerConfiguration = new LoggerConfiguration()
                    .ReadFrom.Configuration(_configuration)
                    .WriteTo.Debug()
                    .WriteTo.Console();

                if (_writeToFiles.Value())
                {
                    loggerConfiguration = loggerConfiguration.WriteTo.File(
                        path: _writeToFiles.FilePath,
                        rollingInterval: RollingInterval.Day,
                        shared: true,
                        outputTemplate: _messageTemplate.Value());
                }

                _logger = loggerConfiguration.CreateLogger();
            }
            catch (Exception exception)
            {
                _exception = exception;
            }

            return this;
        }

        public bool IsCreated() => _logger != null;
    }
}
