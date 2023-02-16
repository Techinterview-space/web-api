using DinkToPdf.Contracts;

namespace Infrastructure.Services.PDF;

public interface IDisposableConverter : IConverter, IDisposable
{
}