using System;
using DinkToPdf;

namespace MG.Utils.Export.Pdf;

public class InjectedSynchronizedConverter : SynchronizedConverter, IDisposableConverter
{
    private bool _disposed = false;

    public InjectedSynchronizedConverter()
        : base(new PdfTools())
    {
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing || _disposed)
        {
            return;
        }

        _disposed = true;
        Tools.Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}