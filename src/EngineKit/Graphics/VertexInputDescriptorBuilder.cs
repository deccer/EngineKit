using System.Collections.Generic;
using System.Linq;

namespace EngineKit.Graphics;

public class VertexInputDescriptorBuilder
{
    private readonly IList<VertexInputBindingDescriptor> _vertexInputBindingDescriptors;

    public VertexInputDescriptorBuilder()
    {
        _vertexInputBindingDescriptors = new List<VertexInputBindingDescriptor>();
    }

    public VertexInputDescriptorBuilder AddAttribute(
        uint binding,
        DataType dataType,
        int componentCount,
        uint offset,
        bool isNormalized = false)
    {
        var location = (uint)_vertexInputBindingDescriptors.Count;
        _vertexInputBindingDescriptors.Add(new VertexInputBindingDescriptor(location, binding, dataType, componentCount,  offset, isNormalized));
        return this;
    }

    public VertexInputDescriptor Build(Label label)
    {
        return new VertexInputDescriptor(_vertexInputBindingDescriptors.ToArray(), label);
    }
}