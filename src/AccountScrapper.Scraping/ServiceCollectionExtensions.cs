using AccountScrapper.Core.Abstractions;
using AccountScrapper.Scraping.Browser;
using AccountScrapper.Scraping.Hapoalim;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AccountScrapper.Scraping;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHapoalimScraper(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BrowserOptions>(configuration.GetSection(BrowserOptions.SectionName));
        services.Configure<HapoalimOptions>(configuration.GetSection(HapoalimOptions.SectionName));
        services.AddSingleton<IBrowserLauncher, PuppeteerBrowserLauncher>();
        services.AddSingleton<ITransactionSource, HapoalimTransactionSource>();
        return services;
    }
}
