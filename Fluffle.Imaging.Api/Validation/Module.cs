namespace Fluffle.Imaging.Api.Validation;

public static class Module
{
    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddSingleton<ImagingExceptionMiddleware>();

        // Signatures as per https://en.wikipedia.org/wiki/List_of_file_signatures
        // Except for JPEG, there are more valid signatures than listed on Wikipedia
        var fileSignatureChecker = new FileSignatureChecker();

        // JPEG
        fileSignatureChecker.Add([0xFF, 0xD8]);

        // PNG
        fileSignatureChecker.Add([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);

        // GIF
        fileSignatureChecker.Add([0x47, 0x49, 0x46, 0x38, 0x37, 0x61]);
        fileSignatureChecker.Add([0x47, 0x49, 0x46, 0x38, 0x39, 0x61]);

        // WebP
        fileSignatureChecker.Add([0x52, 0x49, 0x46, 0x46, null, null, null, null, 0x57, 0x45, 0x42, 0x50]);

        services.AddSingleton(fileSignatureChecker);

        return services;
    }

    public static IApplicationBuilder UseValidation(this IApplicationBuilder app)
    {
        app.UseMiddleware<ImagingExceptionMiddleware>();

        return app;
    }
}
