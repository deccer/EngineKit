using System;
using System.Linq;
using System.Reflection;
using System.Text;
using OpenTK.Mathematics;
using Serilog;
using Point = SixLabors.ImageSharp.Point;
using Vector2 = System.Numerics.Vector2;

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

    private string? GenerateGlslFromType(Type includeType)
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
            glsl.AppendLine($"    {ToGlslType(member.Type)} {member.Name};");
        }
        glsl.AppendLine("};");

        return glsl.ToString();
    }

    private string ToGlslType(Type type)
    {
        if (type == typeof(int))
        {
            return "int";
        }

        if (type == typeof(uint))
        {
            return "uint";
        }

        if (type == typeof(float))
        {
            return "float";
        }

        if (type == typeof(Point))
        {
            return "ivec2";
        }

        if (type == typeof(Vector3i))
        {
            return "ivec3";
        }

        if (type == typeof(Vector4i))
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

        return "INVALID";
    }
}