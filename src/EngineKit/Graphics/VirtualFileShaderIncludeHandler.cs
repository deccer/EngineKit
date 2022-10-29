using System;
using System.Linq;
using System.Reflection;
using System.Text;
using EngineKit.Mathematics;
using Point = SixLabors.ImageSharp.Point;
using Vector2 = System.Numerics.Vector2;

namespace EngineKit.Graphics;

public class VirtualFileShaderIncludeHandler : IShaderIncludeHandler
{
    public string? HandleInclude(string? include)
    {
        if (include == null)
        {
            return null;
        }

        if (include.EndsWith("virtual.glsl"))
        {
            var includeTypeName = string.Join(".", include.Split(".").Reverse().Skip(2).Reverse());
            var includeType = Type.GetType(includeTypeName);
            if (includeType == null)
            {
                return null;
            }
            return GenerateGlslFromType(includeType);
        }

        return null;
    }

    private string? GenerateGlslFromType(Type includeType)
    {
        var glsl = new StringBuilder();
        glsl.AppendLine($"struct {includeType.Name}");
        glsl.AppendLine("{");
        var members = includeType
            .GetFields(BindingFlags.Public)
            .Select(field => new { Name = field.Name, Type = field.FieldType })
            .Concat(includeType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(property => new { Name = property.Name, Type = property.PropertyType }));
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

        return "INVALID";
    }
}