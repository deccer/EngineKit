using EngineKit;
using Serilog;

namespace HelloWindow;

internal sealed class HelloWindowApplication : Application
{
    public HelloWindowApplication(ILogger logger)
        : base(logger)
    {
    }
}