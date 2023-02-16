using System;
using Microsoft.AspNetCore.Http;

namespace MG.Utils.AspNetCore
{
    public record RandomFileName
    {
        private readonly IFormFile _file;

        public RandomFileName(IFormFile file)
        {
            _file = file;
        }

        public override string ToString()
        {
            return $"{Guid.NewGuid()}_{_file.FileName}";
        }

        public static implicit operator string(RandomFileName fileName)
        {
            return fileName.ToString();
        }
    }
}