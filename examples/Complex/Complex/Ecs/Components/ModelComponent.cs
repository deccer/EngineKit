namespace Complex.Ecs.Components;

public class ModelComponent : Component
{
    public string ModelName;

    public ModelComponent(string modelName)
    {
        ModelName = modelName;
    }
}
