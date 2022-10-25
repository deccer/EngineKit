using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using EngineKit.Mathematics;

namespace EngineKit.Graphics;

public static class VertexInputDescriptorFactory
{
    private static readonly IDictionary<string, uint> _locationMap;

    static VertexInputDescriptorFactory()
    {
        _locationMap = new Dictionary<string, uint>
        {
            { nameof(VertexPositionNormalUvTangent.Position), 0 },
            { nameof(VertexPositionColor.Color), 1 },
            { nameof(VertexPositionNormalUvTangent.Normal), 2 },
            { nameof(VertexPositionNormalUvTangent.Uv), 3 },
            { nameof(VertexPositionNormalUvTangent.Tangent), 4 },
        };
    }

    public static VertexInputDescriptor CreateFromStruct<T>()
    {
        var vertexBindingDescriptors = ExtractVertexBindingDescriptors<T>();
        return new VertexInputDescriptor(vertexBindingDescriptors);
    }

    private static VertexInputBindingDescriptor[] ExtractVertexBindingDescriptors<T>()
    {
        var type = typeof(T);
        var typeFields = type.GetFields();
        return typeFields.Select(field =>
        {
            var fieldName = field.Name;
            var fieldType = field.FieldType;
            var fieldLocation = ToLocation(fieldName);
            var fieldComponentCount = ToComponentCount(fieldType);
            var fieldDataType = ToDataType(fieldType);
            var fieldOffset = (uint)Marshal.OffsetOf<T>(fieldName);

            return new VertexInputBindingDescriptor(
                fieldLocation,
                0,
                fieldComponentCount,
                fieldDataType,
                fieldOffset);
        }).ToArray();
    }

    private static uint ToLocation(string attributeName)
    {
        if (_locationMap.TryGetValue(attributeName, out var location))
        {
            return location;
        }

        throw new ArgumentOutOfRangeException($"Unknown attribute name {attributeName}");
    }

    private static DataType ToDataType(Type type)
    {
        if (type == typeof(Vector4) || type == typeof(Vector3) || type == typeof(Vector2) || type == typeof(float))
        {
            return DataType.Float;
        }

        if (type == typeof(uint))
        {
            return DataType.UnsignedInteger;
        }

        if (type == typeof(ushort))
        {
            return DataType.UnsignedShort;
        }

        if (type == typeof(byte))
        {
            return DataType.UnsignedByte;
        }

        if (type == typeof(Int4) || type == typeof(Int3) || type == typeof(int))
        {
            return DataType.Integer;
        }

        if (type == typeof(short))
        {
            return DataType.Short;
        }

        if (type == typeof(sbyte))
        {
            return DataType.Byte;
        }

        throw new ArgumentOutOfRangeException();
    }

    private static int ToComponentCount(Type type)
    {
        if (type == typeof(Vector4) || type == typeof(Int4))
        {
            return 4;
        }

        if (type == typeof(Vector3) || type == typeof(Int3))
        {
            return 3;
        }

        if (type == typeof(Vector2))
        {
            return 2;
        }

        if (type == typeof(uint) || type == typeof(ushort) || type == typeof(byte) ||
            type == typeof(int) || type == typeof(short) || type == typeof(sbyte) ||
            type == typeof(float))
        {
            return 1;
        }

        throw new ArgumentOutOfRangeException();
    }
}