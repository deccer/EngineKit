namespace Complex.States;

public interface IProgramState
{
    bool Load();
    
    void Render(float deltaTime, float elapsedSeconds);

    void Update(float deltaTime, float elapsedSeconds);
}