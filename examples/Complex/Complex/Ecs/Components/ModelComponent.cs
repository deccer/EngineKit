namespace Complex.Ecs.Components;

public class ModelComponent : Component
{
    public ModelComponent(string modelName)
    {
        ModelName = modelName;
    }

    public string ModelName;
}