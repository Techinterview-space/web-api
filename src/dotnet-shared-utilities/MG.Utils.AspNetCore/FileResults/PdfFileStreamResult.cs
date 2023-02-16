using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace MG.Utils.AspNetCore.FileResults
{
    public class PdfFileStreamResult : FileStreamResult
    {
        public PdfFileStreamResult(Stream stream)
            : base(stream, "application/pdf")
        {
        }
    }
}