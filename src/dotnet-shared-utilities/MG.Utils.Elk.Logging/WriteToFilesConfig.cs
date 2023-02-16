using MG.Utils.Abstract.NonNullableObjects;
using Microsoft.Extensions.Configuration;

namespace MG.Utils.Elk.Logging
{
    public class WriteToFilesConfig
    {
        private readonly Bool _value;
        private NonNullableString _path;
        private IConfiguration _configuration;

        public WriteToFilesConfig(IConfiguration configuration)
        {
            _value = new Bool(configuration.GetSection("Serilog")?["WriteToFileAllowed"]);
            _configuration = configuration;
        }

        public bool Value() => _value.ToBool();

        public string FilePath
        {
            get
            {
                if (_path == null)
                {
                    _path = new NonNullableString(_configuration.GetSection("Serilog")?["FilePath"]);
                }

                return _path;
            }
        }
    }
}
