using System;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Elasticsearch;

namespace Web.Api.Services.Logging;

public class ElkSerilog
{
    private readonly string _appName;
    private readonly string _connectionString;
    private readonly string _environmentName;
    private readonly IConfiguration _configuration;

    public ElkSerilog(
        IConfiguration config,
        string appName,
        string connectionString,
        string environmentName)
    {
        _configuration = config;
        _appName = appName;
        _connectionString = connectionString;
        _environmentName = environmentName;
    }

    public void Setup()
    {
        Log.Logger = Logger();
    }

    private Logger Logger()
    {
        try
        {
            return new LoggerConfiguration()
                .WriteTo.Elasticsearch(ElasticsearchSinkOptions())
                .WriteTo.Console()
                .CreateLogger();
        }
        catch (Exception e)
        {
            if (Debugger.IsAttached)
            {
                return new LoggerConfiguration()
                    .WriteTo.Console()
                    .CreateLogger();
            }

            throw new InvalidOperationException("Could not create logger for ELK", e);
        }
    }

    private ElasticsearchSinkOptions ElasticsearchSinkOptions()
    {
        return new ElasticsearchSinkOptions(new Uri(_connectionString))
        {
            IndexFormat = $"logstash-{_appName}-{_environmentName}",
            AutoRegisterTemplate = true,
            OverwriteTemplate = true,
            DetectElasticsearchVersion = false, // TODO mgorbatyuk: trying to fix
            AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
            NumberOfReplicas = 1,
            NumberOfShards = 2,
            CustomFormatter = new ExceptionAsObjectJsonFormatter(renderMessage: true),
            RegisterTemplateFailure = RegisterTemplateRecovery.FailSink,
            FailureCallback = e => Console.WriteLine("Unable to submit event " + e.MessageTemplate),
            EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                               EmitEventFailureHandling.WriteToFailureSink |
                               EmitEventFailureHandling.RaiseCallback,
        };
    }
}