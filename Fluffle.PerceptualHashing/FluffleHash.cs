using Nitranium.PerceptualHashing;
using Nitranium.PerceptualHashing.Imaging;
using Nitranium.PerceptualHashing.Providers;
using System;
using System.Diagnostics;

namespace Noppes.Fluffle.PerceptualHashing;

/// <summary>
/// Provides perceptual hashing functionality.
/// </summary>
public class FluffleHash
{
    private readonly ImagingProviderFactory _imagingProviderFactory;

    public FluffleHash()
    {
        // Use libvips imaging provider on platforms that are not Linux
        _imagingProviderFactory = new VipsInteropImagingProviderFactory();
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            return;

        // Prevent running program on Windows with the system drawing imaging provider
        if (!Debugger.IsAttached)
            throw new InvalidOperationException("This application can't run in production mode on Windows (no libvips).");

        _imagingProviderFactory = new DebugImagingProviderFactory<SystemDrawingImagingProviderFactory>();
    }

    public PHash Create(int size)
    {
        return new PHash
        {
            Size = size,
            Channel = Channel.Average,
            ImagingProviderFactory = _imagingProviderFactory
        };
    }
}
