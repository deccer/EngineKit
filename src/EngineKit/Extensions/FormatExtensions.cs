using System;
using EngineKit.Graphics;
using EngineKit.Native.Ktx;

namespace EngineKit.Extensions;

public static class FormatExtensions
{
    public static bool IsCompressedFormat(this Format format)
    {
        return format switch
        {
            Format.Bc1RgbUNorm => true,
            Format.Bc1RgbSrgb => true,
            Format.Bc1RgbaUNorm => true,
            Format.Bc2RgbaUNorm => true,
            Format.Bc2RgbaSrgb => true,
            Format.Bc3RgbaUNorm => true,
            Format.Bc3RgbaSrgb => true,
            Format.Bc4RUNorm => true,
            Format.Bc4RSNorm => true,
            Format.Bc5RgSNorm => true,
            Format.Bc5RgUNorm => true,
            Format.Bc6hRgbUFloat => true,
            Format.Bc6hRgbSFloat => true,
            Format.Bc7RgbaUNorm => true,
            Format.Bc7RgbaSrgb => true,
            _ => false
        };
    }
    public static Format ToFormat(this Ktx.VkFormat vulkanFormat)
    {
        return vulkanFormat switch
        {
            Ktx.VkFormat.Bc1RgbUnormBlock => Format.Bc1RgbUNorm,
            Ktx.VkFormat.Bc1RgbSrgbBlock => Format.Bc1RgbSrgb,
            Ktx.VkFormat.Bc1RgbaUnormBlock => Format.Bc1RgbaUNorm,
            Ktx.VkFormat.Bc1RgbaSrgbBlock => Format.Bc1RgbaUNorm,
            Ktx.VkFormat.Bc2UnormBlock => Format.Bc2RgbaUNorm,
            Ktx.VkFormat.Bc2SrgbBlock => Format.Bc2RgbaSrgb,
            Ktx.VkFormat.Bc3UnormBlock => Format.Bc3RgbaUNorm,
            Ktx.VkFormat.Bc3SrgbBlock => Format.Bc3RgbaSrgb,
            Ktx.VkFormat.Bc4UnormBlock => Format.Bc4RUNorm,
            Ktx.VkFormat.Bc4SnormBlock => Format.Bc4RSNorm,
            Ktx.VkFormat.Bc5UnormBlock => Format.Bc5RgSNorm,
            Ktx.VkFormat.Bc5SnormBlock => Format.Bc5RgUNorm,
            Ktx.VkFormat.Bc6HUfloatBlock => Format.Bc6hRgbUFloat,
            Ktx.VkFormat.Bc6HSfloatBlock => Format.Bc6hRgbSFloat,
            Ktx.VkFormat.Bc7UnormBlock => Format.Bc7RgbaUNorm,
            Ktx.VkFormat.Bc7SrgbBlock => Format.Bc7RgbaSrgb,
            _ => throw new ArgumentOutOfRangeException(nameof(vulkanFormat), vulkanFormat, null)
        };
    }
    public static bool IsStencilFormat(this Format format)
    {
        return format switch
        {
            Format.D32FloatS8UInt => true,
            Format.D24UNormS8UInt => true,
            _ => false
        };
    }

    public static bool IsColorFormat(this Format format)
    {
        return !(IsStencilFormat(format) || IsDepthFormat(format));
    }
    
    public static bool IsDepthFormat(this Format format)
    {
        return format switch
        {
            Format.D32Float => true,
            Format.D32UNorm => true,
            Format.D32FloatS8UInt => true,
            Format.D24UNormS8UInt => true,
            Format.D24UNorm => true,
            Format.D16UNorm => true,
            _ => false
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
            case Format.R10G10B10A2UNorm:
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
            case Format.R11G11B10Float:
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
            case Format.R11G11B10Float:
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
            case Format.R10G10B10A2UNorm:
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
            Format.R8G8B8Srgb => FormatBaseType.Float,
            Format.R8G8B8A8Srgb => FormatBaseType.Float,
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
            Format.R10G10B10A2UNorm => FormatBaseType.Float,
            Format.R11G11B10Float => FormatBaseType.Float,
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
    
    public static DataType ToDataType(this Format format)
    {
        return format switch
        {
            Format.R8UNorm => DataType.Float,
            Format.R8G8UNorm => DataType.Float,
            Format.R8G8B8UNorm => DataType.Float,
            Format.R8G8B8A8UNorm => DataType.Float,
            Format.R8UInt => DataType.UnsignedInteger,
            Format.R8G8UInt => DataType.UnsignedInteger,
            Format.R8G8B8UInt => DataType.UnsignedInteger,
            Format.R8G8B8A8UInt => DataType.UnsignedInteger,
            Format.R8SNorm => DataType.Float,
            Format.R8G8SNorm => DataType.Float,
            Format.R8G8B8SNorm => DataType.Float,
            Format.R8G8B8A8SNorm => DataType.Float,
            Format.R8SInt => DataType.Integer,
            Format.R8G8SInt => DataType.Integer,
            Format.R8G8B8SInt => DataType.Integer,
            Format.R8G8B8A8SInt => DataType.Integer,
            Format.R10G10B10A2UNorm => DataType.Float,
            Format.R11G11B10Float => DataType.Float,
            Format.R16UNorm => DataType.Float,
            Format.R16G16UNorm => DataType.Float,
            Format.R16G16B16UNorm => DataType.Float,
            Format.R16G16B16A16UNorm => DataType.Float,
            Format.R16UInt => DataType.UnsignedInteger,
            Format.R16G16UInt => DataType.UnsignedInteger,
            Format.R16G16B16UInt => DataType.UnsignedInteger,
            Format.R16G16B16A16UInt => DataType.UnsignedInteger,
            Format.R16SNorm => DataType.Float,
            Format.R16G16SNorm => DataType.Float,
            Format.R16G16B16SNorm => DataType.Float,
            Format.R16G16B16A16SNorm => DataType.Float,
            Format.R16SInt => DataType.Integer,
            Format.R16G16SInt => DataType.Integer,
            Format.R16G16B16SInt => DataType.Integer,
            Format.R16G16B16A16SInt => DataType.Integer,
            Format.R16Float => DataType.Float,
            Format.R16G16Float => DataType.Float,
            Format.R16G16B16Float => DataType.Float,
            Format.R16G16B16A16Float => DataType.Float,
            Format.R32Float => DataType.Float,
            Format.R32G32Float => DataType.Float,
            Format.R32G32B32Float => DataType.Float,
            Format.R32G32B32A32Float => DataType.Float,
            Format.R32SInt => DataType.Integer,
            Format.R32G32SInt => DataType.Integer,
            Format.R32G32B32SInt => DataType.Integer,
            Format.R32G32B32A32SInt => DataType.Integer,
            Format.R32UInt => DataType.UnsignedInteger,
            Format.R32G32UInt => DataType.UnsignedInteger,
            Format.R32G32B32UInt => DataType.UnsignedInteger,
            Format.R32G32B32A32UInt => DataType.UnsignedInteger,
            Format.D32Float => DataType.Float,
            Format.D32UNorm => DataType.UnsignedInteger,
            Format.D24UNorm => DataType.UnsignedInteger,
            Format.D16UNorm => DataType.UnsignedInteger,
            Format.D32FloatS8UInt => DataType.UnsignedInteger,
            Format.D24UNormS8UInt => DataType.UnsignedInteger,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }
}