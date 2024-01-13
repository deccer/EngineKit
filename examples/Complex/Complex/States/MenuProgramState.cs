using Serilog;

namespace Complex.States;

internal sealed class MenuProgramState : IProgramState
{
    private readonly ILogger _logger;

    public MenuProgramState(ILogger logger)
    {
        _logger = logger.ForContext<MenuProgramState>();
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