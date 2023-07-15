using Microsoft.Extensions.DependencyInjection;

namespace Noppes.Fluffle.Imaging.Tests;

public static class ServiceCollectionExtensions
{
    public static void AddImagingTests(this IServiceCollection services, Func<IServiceProvider, Action<string>> createLogger)
    {
        services.AddSingleton<TestCaseHashesProvider>();
        
        services.AddSingleton<ThumbnailTestCaseProvider>();
        services.AddSingleton<PreconvertedTestCaseProvider>();
        services.AddSingleton<CompleteTestCaseProvider>();
        
        services.AddSingleton<IImagingTestsExecutor, ImagingTestsExecutor>();
        
        services.AddSingleton(provider => new Logger(createLogger(provider)));
    }
}