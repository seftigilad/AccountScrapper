using AccountScrapper.Core.Abstractions;
using AccountScrapper.Export.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AccountScrapper.Export;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJsonExport(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ExportOptions>(configuration.GetSection(ExportOptions.SectionName));
        services.AddSingleton<ITransactionExporter, JsonTransactionExporter>();
        return services;
    }
}
