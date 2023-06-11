using System;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using EngineKit.Mathematics;
using Serilog;
using Vector2 = System.Numerics.Vector2;
using Vector3 = EngineKit.Mathematics.Vector3;
using Vector4 = EngineKit.Mathematics.Vector4;

namespace EngineKit.Graphics;

public class VirtualFileShaderIncludeHandler : IShaderIncludeHandler
{
    public string? HandleInclude(string? include)
    {
        if (include == null || !include.EndsWith("virtual.glsl"))
        {
            return null;
        }

        var splitTypeName = include.Split(".").Reverse().Skip(2).Reverse();
        var includeTypeName = string.Join(".", splitTypeName);
        var includeTypeFullName = $"{includeTypeName}, {splitTypeName.First()}";
        var includeType = Type.GetType(includeTypeFullName);
        if (includeType == null)
        {
            Log.Logger.Error("{Category}: Unable to resolve virtual shader to type '{VirtualType}'", "Shader", includeTypeFullName);
            return null;
        }

        return GenerateGlslFromType(includeType);
    }

    private static string GenerateGlslFromType(Type includeType)
    {
        var glsl = new StringBuilder();
        glsl.AppendLine($"struct {includeType.Name}");
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
            return "uvec2";
        }

        if (type == typeof(float))
        {
            return "float";
        }

        if (type == typeof(Point))
        {
            return "ivec2";
        }

        if (type == typeof(Color4))
        {
            return "vec4";
        }

        if (type == typeof(Point))
        {
            return "ivec2";
        }

        if (type == typeof(Int3))
        {
            return "ivec3";
        }

        if (type == typeof(Int4))
        {
            return "ivec4";
        }

        if (type == typeof(Vector2))
        {
            return "vec2";
        }

        if (type == typeof(Vector3))
        {
            return "vec3";
        }

        if (type == typeof(Vector4))
        {
            return "vec4";
        }

        if (type == typeof(Matrix3x3))
        {
            return "mat3";
        }

        if (type == typeof(Matrix) || type == typeof(Matrix4x4))
        {
            return "mat4";
        }

        return "INVALID";
    }
}