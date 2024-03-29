﻿using System;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

internal sealed class InputLayout : IInputLayout
{
    private readonly uint _id;

    public InputLayout(VertexInputDescriptor vertexInputDescriptor)
    {
        _id = GL.CreateVertexArray();
        var label = "InputLayout-Default";
        if (vertexInputDescriptor.VertexBindingDescriptors != null)
        {
            label = string.IsNullOrEmpty(vertexInputDescriptor.Label)
                    ? "InputLayout-"
                    : $"InputLayout-{vertexInputDescriptor.Label}-";
            foreach (var vertexBinding in vertexInputDescriptor.VertexBindingDescriptors)
            {
                GL.EnableVertexArrayAttrib(_id, vertexBinding.Location);
                GL.VertexArrayAttribBinding(_id, vertexBinding.Location, vertexBinding.Binding);

                var componentDataType = vertexBinding.DataType;
                var componentCount = vertexBinding.ComponentCount;

                label += $"{componentDataType}{componentCount}";

                switch (componentDataType)
                {
                    case GL.DataType.Float:
                        GL.VertexArrayAttribFormat(
                            _id,
                            vertexBinding.Location,
                            componentCount,
                            componentDataType,
                            vertexBinding.IsNormalized,
                            vertexBinding.Offset);
                        break;
                    case GL.DataType.Int:
                    case GL.DataType.UnsignedInt:
                        GL.VertexArrayAttribIFormat(
                            _id,
                            vertexBinding.Location,
                            componentCount,
                            componentDataType,
                            vertexBinding.Offset);
                        break;
                    case GL.DataType.Byte:
                    case GL.DataType.UnsignedByte:
                        GL.VertexArrayAttribFormat(
                            _id,
                            vertexBinding.Location,
                            componentCount,
                            componentDataType,
                            true,
                            vertexBinding.Offset);
                        break;
                }
            }
        }

        GL.ObjectLabel(GL.ObjectIdentifier.VertexArray, _id, label);
    }

    public uint Id => _id;

    public void Bind()
    {
        GL.BindVertexArray(_id);
    }

    public void Dispose()
    {
        GL.DeleteVertexArray(_id);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_id);
    }
}