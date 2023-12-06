using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using EngineKit.Mathematics;
using Serilog;
using Num = System.Numerics;
using Vector2 = EngineKit.Mathematics.Vector2;
using Vector3 = EngineKit.Mathematics.Vector3;
using Vector4 = EngineKit.Mathematics.Vector4;

namespace EngineKit.Graphics.Shaders;

public class VirtualFileShaderIncludeHandler : IShaderIncludeHandler
{
    private static readonly IDictionary<string, string> _typeNamespaceMap;

    static VirtualFileShaderIncludeHandler()
    {
        var assemblies = new[]
            { Assembly.GetEntryAssembly(), Assembly.GetCallingAssembly(), Assembly.GetExecutingAssembly(), };
        var glslTypes = assemblies
            .Where(asm => asm != null)
            .Distinct()
            .SelectMany(asm => asm!.GetTypes()
                .Where(type => type.GetCustomAttribute<GlslAttribute>() != null || type.Name.StartsWith("Gpu")))
            .DistinctBy(type => type.FullName);

        _typeNamespaceMap = glslTypes.ToDictionary(type => type.FullName!, type => type.Assembly.GetName().Name!);
    }

    public VirtualFileShaderIncludeHandler(IDictionary<string, string>? additionalTypeNamespaceMappings = null)
    {
        if (additionalTypeNamespaceMappings != null)
        {
            foreach (var (typeName, assemblyName) in additionalTypeNamespaceMappings)
            {
                if (!_typeNamespaceMap.ContainsKey(typeName))
                {
                    _typeNamespaceMap.Add(typeName, assemblyName);
                }
            }
        }
    }

    public string? HandleInclude(string? include)
    {
        if (include == null || !include.EndsWith("virtual.glsl"))
        {
            return null;
        }

        var typeNameWithoutVirtualGlslExtension = include.Split(".").Reverse().Skip(2).Reverse();
        var fullTypeName = string.Join(".", typeNameWithoutVirtualGlslExtension);
        var fullQualifiedTypeName = fullTypeName;
        if (_typeNamespaceMap.TryGetValue(fullTypeName, out var assemblyName))
        {
            fullQualifiedTypeName = $"{fullTypeName}, {assemblyName}";
        }
        else
        {
            Log.Logger.Error("{Category}: Unable to resolve assembly for virtual shader to type '{VirtualType}'", "Shader", fullQualifiedTypeName);
            return null;
        }

        var includeType = Type.GetType(fullQualifiedTypeName);
        if (includeType == null)
        {
            Log.Logger.Error("{Category}: Unable to resolve virtual shader to type '{VirtualType}'", "Shader", fullQualifiedTypeName);
            return null;
        }

        return GenerateGlslFromType(includeType);
    }

    private static string GenerateGlslFromType(Type includeType)
    {
        var glsl = new StringBuilder();
        var uniformBufferAttribute = includeType.GetCustomAttribute<UniformBufferAttribute>();
        if (uniformBufferAttribute != null)
        {
            glsl.AppendLine($"layout(binding = {uniformBufferAttribute.Binding}, std140) uniform {includeType.Name}");
        }
        else
        {
            glsl.AppendLine($"struct {includeType.Name}");
        }

        var shaderStorageBufferAttribute = includeType.GetCustomAttribute<ShaderStorageBufferAttribute>();

        glsl.AppendLine("{");
        var fields = includeType
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Select(field => new { Name = field.Name, Type = field.FieldType });

        var properties = includeType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => new { property.Name, Type = property.PropertyType });

        var members = fields.Concat(properties);
        foreach (var member in members)
        {
            glsl.AppendLine(member.Name.EndsWith("Texture")
                ? $"    uvec2 {member.Name};"
                : $"    {ToGlslType(member.Type)} {member.Name};");
        }
        glsl.AppendLine("};");

        if (shaderStorageBufferAttribute != null)
        {
            var readOnlyAttribute = shaderStorageBufferAttribute.ReadOnly ? " readonly" : string.Empty;
            var bufferType = string.IsNullOrEmpty(shaderStorageBufferAttribute.Alias)
                ? includeType.Name + "Buffer"
                : shaderStorageBufferAttribute.Alias;

            glsl.AppendLine();
            glsl.AppendLine($"layout(binding = {shaderStorageBufferAttribute.Binding}, std430){readOnlyAttribute} buffer {bufferType}");
            glsl.AppendLine("{");
            glsl.AppendLine($"    {includeType.Name} {shaderStorageBufferAttribute.ArrayName}[];");
            glsl.AppendLine($"}} {JsonNamingPolicy.CamelCase.ConvertName(bufferType)};");
        }

        return glsl.ToString();
    }

    private static string ToGlslType(Type type)
    {
        if (type == typeof(int))
        {
            return "int";
        }

        if (type == typeof(uint))
        {
            return "uint";
        }

        if (type == typeof(ulong))
        {
            return "uint64_t";
        }

        if (type == typeof(float))
        {
            return "float";
        }

        if (type == typeof(Point))
        {
            return "ivec2";
        }

        if (type == typeof(Color4) || type == typeof(Color))
        {
            return "vec4";
        }

        if (type == typeof(Int3))
        {
            return "ivec3";
        }

        if (type == typeof(Int4))
        {
            return "ivec4";
        }

        if (type == typeof(Vector2) || type == typeof(Num.Vector2))
        {
            return "vec2";
        }

        if (type == typeof(Vector3) || type == typeof(Num.Vector3) || type == typeof(Color3))
        {
            return "vec3";
        }

        if (type == typeof(Vector4) || type == typeof(Num.Vector4))
        {
            return "vec4";
        }

        if (type == typeof(Matrix) || type == typeof(Matrix4x4))
        {
            return "mat4";
        }

        return "INVALID";
    }
}