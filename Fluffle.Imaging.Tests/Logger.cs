namespace Noppes.Fluffle.Imaging.Tests;

internal class Logger
{
    private readonly Action<string> _write;

    public Logger(Action<string> write)
    {
        _write = write;
    }

    public void Write(string message)
    {
        _write(message);
    }
}
