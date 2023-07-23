# EngineKit

[![Discord](https://img.shields.io/discord/846125233807163437?style=plastic&logo=discord&logoColor=orange&label=EngineKit)](https://discord.gg/VxEaZ3B4Tg)

Abstraction over modern OpenGL.

It tries to hide the ugliness of the global state machine that is OpenGL.

## Getting Started

Create two projects

- `YourProject` as a console project
- `YourProject.Assets` as a class library project
---

- Add `YourProject.Assets` as a project reference to `YourProject`.
- Copy `Fonts` directory from [here](https://github.com/deccer/EngineKit/tree/main/examples/ForwardRendering/ForwardRendering.Assets) into `YourProject.Assets` (i am working on a neater solution)
- Add `EngineKit` to `YourProject` via nuget as a usual package.
- Create an `appsettings.json` in `YourProject` which should like like [this](https://github.com/deccer/EngineKit/blob/main/examples/ForwardRendering/ForwardRendering/appsettings.json) one.
- Make sure to have it copied when its newer by rightclickin it -> Properties -> "Copy to output directory" -> "Copy if newer"
---
- Create a class `YourProjectApplication` in `YourProject` and let it derive from `GraphicsApplication` (let your IDE implement the ctor)
- `Program.cs` of `YourProject` should look like
```cs
using EngineKit;
using EngineKit.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace YourProject;

internal static class Program
{
    public static void Main(string[] args)
    {
        using var serviceProvider = CreateServiceProvider();

        var application = serviceProvider.GetRequiredService<IApplication>();
        application.Run();
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false)
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddSingleton(Log.Logger);
        services.Configure<WindowSettings>(configuration.GetSection(nameof(WindowSettings)));
        services.Configure<ContextSettings>(configuration.GetSection(nameof(ContextSettings)));
        services.AddEngine();
        services.AddSingleton<IApplication, XXX>(); // replace XXX with YourProjectApplication 
        return services.BuildServiceProvider();
    }
}
```
- Run it. 
- You should get a black window which you cannot close :)
- For that you can implement `YourProjectApplication`'s `Update` method via
```cs
protected override void Update(float deltaTime)
{
    base.Update(deltaTime);
    if (IsKeyPressed(Glfw.Key.KeyEscape))
    {
        Close();
    }
}
```

TODO Complex Example