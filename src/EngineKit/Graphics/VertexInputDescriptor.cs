using System;

namespace EngineKit.Graphics;

public readonly record struct VertexInputDescriptor
{
    public readonly VertexBindingDescriptor[] VertexBindingDescriptors;

    public static VertexInputDescriptor CreateFromVertexType(VertexType vertexType)
    {
        switch (vertexType)
        {
            case VertexType.Default:
                return new VertexInputDescriptor(null);
            case VertexType.ImGui:
                return new VertexInputDescriptor(
                    new VertexBindingDescriptor(0, 0, 2, DataType.Float, 0),
                    new VertexBindingDescriptor(1, 0, 2, DataType.Float, 8),
                    new VertexBindingDescriptor(3, 0, 4, DataType.UnsignedByte, 16, true));
            case VertexType.Position:
                return new VertexInputDescriptor(
                    new VertexBindingDescriptor(0, 0, 3, DataType.Float, 0));
            case VertexType.PositionColor:
                return new VertexInputDescriptor(
                    new VertexBindingDescriptor(0, 0, 3, DataType.Float, 0),
                    new VertexBindingDescriptor(1, 0, 3, DataType.Float, 12));
            case VertexType.PositionColorUv:
                return new VertexInputDescriptor(
                    new VertexBindingDescriptor(0, 0, 3, DataType.Float, 0),
                    new VertexBindingDescriptor(1, 0, 3, DataType.Float, 12),
                    new VertexBindingDescriptor(3, 0, 2, DataType.Float, 24));
            case VertexType.PositionNormal:
                return new VertexInputDescriptor(
                    new VertexBindingDescriptor(0, 0, 3, DataType.Float, 0),
                    new VertexBindingDescriptor(2, 0, 3, DataType.Float, 12));
            case VertexType.PositionNormalUv:
                return new VertexInputDescriptor(
                    new VertexBindingDescriptor(0, 0, 3, DataType.Float, 0),
                    new VertexBindingDescriptor(2, 0, 3, DataType.Float, 12),
                    new VertexBindingDescriptor(3, 0, 2, DataType.Float, 24));
            case VertexType.PositionNormalUvTangent:
                return new VertexInputDescriptor(
                    new VertexBindingDescriptor(0, 0, 3, DataType.Float, 0),
                    new VertexBindingDescriptor(2, 0, 3, DataType.Float, 12),
                    new VertexBindingDescriptor(3, 0, 2, DataType.Float, 24),
                    new VertexBindingDescriptor(4, 0, 4, DataType.Float, 32));
            case VertexType.PositionUv:
                return new VertexInputDescriptor(
                    new VertexBindingDescriptor(0, 0, 3, DataType.Float, 0),
                    new VertexBindingDescriptor(3, 0, 2, DataType.Float, 12));
            default:
                throw new ArgumentOutOfRangeException(nameof(vertexType));
        }
    }

    public VertexInputDescriptor(params VertexBindingDescriptor[]? vertexBindingDescriptors)
    {
        if (vertexBindingDescriptors != null)
        {
            VertexBindingDescriptors = new VertexBindingDescriptor[vertexBindingDescriptors.Length];
            for (var i = 0; i < vertexBindingDescriptors.Length; i++)
            {
                VertexBindingDescriptors[i] = vertexBindingDescriptors[i];
            }
        }
        else
        {
            VertexBindingDescriptors = Array.Empty<VertexBindingDescriptor>();
        }
    }
}