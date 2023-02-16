using MG.Utils.Abstract.NonNullableObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace MG.Utils.Elk.Logging
{
    public class ElasticSearchConfig
    {
        private NonNullableString _connectionString;

        private NonNullableString _appName;

        public string AppName
        {
            get
            {
                if (_appName == null)
                {
                    _appName = new NonNullableString(Configuration.GetSection("ElasticSearch")?["AppName"]);
                }

                return _appName.Value();
            }
        }

        public string ConnectionString
        {
            get
            {
                if (_connectionString == null)
                {
                    _connectionString = new NonNullableString(Configuration.GetSection("ElasticSearch")?["ConnectionString"]);
                }

                return _connectionString.Value();
            }
        }

        public Uri ConnectionUri => new Uri(ConnectionString);

        public string Environment { get; }

        public IConfiguration Configuration { get; }

        public ElasticSearchConfig(IConfiguration configuration, IHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment.EnvironmentName;
        }
    }
}
