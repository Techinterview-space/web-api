using System;
using DinkToPdf.Contracts;

namespace TechInterviewer.Services.PDF;

public interface IDisposableConverter : IConverter, IDisposable
{
}