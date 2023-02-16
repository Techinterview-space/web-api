using System.IO;
using System.Threading.Tasks;
using ClosedXML.Excel;
using MG.Utils.Abstract;

namespace MG.Utils.Export.Worksheets
{
    public abstract class ExcelWorkbookBase
    {
        private readonly string _fileName;

        protected ExcelWorkbookBase(string fileName)
        {
            fileName.ThrowIfNull(nameof(fileName));
            _fileName = fileName;
        }

        protected abstract Task WriteAsync(XLWorkbook workbook);

        public async Task<Stream> WriteAsync()
        {
            await using var stream = new MemoryStream();
            using var workbook = new XLWorkbook(_fileName);

            await WriteAsync(workbook);

            workbook.SaveAs(stream);
            return new MemoryStream(stream.ToArray());
        }
    }
}