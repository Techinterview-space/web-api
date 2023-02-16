using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

namespace MG.Utils.AspNetCore.I18N.Middlewares
{
    // https://github.com/khalidabuhakmeh/aspnetcore_localization_sample/blob/master/WebApplication/Middlewares/RequestLocalizationCookiesMiddleware.cs
    public class RequestLocalizationCookiesMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly CookieRequestCultureProvider _provider;

        public RequestLocalizationCookiesMiddleware(RequestDelegate next, IOptions<RequestLocalizationOptions> requestLocalizationOptions)
        {
            _next = next;
            _provider = requestLocalizationOptions
                .Value
                .RequestCultureProviders
                .Where(x => x is CookieRequestCultureProvider)
                .Cast<CookieRequestCultureProvider>()
                .FirstOrDefault();
        }

        public async Task InvokeAsync(HttpContext context, IOptions<RequestLocalizationOptions> requestLocalizationOptions)
        {
            if (_provider != null)
            {
                var feature = context.Features.Get<IRequestCultureFeature>();

                if (feature != null)
                {
                    // remember culture across request
                    context.Response
                        .Cookies
                        .Append(_provider.CookieName, CookieRequestCultureProvider.MakeCookieValue(feature.RequestCulture));
                }
            }

            await _next(context);
        }
    }
}