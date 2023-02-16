using Elasticsearch.Net;
using Serilog;
using Serilog.Core;
using Serilog.Exceptions;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Elasticsearch;

namespace MG.Utils.Elk.Logging
{
    public class ElkLoggerBuilder : ILoggerBuilder
    {
        private readonly ElasticSearchConfig _configuration;
        private Logger _logger;
        private ElasticsearchClientException _elkClientException;

        public ElkLoggerBuilder(ElasticSearchConfig configuration)
        {
            _configuration = configuration;
        }

        public bool IsCreated() => _logger != null;

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
            if (_elkClientException == null)
            {
                throw new InvalidOperationException(ExceptionMessage.LoggerNotInitiated);
            }

            return _elkClientException;
        }

        public ILoggerBuilder TryCreateLogger()
        {
            try
            {
                _logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .Enrich.WithExceptionDetails()
                    .ReadFrom.Configuration(_configuration.Configuration)
                    .WriteTo.Debug()
                    .WriteTo.Console()
                    .WriteTo.Elasticsearch(ConfigureEls())
                    .CreateLogger();
            }
            catch (ElasticsearchClientException exception)
            {
                _elkClientException = exception;
            }

            return this;
        }

        private ElasticsearchSinkOptions ConfigureEls()
        {
            return new ElasticsearchSinkOptions(_configuration.ConnectionUri)
            {
                IndexFormat = string.Format("logstash-{0}-{1}", _configuration.AppName, _configuration.Environment),
                AutoRegisterTemplate = true,
                OverwriteTemplate = true,
                DetectElasticsearchVersion = true,
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                NumberOfReplicas = 1,
                NumberOfShards = 2,
                CustomFormatter = new ExceptionAsObjectJsonFormatter(renderMessage: true),

                // BufferBaseFilename = "./buffer",
                RegisterTemplateFailure = RegisterTemplateRecovery.FailSink,
                FailureCallback = e => Console.WriteLine(string.Format(ExceptionMessage.FailedToSubmitEvent, e.MessageTemplate)),
                EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                                   EmitEventFailureHandling.WriteToFailureSink |
                                   EmitEventFailureHandling.RaiseCallback,
            };
        }
    }
}
