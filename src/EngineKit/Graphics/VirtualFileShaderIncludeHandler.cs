using System;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using OpenTK.Mathematics;
using Serilog;
using Point = SixLabors.ImageSharp.Point;
using Vector2 = System.Numerics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;
using Vector4 = OpenTK.Mathematics.Vector4;

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

        if (type == typeof(Color4))
        {
            return "vec4";
        }

        if (type == typeof(Vector2i))
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

        if (type == typeof(Matrix2))
        {
            return "mat2";
        }

        if (type == typeof(Matrix3))
        {
            return "mat3";
        }

        if (type == typeof(Matrix4) || type == typeof(Matrix4x4))
        {
            return "mat4";
        }

        if (type == typeof(Matrix4x3))
        {
            return "mat4x3";
        }

        if (type == typeof(Matrix2d))
        {
            return "dmat2";
        }

        if (type == typeof(Matrix3d))
        {
            return "dmat3";
        }

        if (type == typeof(Matrix4d))
        {
            return "dmat4";
        }

        if (type == typeof(Matrix4x3d))
        {
            return "dmat4x3";
        }

        return "INVALID";
    }
}