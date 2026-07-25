using System.Globalization;
using API.Localization;
using Application.Interfaces.Localization;
using Infrastructure.Localization;
using Microsoft.AspNetCore.Localization;

namespace API.Extentions
{
    public static class LocalizationExtension
    {
        public static IServiceCollection AddLocalizationExtension(this IServiceCollection services)
        {
            services.AddLocalization();

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supported = SupportedCultures.All
                    .Select(c => new CultureInfo(c))
                    .ToList();

                options.DefaultRequestCulture = new RequestCulture(SupportedCultures.Default);
                options.SupportedCultures = supported;
                options.SupportedUICultures = supported;

                // Only Accept-Language header — query string and cookie providers are disabled
                options.RequestCultureProviders = [new AcceptLanguageHeaderRequestCultureProvider()];
            });

            services.AddScoped(typeof(IMessageLocalizer<>), typeof(MessageLocalizer<>));

            return services;
        }
    }
}
