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
                return VertexInputDescriptorFactory.CreateFromStruct<VertexPosition>();
            case VertexType.PositionColor:
                return VertexInputDescriptorFactory.CreateFromStruct<VertexPositionColor>();
            case VertexType.PositionColorUv:
                return VertexInputDescriptorFactory.CreateFromStruct<VertexPositionColorUv>();
            case VertexType.PositionNormal:
                return VertexInputDescriptorFactory.CreateFromStruct<VertexPositionNormal>();
            case VertexType.PositionNormalUv:
                return VertexInputDescriptorFactory.CreateFromStruct<VertexPositionNormalUv>();
            case VertexType.PositionNormalUvTangent:
                return VertexInputDescriptorFactory.CreateFromStruct<VertexPositionNormalUvTangent>();
            case VertexType.PositionUv:
                return VertexInputDescriptorFactory.CreateFromStruct<VertexPositionUv>();
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