﻿namespace Fluffle.Imaging.Api.Client;

public class ImageMetadataModel
{
    public required int Width { get; set; }

    public required int Height { get; set; }

    public required int CenterX { get; set; }

    public required int CenterY { get; set; }
}
