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
        _vertexInputBindingDescriptors.Add(new VertexInputBindingDescriptor(location, binding, dataType, componentCount, offset, isNormalized));
        return this;
    }

    /*
    public VertexInputDescriptorBuilder AddAttribute(
        uint binding,
        Format format,
        uint offset)
    {
        var location = (uint)_vertexInputBindingDescriptors.Count;
        var (dataType, componentCount, isNormalized) = FormatToDataTypeAndComponentCount(format);
        _vertexInputBindingDescriptors.Add(new VertexInputBindingDescriptor(location, binding, dataType, componentCount, offset, isNormalized));
        return this;
    }
    */

    public VertexInputDescriptor Build(Label label)
    {
        return new VertexInputDescriptor(_vertexInputBindingDescriptors.ToArray(), label);
    }

    /*
    private (DataType DataType, int ComponentType, bool IsNormalized) FormatToDataTypeAndComponentCount(Format format)
    {
        var dataType = format.ToDataType();
    }
    */
}