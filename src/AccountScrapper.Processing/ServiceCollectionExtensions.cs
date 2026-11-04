using AccountScrapper.Core.Abstractions;
using AccountScrapper.Processing.Masking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AccountScrapper.Processing;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTransactionProcessing(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MaskingOptions>(configuration.GetSection(MaskingOptions.SectionName));
        services.AddSingleton<IDescriptionMasker, DescriptionMasker>();
        services.AddSingleton<ITransactionProcessor, TransactionProcessor>();
        return services;
    }
}
