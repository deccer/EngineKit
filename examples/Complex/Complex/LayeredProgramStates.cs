using System.Collections.Generic;
using System.Linq;
using Complex.States;
using Serilog;

namespace Complex;

internal sealed class LayeredProgramStates : ILayeredProgramStates
{
    private readonly ILogger _logger;
    private readonly Dictionary<string, IProgramState> _programStates;
    private readonly Dictionary<string, IEnumerable<string>> _programStatesLayers;

    private IEnumerable<IProgramState>? _currentProgramStateLayer;

    public LayeredProgramStates(
        ILogger logger,
        IEnumerable<IProgramState> programStates)
    {
        _logger = logger.ForContext<LayeredProgramStates>();
        _programStates = new Dictionary<string, IProgramState>();
        _programStatesLayers = new Dictionary<string, IEnumerable<string>>();

        foreach (var programState in programStates)
        {
            AddProgramState(programState.GetType().Name, programState);
        }
    }

    public bool Load()
    {
        foreach (var programState in _programStates)
        {
            if (!programState.Value.Load())
            {
                return false;
            }
        }

        return true;
    }

    public void ComposeLayeredState(string stateName, IEnumerable<string> stateNames)
    {
        if (!_programStatesLayers.TryAdd(stateName, stateNames))
        {
            _logger.Warning("State {StateName} already exists");
        }
    }

    public void SwitchToState(string stateName)
    {
        if (_programStatesLayers.TryGetValue(stateName, out IEnumerable<string>? stateNames))
        {
            _currentProgramStateLayer = _programStates
                .Where(programState => stateNames.Contains(programState.Key))
                .Select(programState => programState.Value);
            if (_currentProgramStateLayer != null)
            {
                foreach (var programState in _currentProgramStateLayer)
                {
                    programState.Activate();
                }
            }
        }
    }

    public void Render(float deltaTime, float elapsedSeconds)
    {
        if (_currentProgramStateLayer == null)
        {
            return;
        }

        foreach (var currentProgramState in _currentProgramStateLayer)
        {
            currentProgramState.Render(deltaTime, elapsedSeconds);
        }
    }

    public void Update(float deltaTime, float elapsedSeconds)
    {
        if (_currentProgramStateLayer == null)
        {
            return;
        }

        foreach (var currentProgramState in _currentProgramStateLayer)
        {
            currentProgramState.Update(deltaTime, elapsedSeconds);
        }
    }

    private void AddProgramState(string stateName, IProgramState programState)
    {
        if (!_programStates.TryAdd(stateName, programState))
        {
            _logger.Warning("State {StateName} already exists");
        }
    }
}
