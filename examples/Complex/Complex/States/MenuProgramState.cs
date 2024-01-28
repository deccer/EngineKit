using EngineKit;
using Serilog;

namespace Complex.States;

internal sealed class MenuProgramState : IProgramState
{
    private readonly IApplicationContext _applicationContext;
    private readonly ILogger _logger;

    public MenuProgramState(ILogger logger,
        IApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
        _logger = logger.ForContext<MenuProgramState>();
    }

    public void Activate()
    {
        _applicationContext.IsEditorEnabled = false;
    }

    public bool Load()
    {
        _logger.Debug("{Category}: Loaded {ProgramStateName}", "ProgramState", GetType().Name);

        return true;
    }

    public void Render(float deltaTime, float elapsedSeconds)
    {
    }

    public void Update(float deltaTime, float elapsedSeconds)
    {
    }
}
