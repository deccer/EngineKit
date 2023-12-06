using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace EngineKit.Graphics;

public readonly record struct VertexInputDescriptor(VertexInputBindingDescriptor[] VertexBindingDescriptors, Label Label)
{
    private static readonly IDictionary<VertexType, VertexInputDescriptor> _vertexTypeToVertexInputDescriptorMapping;
    private static readonly IDictionary<Type, bool> _fieldTypeToNormalizedMapping;
    private static readonly IDictionary<Type, int> _fieldTypeToComponentCountMapping;
    private static readonly IDictionary<Type, DataType> _fieldTypeToDataTypeMapping;
    
    public readonly VertexInputBindingDescriptor[] VertexBindingDescriptors = VertexBindingDescriptors;

    public readonly Label Label = Label;

    static VertexInputDescriptor()
    {
        _fieldTypeToComponentCountMapping = new Dictionary<Type, int>
        {
            { typeof(float), 1 },
            { typeof(Vector2), 2},
            { typeof(Vector3), 3},
            { typeof(Vector4), 4},
            { typeof(uint), 4 }
        };
        _fieldTypeToDataTypeMapping = new Dictionary<Type, DataType>
        {
            { typeof(float), DataType.Float },
            { typeof(Vector2), DataType.Float },
            { typeof(Vector3), DataType.Float },
            { typeof(Vector4), DataType.Float },
            { typeof(uint), DataType.UnsignedByte }
        };
        _fieldTypeToNormalizedMapping = new Dictionary<Type, bool>
        {
            { typeof(float), false },
            { typeof(Vector2), false },
            { typeof(Vector3), false },
            { typeof(Vector4), false },
            { typeof(uint), true }
        };
        _vertexTypeToVertexInputDescriptorMapping = new Dictionary<VertexType, VertexInputDescriptor>
        {
            { VertexType.Position, BuildVertexInputDescriptorFor<VertexPosition>() },
            { VertexType.PositionColor, BuildVertexInputDescriptorFor<VertexPositionColor>() },
            { VertexType.PositionColorUv, BuildVertexInputDescriptorFor<VertexPositionColorUv>() },
            { VertexType.PositionNormal, BuildVertexInputDescriptorFor<VertexPositionNormal>() },
            { VertexType.PositionNormalUv, BuildVertexInputDescriptorFor<VertexPositionNormalUv>() },
            { VertexType.PositionNormalUvTangent, BuildVertexInputDescriptorFor<VertexPositionNormalUvTangent>() },
            { VertexType.Default, BuildVertexInputDescriptorFor<VertexPositionNormalUvTangent>() },
            { VertexType.ImGui, BuildVertexInputDescriptorFor<ImDrawVert>() }
        };
    }

    public static VertexInputDescriptor ForVertexType(VertexType vertexType)
    {
        if (_vertexTypeToVertexInputDescriptorMapping.TryGetValue(vertexType, out var vertexInputDescriptor))
        {
            return vertexInputDescriptor;
        }

        throw new ArgumentOutOfRangeException($"VertexType {vertexType} has no vertex input descriptor mapping");
    }

    private static VertexInputDescriptor BuildVertexInputDescriptorFor<TVertexType>()
    {
        var vertexType = typeof(TVertexType);
        var vertexTypeAttributes = vertexType.GetFields();
        var vertexInputBindingDescriptors = vertexTypeAttributes.Select((vertexTypeAttribute, index) =>
        {
            var binding = 0u;
            var location = GetLocationFromFieldName(vertexTypeAttribute.Name);
            var dataType = GetDataTypeFromFieldType(vertexTypeAttribute.FieldType);
            var componentCount = GetComponentCountFromFieldType(vertexTypeAttribute.FieldType);
            var offset = (uint)Marshal.OffsetOf<TVertexType>(vertexTypeAttribute.Name);
            var isNormalized = GetNormalizedFromFieldType(vertexTypeAttribute.FieldType);

            return new VertexInputBindingDescriptor(location, binding, dataType, componentCount, offset, isNormalized);
        });
        return new VertexInputDescriptor(vertexInputBindingDescriptors.ToArray(), vertexType.Name);
    }

    private static uint GetLocationFromFieldName(string fieldName)
    {
        return fieldName switch
        {
            "Position" => 0u,
            "Color" => 1u,
            "Normal" => 2u,
            "Uv" => 3u,
            "Tangent" => 4u,
            _ => GetLocationFromFieldNameForImGui(fieldName)
        };
    }

    private static uint GetLocationFromFieldNameForImGui(string fieldName)
    {
        return fieldName.ToLower() switch
        {
            "pos" => 0u,
            "uv" => 1u,
            "col" => 2u,
            _ => throw new ArgumentOutOfRangeException(nameof(fieldName), fieldName, null)
        };
    }

    private static bool GetNormalizedFromFieldType(Type fieldType)
    {
        if (_fieldTypeToNormalizedMapping.TryGetValue(fieldType, out var isNormalized))
        {
            return isNormalized;
        }
        
        throw new ArgumentOutOfRangeException($"FieldType {fieldType.Name} has no normalized mapping");
    }

    private static DataType GetDataTypeFromFieldType(Type fieldType)
    {
        if (_fieldTypeToDataTypeMapping.TryGetValue(fieldType, out var dataType))
        {
            return dataType;
        }
        
        throw new ArgumentOutOfRangeException($"FieldType {fieldType.Name} has no data type mapping");
    }

    private static int GetComponentCountFromFieldType(Type fieldType)
    {
        if (_fieldTypeToComponentCountMapping.TryGetValue(fieldType, out var componentCount))
        {
            return componentCount;
        }

        throw new ArgumentOutOfRangeException($"FieldType {fieldType.Name} has no component count mapping");
    }
}