namespace Complex.Engine.Ecs.Components;

public class ModelComponent : Component
{
    public string ModelName;

    public ModelComponent(string modelName)
    {
        ModelName = modelName;
    }
}
