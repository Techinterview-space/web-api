using System;
using DinkToPdf.Contracts;

namespace MG.Utils.Export.Pdf;

public interface IDisposableConverter : IConverter, IDisposable
{
}