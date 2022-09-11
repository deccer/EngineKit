using System;
using EngineKit.Graphics;
using EngineKit.Native.OpenGL;

namespace EngineKit.Extensions;

public static class FormatExtensions
{
    public static VertexAttributeType ToVertexAttributeType(this Format format)
    {
        return format switch
        {
            Format.R16Float => VertexAttributeType.Float,
            Format.R16G16Float => VertexAttributeType.Float,
            Format.R16G16B16Float => VertexAttributeType.Float,
            Format.R16G16B16A16Float => VertexAttributeType.Float,
            Format.R32Float => VertexAttributeType.Float,
            Format.R32G32Float => VertexAttributeType.Float,
            Format.R32G32B32Float => VertexAttributeType.Float,
            Format.R32G32B32A32Float => VertexAttributeType.Float,
            Format.R8UInt => VertexAttributeType.Integer,
            Format.R8UNorm => VertexAttributeType.Integer,
            Format.R8SInt => VertexAttributeType.Integer,
            Format.R8SNorm => VertexAttributeType.Integer,
            Format.R8G8UInt => VertexAttributeType.Integer,
            Format.R8G8UNorm => VertexAttributeType.Integer,
            Format.R8G8SInt => VertexAttributeType.Integer,
            Format.R8G8SNorm => VertexAttributeType.Integer,
            Format.R8G8B8UInt => VertexAttributeType.Integer,
            Format.R8G8B8UNorm => VertexAttributeType.Integer,
            Format.R8G8B8SInt => VertexAttributeType.Integer,
            Format.R8G8B8SNorm => VertexAttributeType.Integer,
            Format.R8G8B8A8UInt => VertexAttributeType.Integer,
            Format.R8G8B8A8UNorm => VertexAttributeType.Byte,
            Format.R8G8B8A8SInt => VertexAttributeType.Integer,
            Format.R8G8B8A8SNorm => VertexAttributeType.Integer,
            Format.R16UInt => VertexAttributeType.Integer,
            Format.R16UNorm => VertexAttributeType.Integer,
            Format.R16SInt => VertexAttributeType.Integer,
            Format.R16SNorm => VertexAttributeType.Integer,
            Format.R16G16UInt => VertexAttributeType.Integer,
            Format.R16G16UNorm => VertexAttributeType.Integer,
            Format.R16G16SInt => VertexAttributeType.Integer,
            Format.R16G16SNorm => VertexAttributeType.Integer,
            Format.R16G16B16UInt => VertexAttributeType.Integer,
            Format.R16G16B16UNorm => VertexAttributeType.Integer,
            Format.R16G16B16SInt => VertexAttributeType.Integer,
            Format.R16G16B16SNorm => VertexAttributeType.Integer,
            Format.R16G16B16A16UInt => VertexAttributeType.Integer,
            Format.R16G16B16A16UNorm => VertexAttributeType.Integer,
            Format.R16G16B16A16SInt => VertexAttributeType.Integer,
            Format.R16G16B16A16SNorm => VertexAttributeType.Integer,
            Format.R32UInt => VertexAttributeType.Integer,
            Format.R32SInt => VertexAttributeType.Integer,
            Format.R32G32UInt => VertexAttributeType.Integer,
            Format.R32G32SInt => VertexAttributeType.Integer,
            Format.R32G32B32UInt => VertexAttributeType.Integer,
            Format.R32G32B32SInt => VertexAttributeType.Integer,
            Format.R32G32B32A32UInt => VertexAttributeType.Integer,
            Format.R32G32B32A32SInt => VertexAttributeType.Integer,
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }

    public static GL.SizedInternalFormat ToSizedInternalFormat(this Format format)
    {
        return format switch
        {
            Format.D16UNorm => GL.SizedInternalFormat.DepthComponent16,
            Format.D24UNorm => GL.SizedInternalFormat.DepthComponent24,
            Format.D32UNorm => GL.SizedInternalFormat.DepthComponent32,
            Format.D32Float => GL.SizedInternalFormat.DepthComponent32f,
            Format.D32FloatS8UInt => GL.SizedInternalFormat.Depth32fStencil8,
            Format.D24UNormS8UInt => GL.SizedInternalFormat.Depth24Stencil8,
            Format.R8UInt => GL.SizedInternalFormat.R8ui,
            Format.R8UNorm => GL.SizedInternalFormat.R8,
            Format.R8SInt => GL.SizedInternalFormat.R8i,
            Format.R8SNorm => GL.SizedInternalFormat.R8Snorm,
            Format.R8G8UInt => GL.SizedInternalFormat.Rg8ui,
            Format.R8G8UNorm => GL.SizedInternalFormat.Rg8,
            Format.R8G8SInt => GL.SizedInternalFormat.Rg8i,
            Format.R8G8SNorm => GL.SizedInternalFormat.Rg8Snorm,
            Format.R8G8B8UInt => GL.SizedInternalFormat.Rgb8ui,
            Format.R8G8B8UNorm => GL.SizedInternalFormat.Rgb8,
            Format.R8G8B8SInt => GL.SizedInternalFormat.Rgb8i,
            Format.R8G8B8SNorm => GL.SizedInternalFormat.Rgb8Snorm,
            Format.R8G8B8A8UInt => GL.SizedInternalFormat.Rgba8ui,
            Format.R8G8B8A8UNorm => GL.SizedInternalFormat.Rgba8,
            Format.R8G8B8A8SInt => GL.SizedInternalFormat.Rgba8i,
            Format.R8G8B8A8SNorm => GL.SizedInternalFormat.Rgba8Snorm,
            Format.R16UInt => GL.SizedInternalFormat.R16ui,
            Format.R16UNorm => GL.SizedInternalFormat.R16,
            Format.R16SInt => GL.SizedInternalFormat.R16i,
            Format.R16SNorm => GL.SizedInternalFormat.R16Snorm,
            Format.R16Float => GL.SizedInternalFormat.R16f,
            Format.R16G16UInt => GL.SizedInternalFormat.Rg16ui,
            Format.R16G16UNorm => GL.SizedInternalFormat.Rg16,
            Format.R16G16SInt => GL.SizedInternalFormat.Rg16i,
            Format.R16G16SNorm => GL.SizedInternalFormat.Rg16Snorm,
            Format.R16G16Float => GL.SizedInternalFormat.Rg16f,
            Format.R16G16B16UInt => GL.SizedInternalFormat.Rgb16ui,
            Format.R16G16B16UNorm => GL.SizedInternalFormat.Rgb16,
            Format.R16G16B16SInt => GL.SizedInternalFormat.Rgb16i,
            Format.R16G16B16SNorm => GL.SizedInternalFormat.Rgb16Snorm,
            Format.R16G16B16Float => GL.SizedInternalFormat.Rgb16f,
            Format.R16G16B16A16UInt => GL.SizedInternalFormat.Rgba16ui,
            Format.R16G16B16A16UNorm => GL.SizedInternalFormat.Rgba16,
            Format.R16G16B16A16SInt => GL.SizedInternalFormat.Rgba16i,
            Format.R16G16B16A16SNorm => GL.SizedInternalFormat.Rgba16Snorm,
            Format.R16G16B16A16Float => GL.SizedInternalFormat.Rgba16f,
            Format.R32UInt => GL.SizedInternalFormat.R32ui,
            Format.R32SInt => GL.SizedInternalFormat.R32i,
            Format.R32Float => GL.SizedInternalFormat.R32f,
            Format.R32G32UInt => GL.SizedInternalFormat.Rg32ui,
            Format.R32G32SInt => GL.SizedInternalFormat.Rg32i,
            Format.R32G32Float => GL.SizedInternalFormat.Rg32f,
            Format.R32G32B32UInt => GL.SizedInternalFormat.Rgb32ui,
            Format.R32G32B32SInt => GL.SizedInternalFormat.Rgb32i,
            Format.R32G32B32Float => GL.SizedInternalFormat.Rgb32f,
            Format.R32G32B32A32UInt => GL.SizedInternalFormat.Rgba32ui,
            Format.R32G32B32A32SInt => GL.SizedInternalFormat.Rgba32i,
            Format.R32G32B32A32Float => GL.SizedInternalFormat.Rgba32f,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    public static bool IsNormalized(this Format format)
    {
        switch (format)
        {
            case Format.R8UNorm:
            case Format.R8SNorm:
            case Format.R16UNorm:
            case Format.R16SNorm:
            case Format.R8G8UNorm:
            case Format.R8G8SNorm:
            case Format.R16G16UNorm:
            case Format.R16G16SNorm:
            case Format.R8G8B8A8UNorm:
            case Format.R8G8B8A8SNorm:
            case Format.R8G8B8UNorm:
            case Format.R8G8B8SNorm:
            case Format.R16G16B16UNorm:
            case Format.R16G16B16SNorm:
            case Format.R16G16B16A16UNorm:
            case Format.R16G16B16A16SNorm:
            case Format.D32UNorm:
            case Format.D24UNorm:
            case Format.D16UNorm:
                return true;

            case Format.R8UInt:
            case Format.R8SInt:
            case Format.R16UInt:
            case Format.R16SInt:
            case Format.R16Float:
            case Format.R32Float:
            case Format.R32SInt:
            case Format.R32UInt:
            case Format.D32Float:
            case Format.R8G8UInt:
            case Format.R8G8SInt:
            case Format.R16G16UInt:
            case Format.R16G16SInt:
            case Format.R16G16Float:
            case Format.R32G32Float:
            case Format.R32G32SInt:
            case Format.R32G32UInt:
            case Format.D32FloatS8UInt:
            case Format.D24UNormS8UInt:
            case Format.R8G8B8UInt:
            case Format.R8G8B8SInt:
            case Format.R16G16B16UInt:
            case Format.R16G16B16SInt:
            case Format.R16G16B16Float:
            case Format.R32G32B32Float:
            case Format.R32G32B32SInt:
            case Format.R32G32B32UInt:
            case Format.R8G8B8A8UInt:
            case Format.R8G8B8A8SInt:
            case Format.R16G16B16A16UInt:
            case Format.R16G16B16A16SInt:
            case Format.R16G16B16A16Float:
            case Format.R32G32B32A32Float:
            case Format.R32G32B32A32SInt:
            case Format.R32G32B32A32UInt:
                return false;

            default: throw new ArgumentException("Unknown Format");
        }
    }

    public static int ToFormatComponentCount(this Format format)
    {
        switch (format)
        {
            case Format.R8UNorm:
            case Format.R8UInt:
            case Format.R8SNorm:
            case Format.R8SInt:
            case Format.R16UNorm:
            case Format.R16UInt:
            case Format.R16SNorm:
            case Format.R16SInt:
            case Format.R16Float:
            case Format.R32Float:
            case Format.R32SInt:
            case Format.R32UInt:
            case Format.D32Float:
            case Format.D32UNorm:
            case Format.D24UNorm:
            case Format.D16UNorm:
                return 1;

            case Format.R8G8UNorm:
            case Format.R8G8UInt:
            case Format.R8G8SNorm:
            case Format.R8G8SInt:
            case Format.R16G16UNorm:
            case Format.R16G16UInt:
            case Format.R16G16SNorm:
            case Format.R16G16SInt:
            case Format.R16G16Float:
            case Format.R32G32Float:
            case Format.R32G32SInt:
            case Format.R32G32UInt:
            case Format.D32FloatS8UInt:
            case Format.D24UNormS8UInt:
                return 2;

            case Format.R8G8B8UNorm:
            case Format.R8G8B8UInt:
            case Format.R8G8B8SNorm:
            case Format.R8G8B8SInt:
            case Format.R16G16B16UNorm:
            case Format.R16G16B16UInt:
            case Format.R16G16B16SNorm:
            case Format.R16G16B16SInt:
            case Format.R16G16B16Float:
            case Format.R32G32B32Float:
            case Format.R32G32B32SInt:
            case Format.R32G32B32UInt:
                return 3;

            case Format.R8G8B8A8UNorm:
            case Format.R8G8B8A8UInt:
            case Format.R8G8B8A8SNorm:
            case Format.R8G8B8A8SInt:
            case Format.R16G16B16A16UNorm:
            case Format.R16G16B16A16UInt:
            case Format.R16G16B16A16SNorm:
            case Format.R16G16B16A16SInt:
            case Format.R16G16B16A16Float:
            case Format.R32G32B32A32Float:
            case Format.R32G32B32A32SInt:
            case Format.R32G32B32A32UInt:
                return 4;

            default: throw new ArgumentException("Unknown Format");
        }
    }

    public static FormatBaseType ToFormatBaseType(this Format format)
    {
        return format switch
        {
            Format.R8UNorm => FormatBaseType.Float,
            Format.R8G8UNorm => FormatBaseType.Float,
            Format.R8G8B8UNorm => FormatBaseType.Float,
            Format.R8G8B8A8UNorm => FormatBaseType.Float,
            Format.R8UInt => FormatBaseType.UnsignedInteger,
            Format.R8G8UInt => FormatBaseType.UnsignedInteger,
            Format.R8G8B8UInt => FormatBaseType.UnsignedInteger,
            Format.R8G8B8A8UInt => FormatBaseType.UnsignedInteger,
            Format.R8SNorm => FormatBaseType.Float,
            Format.R8G8SNorm => FormatBaseType.Float,
            Format.R8G8B8SNorm => FormatBaseType.Float,
            Format.R8G8B8A8SNorm => FormatBaseType.Float,
            Format.R8SInt => FormatBaseType.SignedInteger,
            Format.R8G8SInt => FormatBaseType.SignedInteger,
            Format.R8G8B8SInt => FormatBaseType.SignedInteger,
            Format.R8G8B8A8SInt => FormatBaseType.SignedInteger,
            Format.R16UNorm => FormatBaseType.Float,
            Format.R16G16UNorm => FormatBaseType.Float,
            Format.R16G16B16UNorm => FormatBaseType.Float,
            Format.R16G16B16A16UNorm => FormatBaseType.Float,
            Format.R16UInt => FormatBaseType.UnsignedInteger,
            Format.R16G16UInt => FormatBaseType.UnsignedInteger,
            Format.R16G16B16UInt => FormatBaseType.UnsignedInteger,
            Format.R16G16B16A16UInt => FormatBaseType.UnsignedInteger,
            Format.R16SNorm => FormatBaseType.Float,
            Format.R16G16SNorm => FormatBaseType.Float,
            Format.R16G16B16SNorm => FormatBaseType.Float,
            Format.R16G16B16A16SNorm => FormatBaseType.Float,
            Format.R16SInt => FormatBaseType.SignedInteger,
            Format.R16G16SInt => FormatBaseType.SignedInteger,
            Format.R16G16B16SInt => FormatBaseType.SignedInteger,
            Format.R16G16B16A16SInt => FormatBaseType.SignedInteger,
            Format.R16Float => FormatBaseType.Float,
            Format.R16G16Float => FormatBaseType.Float,
            Format.R16G16B16Float => FormatBaseType.Float,
            Format.R16G16B16A16Float => FormatBaseType.Float,
            Format.R32Float => FormatBaseType.Float,
            Format.R32G32Float => FormatBaseType.Float,
            Format.R32G32B32Float => FormatBaseType.Float,
            Format.R32G32B32A32Float => FormatBaseType.Float,
            Format.R32SInt => FormatBaseType.SignedInteger,
            Format.R32G32SInt => FormatBaseType.SignedInteger,
            Format.R32G32B32SInt => FormatBaseType.SignedInteger,
            Format.R32G32B32A32SInt => FormatBaseType.SignedInteger,
            Format.R32UInt => FormatBaseType.UnsignedInteger,
            Format.R32G32UInt => FormatBaseType.UnsignedInteger,
            Format.R32G32B32UInt => FormatBaseType.UnsignedInteger,
            Format.R32G32B32A32UInt => FormatBaseType.UnsignedInteger,
            Format.D32Float => FormatBaseType.Float,
            Format.D32UNorm => FormatBaseType.UnsignedInteger,
            Format.D24UNorm => FormatBaseType.UnsignedInteger,
            Format.D16UNorm => FormatBaseType.UnsignedInteger,
            Format.D32FloatS8UInt => FormatBaseType.UnsignedInteger,
            Format.D24UNormS8UInt => FormatBaseType.UnsignedInteger,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }
}