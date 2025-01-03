﻿using System;
using EngineKit.Graphics.RHI;
using EngineKit.Native.OpenGL;

namespace EngineKit.Extensions;

public static class ToGLExtensions
{
    public static GL.MapFlags ToGL(this MapFlags mapFlags)
    {
        var result = GL.MapFlags.Read;
        if ((mapFlags & MapFlags.Read) == MapFlags.Read)
        {
            result |= GL.MapFlags.Read;
        }

        if ((mapFlags & MapFlags.Write) == MapFlags.Write)
        {
            result |= GL.MapFlags.Write;
        }

        if ((mapFlags & MapFlags.Persistent) == MapFlags.Persistent)
        {
            result |= GL.MapFlags.Persistent;
        }
        
        if ((mapFlags & MapFlags.Coherent) == MapFlags.Coherent)
        {
            result |= GL.MapFlags.Coherent;
        }
        return result;
    }
    
    public static uint ToGL(this BufferStorageFlags bufferStorageFlags)
    {
        var result = 0u;
        result |= (bufferStorageFlags & BufferStorageFlags.ClientStorage) == BufferStorageFlags.ClientStorage ? (uint)GL.BufferStorageFlags.ClientStorageBit : 0u;
        result |= (bufferStorageFlags & BufferStorageFlags.DynamicStorage) == BufferStorageFlags.DynamicStorage ? (uint)GL.BufferStorageFlags.DynamicStorageBit : 0u;
        const uint memoryMappingFlags = (uint)GL.MapFlags.Read |
                                        (uint)GL.MapFlags.Write |
                                        (uint)GL.MapFlags.Persistent |
                                        (uint)GL.MapFlags.Coherent;
        result |= (bufferStorageFlags & BufferStorageFlags.MemoryMapped) == BufferStorageFlags.MemoryMapped ? memoryMappingFlags : 0u;
        return result;
    }
    public static GL.BlitFramebufferFilter ToGL(this BlitFramebufferFilter blitFramebufferFilter)
    {
        return blitFramebufferFilter switch
        {
            BlitFramebufferFilter.Linear => GL.BlitFramebufferFilter.Linear,
            BlitFramebufferFilter.Nearest => GL.BlitFramebufferFilter.Nearest,
            _ => throw new ArgumentOutOfRangeException(nameof(blitFramebufferFilter))
        };
    }
    
    public static GL.FramebufferBit ToGL(this FramebufferBit framebufferBit)
    {
        GL.FramebufferBit result = 0u;
        result |= (framebufferBit & FramebufferBit.ColorBufferBit) == FramebufferBit.ColorBufferBit
            ? GL.FramebufferBit.ColorBufferBit
            : 0;
        result |= (framebufferBit & FramebufferBit.DepthBufferBit) == FramebufferBit.DepthBufferBit
            ? GL.FramebufferBit.DepthBufferBit
            : 0;
        result |= (framebufferBit & FramebufferBit.StencilBufferBit) == FramebufferBit.StencilBufferBit
            ? GL.FramebufferBit.StencilBufferBit
            : 0;
        return result;
    }

    public static int ToGL(this Swizzle swizzle)
    {
        return swizzle switch
        {
            Swizzle.Red => (int)GL.PixelFormat.Red,
            Swizzle.Green => (int)GL.PixelFormat.Green,
            Swizzle.Blue => (int)GL.PixelFormat.Blue,
            Swizzle.Alpha => 0,
            Swizzle.One => 1,
            Swizzle.Zero => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(swizzle), swizzle, null)
        };
    }

    public static uint ToGL(this StorageAllocationFlags storageAllocationFlags)
    {
        var result = 0u;
        result |= (storageAllocationFlags & StorageAllocationFlags.Dynamic) == StorageAllocationFlags.Dynamic ? (uint)GL.BufferStorageFlags.DynamicStorageBit : 0;
        result |= (storageAllocationFlags & StorageAllocationFlags.Client) == StorageAllocationFlags.Client ? (uint)GL.BufferStorageFlags.ClientStorageBit : 0;
        result |= (storageAllocationFlags & StorageAllocationFlags.Mappable) == StorageAllocationFlags.Mappable
            ? (uint)(GL.MapFlags.Read | GL.MapFlags.Write | GL.MapFlags.Persistent | GL.MapFlags.Coherent)
            : 0;
        return result;
    }

    public static GL.DataType ToGL(this DataType dataType)
    {
        return dataType switch
        {
            DataType.Byte => GL.DataType.Byte,
            DataType.UnsignedByte => GL.DataType.UnsignedByte,
            DataType.Integer => GL.DataType.Int,
            DataType.UnsignedInteger => GL.DataType.UnsignedInt,
            DataType.Short => GL.DataType.Short,
            DataType.UnsignedShort => GL.DataType.UnsignedShort,
            DataType.Float => GL.DataType.Float,
            _ => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null)
        };
    }

    public static GL.TextureTarget ToGL(this TextureType textureType)
    {
        return textureType switch
        {
            TextureType.Texture1D => GL.TextureTarget.Texture1d,
            TextureType.Texture1DArray => GL.TextureTarget.Texture1dArray,
            TextureType.Texture2D => GL.TextureTarget.Texture2d,
            TextureType.Texture2DArray => GL.TextureTarget.Texture2dArray,
            TextureType.TextureCube => GL.TextureTarget.TextureCubeMap,
            TextureType.Texture2DMultisample => GL.TextureTarget.Texture2dMultisample,
            TextureType.Texture2DArrayMultisample => GL.TextureTarget.Texture2dMultisampleArray,
            _ => throw new ArgumentOutOfRangeException(nameof(textureType), textureType, null)
        };
    }

    public static GL.BufferTarget ToGL(this BufferTarget bufferTarget)
    {
        return bufferTarget switch
        {
            BufferTarget.VertexBuffer => GL.BufferTarget.ArrayBuffer,
            BufferTarget.IndexBuffer => GL.BufferTarget.ElementArrayBuffer,
            BufferTarget.ShaderStorageBuffer => GL.BufferTarget.ShaderStorageBuffer,
            BufferTarget.UniformBuffer => GL.BufferTarget.UniformBuffer,
            BufferTarget.DrawIndirectBuffer => GL.BufferTarget.DrawIndirectBuffer,
            BufferTarget.DispatchIndirectBuffer => GL.BufferTarget.DispatchIndirectBuffer,
            BufferTarget.ParameterBuffer => GL.BufferTarget.ParameterBuffer,
            _ => throw new ArgumentOutOfRangeException(nameof(bufferTarget), bufferTarget, null)
        };
    }

    public static GL.ShaderType ToGL(this ShaderType shaderType)
    {
        return shaderType switch
        {
            ShaderType.VertexShader => GL.ShaderType.VertexShader,
            ShaderType.FragmentShader => GL.ShaderType.FragmentShader,
            ShaderType.ComputeShader => GL.ShaderType.ComputeShader,
            ShaderType.TaskShader => GL.ShaderType.TaskShader,
            ShaderType.MeshShader => GL.ShaderType.MeshShader,
            _ => throw new ArgumentOutOfRangeException(nameof(shaderType), shaderType, null)
        };
    }

    public static GL.CullMode ToGL(this CullMode cullMode)
    {
        return cullMode switch
        {
            CullMode.Back => GL.CullMode.Back,
            CullMode.Front => GL.CullMode.Front,
            CullMode.FrontAndBack => GL.CullMode.FrontAndBack,
            _ => throw new ArgumentOutOfRangeException(nameof(cullMode), cullMode, null)
        };
    }

    public static GL.ClipControlDepth ToGL(this ClipControlDepth clipControlDepth)
    {
        return clipControlDepth switch
        {
            ClipControlDepth.NegativeOneToOne => GL.ClipControlDepth.NegativeOneToOne,
            ClipControlDepth.ZeroToOne => GL.ClipControlDepth.ZeroToOne,
            _ => throw new ArgumentOutOfRangeException(nameof(clipControlDepth), clipControlDepth, null)
        };
    }

    public static GL.FillMode ToGL(this FillMode fillMode)
    {
        return fillMode switch
        {
            FillMode.Solid => GL.FillMode.Solid,
            FillMode.Line => GL.FillMode.Line,
            FillMode.Point => GL.FillMode.Point,
            _ => throw new ArgumentOutOfRangeException(nameof(fillMode), fillMode, null)
        };
    }

    public static GL.BlendFactor ToGL(this Blend blend)
    {
        return blend switch
        {
            Blend.Zero => GL.BlendFactor.Zero,
            Blend.One => GL.BlendFactor.One,
            Blend.SourceColor => GL.BlendFactor.SrcColor,
            Blend.OneMinusSourceColor => GL.BlendFactor.OneMinusSrcColor,
            Blend.DestinationColor => GL.BlendFactor.DstColor,
            Blend.OneMinusDestinationColor => GL.BlendFactor.OneMinusDstColor,
            Blend.SourceAlpha => GL.BlendFactor.SrcAlpha,
            Blend.OneMinusSourceAlpha => GL.BlendFactor.OneMinusSrcAlpha,
            Blend.DestinationAlpha => GL.BlendFactor.DstAlpha,
            Blend.OneMinusDestinationAlpha => GL.BlendFactor.OneMinusDstAlpha,
            Blend.ConstantColor => GL.BlendFactor.ConstantColor,
            Blend.OneMinusConstantColor => GL.BlendFactor.OneMinusConstantColor,
            Blend.ConstantAlpha => GL.BlendFactor.ConstantAlpha,
            Blend.OneMinusConstantAlpha => GL.BlendFactor.OneMinusConstantAlpha,
            Blend.SourceAlphaSaturate => GL.BlendFactor.SrcAlphaSaturate,
            Blend.Source1Color => GL.BlendFactor.Src1Color,
            Blend.OneMinusSource1Color => GL.BlendFactor.OneMinusSrc1Color,
            Blend.Source1Alpha => GL.BlendFactor.Src1Alpha,
            Blend.OneMinusSource1Alpha => GL.BlendFactor.OneMinusSrc1Alpha,
            _ => throw new ArgumentOutOfRangeException(nameof(blend), blend, null)
        };
    }

    public static GL.CompareOperation ToGL(this CompareFunction compareFunction)
    {
        return compareFunction switch
        {
            CompareFunction.Never => GL.CompareOperation.Never,
            CompareFunction.Always => GL.CompareOperation.Always,
            CompareFunction.Less => GL.CompareOperation.Less,
            CompareFunction.LessOrEqual => GL.CompareOperation.LessOrEqual,
            CompareFunction.Greater => GL.CompareOperation.Greater,
            CompareFunction.GreaterOrEqual => GL.CompareOperation.GreaterOrEqual,
            CompareFunction.Equal => GL.CompareOperation.Equal,
            CompareFunction.NotEqual => GL.CompareOperation.NotEqual,
            _ => throw new ArgumentOutOfRangeException(nameof(compareFunction), compareFunction, null)
        };
    }

    public static GL.BlendOperation ToGL(this BlendFunction blendFunction)
    {
        return blendFunction switch
        {
            BlendFunction.Add => GL.BlendOperation.Add,
            BlendFunction.Subtract => GL.BlendOperation.Subtract,
            BlendFunction.ReverseSubtract => GL.BlendOperation.ReverseSubtract,
            BlendFunction.Min => GL.BlendOperation.Min,
            BlendFunction.Max => GL.BlendOperation.Max,
            _ => throw new ArgumentOutOfRangeException(nameof(blendFunction), blendFunction, null)
        };
    }

    public static GL.FaceWinding ToGL(this FaceWinding faceWinding)
    {
        return faceWinding switch
        {
            FaceWinding.Clockwise => GL.FaceWinding.Clockwise,
            FaceWinding.CounterClockwise => GL.FaceWinding.CounterClockwise,
            _ => throw new ArgumentOutOfRangeException(nameof(faceWinding), faceWinding, null)
        };
    }

    public static GL.SizedInternalFormat ToGL(this Format format)
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
            Format.R8G8B8Srgb => GL.SizedInternalFormat.Srgb8,
            Format.R8G8B8A8UInt => GL.SizedInternalFormat.Rgba8ui,
            Format.R8G8B8A8UNorm => GL.SizedInternalFormat.Rgba8,
            Format.R8G8B8A8SInt => GL.SizedInternalFormat.Rgba8i,
            Format.R8G8B8A8SNorm => GL.SizedInternalFormat.Rgba8Snorm,
            Format.R8G8B8A8Srgb => GL.SizedInternalFormat.Srgb8Alpha8,
            Format.R10G10B10A2UNorm => GL.SizedInternalFormat.Rgb10A2,
            Format.R11G11B10Float => GL.SizedInternalFormat.R11fG11fB10f,
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
            Format.Bc1RgbUNorm => GL.SizedInternalFormat.CompressedRgbS3tcDxt1Ext,
            Format.Bc1RgbaUNorm => GL.SizedInternalFormat.CompressedRgbaS3tcDxt1Ext,
            Format.Bc1RgbSrgb => GL.SizedInternalFormat.CompressedSrgbS3tcDxt1Ext,
            Format.Bc1RgbaSrgb => GL.SizedInternalFormat.CompressedSrgbAlphaS3tcDxt1Ext,
            Format.Bc2RgbaUNorm => GL.SizedInternalFormat.CompressedRgbaS3tcDxt3Ext,
            Format.Bc2RgbaSrgb => GL.SizedInternalFormat.CompressedSrgbAlphaS3tcDxt3Ext,
            Format.Bc3RgbaUNorm => GL.SizedInternalFormat.CompressedRgbaS3tcDxt5Ext,
            Format.Bc3RgbaSrgb => GL.SizedInternalFormat.CompressedSrgbAlphaS3tcDxt5Ext,
            Format.Bc4RUNorm => GL.SizedInternalFormat.CompressedRedRgtc1,
            Format.Bc4RSNorm => GL.SizedInternalFormat.CompressedSignedRedRgtc1,
            Format.Bc5RgUNorm => GL.SizedInternalFormat.CompressedRedGreenRgtc2Ext,
            Format.Bc5RgSNorm => GL.SizedInternalFormat.CompressedSignedRgRgtc2,
            Format.Bc6hRgbUFloat => GL.SizedInternalFormat.CompressedRgbBptcUnsignedFloat,
            Format.Bc6hRgbSFloat => GL.SizedInternalFormat.CompressedRgbBptcSignedFloat,
            Format.Bc7RgbaUNorm => GL.SizedInternalFormat.CompressedRgbaBptcUnorm,
            Format.Bc7RgbaSrgb => GL.SizedInternalFormat.CompressedSrgbAlphaBptcUnorm,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    public static GL.PixelFormat ToGL(this UploadFormat uploadFormat)
    {
        return uploadFormat switch
        {
            UploadFormat.Red => GL.PixelFormat.Red,
            UploadFormat.RedGreen => GL.PixelFormat.Rg,
            UploadFormat.RedGreenBlue => GL.PixelFormat.Rgb,
            UploadFormat.RedGreenBlueAlpha => GL.PixelFormat.Rgba,
            UploadFormat.BlueGreenRed => GL.PixelFormat.Bgr,
            UploadFormat.BlueGreenRedAlpha => GL.PixelFormat.Bgra,
            UploadFormat.DepthComponent => GL.PixelFormat.DepthComponent,
            UploadFormat.StencilIndex => GL.PixelFormat.StencilIndex,
            _ => throw new ArgumentOutOfRangeException(nameof(uploadFormat), uploadFormat, null)
        };
    }

    public static GL.DataType ToGL(this UploadType uploadType)
    {
        return uploadType switch
        {
            UploadType.UnsignedByte => GL.DataType.UnsignedByte,
            UploadType.UnsignedShort => GL.DataType.UnsignedShort,
            UploadType.UnsignedShort4444 => GL.DataType.UnsignedShort4444,
            UploadType.UnsignedShort5551 => GL.DataType.UnsignedShort5551,
            UploadType.UnsignedInteger => GL.DataType.UnsignedInt,
            UploadType.UnsignedInteger8888 => GL.DataType.UnsignedInt8888,
            UploadType.UnsignedInteger1010102 => GL.DataType.UnsignedInt1010102,
            UploadType.SignedByte => GL.DataType.Byte,
            UploadType.SignedShort => GL.DataType.Short,
            UploadType.SignedInteger => GL.DataType.Int,
            UploadType.Float => GL.DataType.Float,
            _ => throw new ArgumentOutOfRangeException(nameof(uploadType), uploadType, null)
        };
    }

    public static GL.AddressMode ToGL(this TextureAddressMode textureAddressMode)
    {
        return textureAddressMode switch
        {
            TextureAddressMode.Repeat => GL.AddressMode.Repeat,
            TextureAddressMode.MirroredRepeat => GL.AddressMode.MirroredRepeat,
            TextureAddressMode.ClampToEdge => GL.AddressMode.ClampToEdge,
            TextureAddressMode.ClampToBorder => GL.AddressMode.ClampToBorder,
            _ => throw new ArgumentOutOfRangeException(nameof(textureAddressMode), textureAddressMode, null)
        };
    }

    public static GL.PrimitiveType ToGL(this PrimitiveTopology primitiveTopology)
    {
        return primitiveTopology switch
        {
            PrimitiveTopology.Points => GL.PrimitiveType.Points,
            PrimitiveTopology.Lines => GL.PrimitiveType.Lines,
            PrimitiveTopology.LineLoop => GL.PrimitiveType.LineLoop,
            PrimitiveTopology.LineStrip => GL.PrimitiveType.LineStrip,
            PrimitiveTopology.Triangles => GL.PrimitiveType.Triangles,
            PrimitiveTopology.TriangleStrip => GL.PrimitiveType.TriangleStrip,
            PrimitiveTopology.TriangleFan => GL.PrimitiveType.TriangleFan,
            _ => throw new ArgumentOutOfRangeException(nameof(primitiveTopology), primitiveTopology, null)
        };
    }

    public static GL.DataType ToGL(this FormatBaseType formatBaseType)
    {
        return formatBaseType switch
        {
            FormatBaseType.SignedInteger => GL.DataType.Int,
            FormatBaseType.UnsignedInteger => GL.DataType.UnsignedInt,
            FormatBaseType.Float => GL.DataType.Float,
            _ => throw new ArgumentOutOfRangeException(nameof(formatBaseType), formatBaseType, null)
        };
    }

    public static GL.Filter ToGL(this TextureInterpolationFilter interpolationFilter)
    {
        return interpolationFilter switch
        {
            TextureInterpolationFilter.Default => GL.Filter.Linear,
            TextureInterpolationFilter.Linear => GL.Filter.Linear,
            TextureInterpolationFilter.Nearest => GL.Filter.Nearest,
            _ => throw new ArgumentOutOfRangeException(nameof(interpolationFilter), interpolationFilter, null)
        };
    }

    public static GL.Filter ToGL(this TextureMipmapFilter mipmapFilter)
    {
        return mipmapFilter switch
        {
            TextureMipmapFilter.Default => GL.Filter.LinearMipmapLinear,
            TextureMipmapFilter.Linear => GL.Filter.Linear,
            TextureMipmapFilter.LinearMipmapLinear => GL.Filter.LinearMipmapLinear,
            TextureMipmapFilter.LinearMipmapNearest => GL.Filter.LinearMipmapNearest,
            TextureMipmapFilter.Nearest => GL.Filter.Nearest,
            TextureMipmapFilter.NearestMipmapLinear => GL.Filter.NearestMipmapLinear,
            TextureMipmapFilter.NearestMipmapNearest => GL.Filter.NearestMipmapNearest,
            _ => throw new ArgumentOutOfRangeException(nameof(mipmapFilter), mipmapFilter, null)
        };
    }

    public static GL.MemoryBarrierMask ToGL(this BarrierMask barrierMask)
    {
        GL.MemoryBarrierMask result = 0u;
        result |= (barrierMask & BarrierMask.Command) == BarrierMask.Command
            ? GL.MemoryBarrierMask.CommandBarrierBit
            : 0;
        result |= (barrierMask & BarrierMask.Framebuffer) == BarrierMask.Framebuffer
            ? GL.MemoryBarrierMask.FramebufferBarrierBit
            : 0;
        result |= (barrierMask & BarrierMask.Uniform) == BarrierMask.Uniform
            ? GL.MemoryBarrierMask.UniformBarrierBit
            : 0;
        result |= (barrierMask & BarrierMask.AtomicCounter) == BarrierMask.AtomicCounter
            ? GL.MemoryBarrierMask.AtomicCounterBarrierBit
            : 0;
        result |= (barrierMask & BarrierMask.BufferUpdate) == BarrierMask.BufferUpdate
            ? GL.MemoryBarrierMask.BufferUpdateBarrierBit
            : 0;
        result |= (barrierMask & BarrierMask.ElementArray) == BarrierMask.ElementArray
            ? GL.MemoryBarrierMask.ElementArrayBarrierBit
            : 0;
        result |= (barrierMask & BarrierMask.PixelBuffer) == BarrierMask.PixelBuffer
            ? GL.MemoryBarrierMask.PixelBufferBarrierBit
            : 0;
        result |= (barrierMask & BarrierMask.QueryBuffer) == BarrierMask.QueryBuffer
            ? GL.MemoryBarrierMask.QueryBufferBarrierBit
            : 0;
        result |= (barrierMask & BarrierMask.ShaderStorage) == BarrierMask.ShaderStorage
            ? GL.MemoryBarrierMask.ShaderStorageBarrierBit
            : 0;
        result |= (barrierMask & BarrierMask.TextureFetch) == BarrierMask.TextureFetch
            ? GL.MemoryBarrierMask.TextureFetchBarrierBit
            : 0;
        result |= (barrierMask & BarrierMask.TextureUpdate) == BarrierMask.TextureUpdate
            ? GL.MemoryBarrierMask.TextureUpdateBarrierBit
            : 0;
        result |= (barrierMask & BarrierMask.TransformFeedback) == BarrierMask.TransformFeedback
            ? GL.MemoryBarrierMask.TransformFeedbackBarrierBit
            : 0;
        result |= (barrierMask & BarrierMask.ClientMappedBuffer) == BarrierMask.ClientMappedBuffer
            ? GL.MemoryBarrierMask.ClientMappedBufferBarrierBit
            : 0;
        result |= (barrierMask & BarrierMask.ShaderGlobalAccess) == BarrierMask.ShaderGlobalAccess
            ? GL.MemoryBarrierMask.ShaderGlobalAccessBarrierBitNv
            : 0;
        result |= (barrierMask & BarrierMask.ShaderImageAccess) == BarrierMask.ShaderImageAccess
            ? GL.MemoryBarrierMask.ShaderImageAccessBarrierBit
            : 0;
        result |= (barrierMask & BarrierMask.VertexAttribArray) == BarrierMask.VertexAttribArray
            ? GL.MemoryBarrierMask.VertexAttribArrayBarrierBit
            : 0;
        return result;
    }

    public static GL.MemoryAccess ToGL(this MemoryAccess memoryAccess)
    {
        return memoryAccess switch
        {
            MemoryAccess.ReadOnly => GL.MemoryAccess.ReadOnly,
            MemoryAccess.WriteOnly => GL.MemoryAccess.WriteOnly,
            MemoryAccess.ReadWrite => GL.MemoryAccess.ReadWrite,
            _ => throw new ArgumentOutOfRangeException(nameof(memoryAccess), memoryAccess, null)
        };
    }
}