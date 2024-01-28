using System.Collections.Generic;

namespace Complex;

internal interface ILayeredProgramStates
{
    bool Load();

    void ComposeLayeredState(string stateName,
                             IEnumerable<string> stateNames);

    void SwitchToState(string stateName);

    void Render(float deltaTime,
                float elapsedSeconds);

    void Update(float deltaTime,
                float elapsedSeconds);
}
