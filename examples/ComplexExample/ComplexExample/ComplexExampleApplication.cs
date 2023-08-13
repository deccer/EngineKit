using EngineKit;
using EngineKit.Graphics;
using EngineKit.Input;
using EngineKit.Mathematics;
using EngineKit.Native.Glfw;
using EngineKit.Native.OpenGL;
using Microsoft.Extensions.Options;
using Serilog;

namespace ComplexExample;

internal sealed class ComplexExampleApplication : GraphicsApplication
{
    private readonly ICamera _camera;
    private readonly IScene _scene;

    public ComplexExampleApplication(
        ILogger logger,
        IOptions<WindowSettings> windowSettings,
        IOptions<ContextSettings> contextSettings,
        IApplicationContext applicationContext,
        ICapabilities capabilities,
        IMetrics metrics,
        ILimits limits,
        IInputProvider inputProvider,
        IGraphicsContext graphicsContext,
        IUIRenderer uiRenderer,
        ICamera camera,
        IScene scene)
        : base(
            logger,
            windowSettings,
            contextSettings,
            applicationContext,
            capabilities,
            metrics,
            limits,
            inputProvider,
            graphicsContext,
            uiRenderer)
    {
        _camera = camera;
        _scene = scene;
    }

    protected override bool Initialize()
    {
        if (!base.Initialize())
        {
            return false;
        }
        
        SetWindowIcon("enginekit-icon.png");

        return true;
    }

    protected override bool Load()
    {
        if (!base.Load())
        {
            return false;
        }

        if (!_scene.Load())
        {
            return false;
        }

        _scene.DiscoverAssets();

        return true;
    }

    protected override void Render(float deltaTime)
    {
        _scene.Render();
        
        GL.PushDebugGroup("Pass: UI");
        UIRenderer.BeginLayout();
        _scene.RenderUi();
        UIRenderer.EndLayout();
        GL.PopDebugGroup();
    }
    
    protected override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        
        if (IsKeyPressed(Glfw.Key.KeyEscape))
        {
            Close();
        }
        
        if (IsMousePressed(Glfw.MouseButton.ButtonRight))
        {
            _camera.ProcessMouseMovement();
        }

        var movement = Vector3.Zero;
        var speedFactor = 160.0f;
        if (IsKeyPressed(Glfw.Key.KeyW))
        {
            movement += _camera.Direction;
        }
        if (IsKeyPressed(Glfw.Key.KeyS))
        {
            movement -= _camera.Direction;
        }
        if (IsKeyPressed(Glfw.Key.KeyA))
        {
            movement += -_camera.Right;
        }
        if (IsKeyPressed(Glfw.Key.KeyD))
        {
            movement += _camera.Right;
        }
        if (IsKeyPressed(Glfw.Key.KeyQ))
        {
            movement -= _camera.Up;
        }
        if (IsKeyPressed(Glfw.Key.KeyE))
        {
            movement += _camera.Up;
        }

        movement = Vector3.Normalize(movement) * deltaTime * 10;
        if (IsKeyPressed(Glfw.Key.KeyLeftShift))
        {
            movement *= speedFactor;
        }
        if (movement.Length() > 0.0f)
        {
            _camera.ProcessKeyboard(movement, deltaTime);
        }        
    }
}