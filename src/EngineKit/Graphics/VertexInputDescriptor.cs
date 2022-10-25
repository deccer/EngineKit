namespace EngineKit.Graphics;

public readonly record struct VertexInputDescriptor(VertexInputBindingDescriptor[] VertexBindingDescriptors, Label Label)
{
    public readonly VertexInputBindingDescriptor[] VertexBindingDescriptors = VertexBindingDescriptors;

    public readonly Label Label = Label;
}