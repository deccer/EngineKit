using System;
using System.Collections.Generic;
using System.Linq;
using EngineKit.Core;
using EngineKit.Extensions;
using EngineKit.Graphics.RHI;
using EngineKit.Native.OpenGL;

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
        Format format,
        uint offset)
    {
        var location = (uint)_vertexInputBindingDescriptors.Count;

        var attributeDataType = GetDataType(format);
        var componentCount = GetComponentCount(format);
        var isNormalized = GetIsFormatNormalized(format);
        _vertexInputBindingDescriptors.Add(new VertexInputBindingDescriptor(location, binding, attributeDataType, componentCount, offset, isNormalized));
        return this;
    }

    private static GL.DataType GetDataType(Format format)
    {
        switch (format)
        {
            case Format.R8UNorm:
            case Format.R8G8UNorm:
            case Format.R8G8B8UNorm:
            case Format.R8G8B8A8UNorm:
            case Format.R8SNorm:
            case Format.R8G8SNorm:
            case Format.R8G8B8SNorm:
            case Format.R8G8B8A8SNorm:
            case Format.R16SNorm:
            case Format.R16G16SNorm:
            case Format.R16G16B16SNorm:
            case Format.R16G16B16A16SNorm:
            case Format.R16UNorm:
            case Format.R16G16UNorm:
            case Format.R16G16B16UNorm:
            case Format.R16G16B16A16UNorm:
            case Format.R16Float:
            case Format.R16G16Float:
            case Format.R16G16B16Float:
            case Format.R16G16B16A16Float:
            case Format.R32Float:
            case Format.R32G32Float:
            case Format.R32G32B32Float:
            case Format.R32G32B32A32Float:
                return DataType.Float.ToGL();
            case Format.R8SInt:
            case Format.R8G8SInt:
            case Format.R8G8B8SInt:
            case Format.R8G8B8A8SInt:
            case Format.R16SInt:
            case Format.R16G16SInt:
            case Format.R16G16B16SInt:
            case Format.R16G16B16A16SInt:                
            case Format.R32SInt:
            case Format.R32G32SInt:
            case Format.R32G32B32SInt:
            case Format.R32G32B32A32SInt:
                return DataType.Integer.ToGL();
            case Format.R8UInt:
            case Format.R8G8UInt:
            case Format.R8G8B8UInt:
            case Format.R8G8B8A8UInt:
            case Format.R16UInt:
            case Format.R16G16UInt:
            case Format.R16G16B16UInt:
            case Format.R16G16B16A16UInt:
            case Format.R32UInt:
            case Format.R32G32UInt:
            case Format.R32G32B32UInt:
            case Format.R32G32B32A32UInt:
                return DataType.UnsignedInteger.ToGL();
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }
    }
    
    private static int GetComponentCount(Format format)
    {
        switch (format)
        {
            case Format.R8UNorm:
            case Format.R8SNorm:
            case Format.R16SNorm:
            case Format.R16UNorm:
            case Format.R16Float:
            case Format.R32Float:
            case Format.R8SInt:
            case Format.R16SInt:
            case Format.R32SInt:
            case Format.R8UInt:
            case Format.R16UInt:
            case Format.R32UInt:
                return 1;
            case Format.R8G8UNorm:
            case Format.R8G8SNorm:
            case Format.R16G16SNorm:
            case Format.R16G16UNorm:
            case Format.R16G16Float:
            case Format.R32G32Float:
            case Format.R8G8SInt:
            case Format.R16G16SInt:
            case Format.R32G32SInt:
            case Format.R8G8UInt:
            case Format.R16G16UInt:
            case Format.R32G32UInt:                
                return 2;
            case Format.R8G8B8UNorm:
            case Format.R8G8B8SNorm:
            case Format.R16G16B16SNorm:
            case Format.R16G16B16UNorm:
            case Format.R16G16B16Float:
            case Format.R32G32B32Float:
            case Format.R8G8B8SInt:
            case Format.R16G16B16SInt:
            case Format.R32G32B32SInt:
            case Format.R8G8B8UInt:
            case Format.R16G16B16UInt:
            case Format.R32G32B32UInt:
                return 3;
            case Format.R8G8B8A8UNorm:
            case Format.R8G8B8A8SNorm:
            case Format.R16G16B16A16SNorm:
            case Format.R16G16B16A16UNorm:
            case Format.R16G16B16A16Float:
            case Format.R32G32B32A32Float:
            case Format.R8G8B8A8SInt:
            case Format.R16G16B16A16SInt:
            case Format.R32G32B32A32SInt:
            case Format.R8G8B8A8UInt:
            case Format.R16G16B16A16UInt:
            case Format.R32G32B32A32UInt:                
                return 4;
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }
    }
    
    private static bool GetIsFormatNormalized(Format format)
    {
        switch (format)
        {
            case Format.R8UNorm:
            case Format.R8G8UNorm:
            case Format.R8G8B8UNorm:
            case Format.R8G8B8A8UNorm:
            case Format.R8SNorm:
            case Format.R8G8SNorm:
            case Format.R8G8B8SNorm:
            case Format.R8G8B8A8SNorm:
            case Format.R16SNorm:
            case Format.R16G16SNorm:
            case Format.R16G16B16SNorm:
            case Format.R16G16B16A16SNorm:
            case Format.R16UNorm:
            case Format.R16G16UNorm:
            case Format.R16G16B16UNorm:
            case Format.R16G16B16A16UNorm:
                return true;
            case Format.R16Float:
            case Format.R16G16Float:
            case Format.R16G16B16Float:
            case Format.R16G16B16A16Float:
            case Format.R32Float:
            case Format.R32G32Float:
            case Format.R32G32B32Float:
            case Format.R32G32B32A32Float:
                
            case Format.R8SInt:
            case Format.R8G8SInt:
            case Format.R8G8B8SInt:
            case Format.R8G8B8A8SInt:
            case Format.R16SInt:
            case Format.R16G16SInt:
            case Format.R16G16B16SInt:
            case Format.R16G16B16A16SInt:                
            case Format.R32SInt:
            case Format.R32G32SInt:
            case Format.R32G32B32SInt:
            case Format.R32G32B32A32SInt:
                
            case Format.R8UInt:
            case Format.R8G8UInt:
            case Format.R8G8B8UInt:
            case Format.R8G8B8A8UInt:
            case Format.R16UInt:
            case Format.R16G16UInt:
            case Format.R16G16B16UInt:
            case Format.R16G16B16A16UInt:
            case Format.R32UInt:
            case Format.R32G32UInt:
            case Format.R32G32B32UInt:
            case Format.R32G32B32A32UInt:
                return false;
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }
    }

    public VertexInputDescriptor Build(Label label)
    {
        return new VertexInputDescriptor(_vertexInputBindingDescriptors.ToArray(), label);
    }
}