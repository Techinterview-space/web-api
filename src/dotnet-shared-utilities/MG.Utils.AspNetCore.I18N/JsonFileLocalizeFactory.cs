using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace MG.Utils.AspNetCore.I18N
{
    public class JsonFileLocalizeFactory : IStringLocalizerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public JsonFileLocalizeFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IStringLocalizer Create(Type resourceSource) => Instance();

        public IStringLocalizer Create(string baseName, string location) => Instance();

        private IStringLocalizer Instance()
        {
            return _serviceProvider.GetRequiredService<IStringLocalizer>();
        }
    }
}