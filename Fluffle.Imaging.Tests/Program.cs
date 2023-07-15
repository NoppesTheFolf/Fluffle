using Noppes.Fluffle.Imaging.Tests;
using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.PerceptualHashing;
using Noppes.Fluffle.Thumbnail;

IServiceCollection services = new ServiceCollection();

services.AddFluffleThumbnail();
services.AddSingleton<FluffleHash>();

services.AddImagingTests(_ => Console.WriteLine);

var provider = services.BuildServiceProvider();

var testsExecutor = provider.GetRequiredService<IImagingTestsExecutor>();
testsExecutor.Execute();
