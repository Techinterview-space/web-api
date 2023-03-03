using System;
using System.Threading;
using System.Threading.Tasks;
using DinkToPdf;
using Domain.Files;
using Microsoft.Extensions.Logging;

namespace TechInterviewer.Services.PDF;

public class PdfRenderer : IPdf
{
    public const string DefaultEncoding = "utf-8";

    private readonly IDisposableConverter _converter;
    private readonly GlobalSettings _globalSettings;
    private readonly string _defaultEncoding;

    private bool _disposed;

    public PdfRenderer(
        ILogger<PdfRenderer> logger,
        IDisposableConverter converter)
        : this(
            converter ?? throw new ArgumentNullException(nameof(converter)),
            new GlobalSettings(),
            DefaultEncoding,
            logger ?? throw new ArgumentNullException(nameof(logger)))
    {
    }

    public PdfRenderer(
        IDisposableConverter converter,
        GlobalSettings globalSettings,
        string defaultEncoding,
        ILogger<PdfRenderer> logger)
    {
        _converter = converter ?? throw new ArgumentNullException(nameof(converter));
        _globalSettings = globalSettings ?? new GlobalSettings();
        _defaultEncoding = defaultEncoding ?? DefaultEncoding;

        _converter.Error += (sender, args) =>
        {
            logger.LogError("Error converting PDF: {Message}", args.Message);
        };

        _converter.Warning += (sender, args) =>
        {
            logger.LogWarning("Warning converting PDF: {Message}", args.Message);
        };

        _converter.Finished += (sender, args) =>
        {
            logger.LogInformation("Finished converting PDF. Success: {Success}", args.Success);
        };

        _converter.PhaseChanged += (sender, args) =>
        {
            logger.LogInformation("Phase changed during converting PDF. {CurrentPhase}/{PhaseCount}", args.CurrentPhase, args.PhaseCount);
        };
    }

    public async Task<FileData> RenderAsync(string htmlContent, string filename, string contentType, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => Render(htmlContent, filename, contentType), cancellationToken);
    }

    public FileData Render(string htmlContent, string filename, string contentType)
    {
        var html = new HtmlToPdfDocument
        {
            Objects =
            {
                new ObjectSettings
                {
                    WebSettings = new WebSettings
                    {
                        DefaultEncoding = _defaultEncoding
                    },
                    HtmlContent = htmlContent
                }
            },
            GlobalSettings = _globalSettings
        };

        var byteContent = _converter.Convert(html);
        return new FileData(byteContent, filename, contentType);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed || !disposing)
        {
            return;
        }

        _converter?.Dispose();
        _disposed = true;
    }
}