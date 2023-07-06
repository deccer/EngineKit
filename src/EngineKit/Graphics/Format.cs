namespace EngineKit.Graphics;

public enum Format
{
    R8UNorm,
    R8G8UNorm,
    R8G8B8UNorm,
    R8G8B8A8UNorm,
    R8UInt,
    R8G8UInt,
    R8G8B8UInt,
    R8G8B8A8UInt,
    R8SNorm,
    R8G8SNorm,
    R8G8B8SNorm,
    R8G8B8A8SNorm,
    R8SInt,
    R8G8SInt,
    R8G8B8SInt,
    R8G8B8Srgb,
    R8G8B8A8SInt,
    R8G8B8A8Srgb,
    R10G10B10A2UNorm,
    R11G11B10Float,
    R16UNorm,
    R16G16UNorm,
    R16G16B16UNorm,
    R16G16B16A16UNorm,
    R16UInt,
    R16G16UInt,
    R16G16B16UInt,
    R16G16B16A16UInt,
    R16SNorm,
    R16G16SNorm,
    R16G16B16SNorm,
    R16G16B16A16SNorm,
    R16SInt,
    R16G16SInt,
    R16G16B16SInt,
    R16G16B16A16SInt,
    R16Float,
    R16G16Float,
    R16G16B16Float,
    R16G16B16A16Float,
    R32Float,
    R32G32Float,
    R32G32B32Float,
    R32G32B32A32Float,
    R32SInt,
    R32G32SInt,
    R32G32B32SInt,
    R32G32B32A32SInt,
    R32UInt,
    R32G32UInt,
    R32G32B32UInt,
    R32G32B32A32UInt,
    D32Float,
    D32UNorm,
    D24UNorm,
    D16UNorm,
    D32FloatS8UInt,
    D24UNormS8UInt,
    Bc1RgbUNorm,
    Bc1RgbSrgb,
    Bc1RgbaUNorm,
    Bc1RgbaSrgb,
    Bc2RgbaUNorm,
    Bc2RgbaSrgb,
    Bc3RgbaUNorm,
    Bc3RgbaSrgb,
    Bc4RUNorm,
    Bc4RSNorm,
    Bc5RgUNorm,
    Bc5RgSNorm,
    Bc6hRgbUFloat,
    Bc6hRgbSFloat,
    Bc7RgbaUNorm,
    Bc7RgbaSrgb
}

/*
 *     Fwog::Format VkBcFormatToFwog(uint32_t vkFormat)
    {
      // https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkFormat.html
      switch (vkFormat)
      {
      case 131: return Fwog::Format::BC1_RGB_UNORM;
      case 132: return Fwog::Format::BC1_RGB_SRGB;
      case 133: return Fwog::Format::BC1_RGBA_UNORM;
      case 134: return Fwog::Format::BC1_RGBA_SRGB;
      case 135: return Fwog::Format::BC2_RGBA_UNORM;
      case 136: return Fwog::Format::BC2_RGBA_SRGB;
      case 137: return Fwog::Format::BC3_RGBA_UNORM;
      case 138: return Fwog::Format::BC3_RGBA_SRGB;
      case 139: return Fwog::Format::BC4_R_UNORM;
      case 140: return Fwog::Format::BC4_R_SNORM;
      case 141: return Fwog::Format::BC5_RG_UNORM;
      case 142: return Fwog::Format::BC5_RG_SNORM;
      case 143: return Fwog::Format::BC6H_RGB_UFLOAT;
      case 144: return Fwog::Format::BC6H_RGB_SFLOAT;
      case 145: return Fwog::Format::BC7_RGBA_UNORM;
      case 146: return Fwog::Format::BC7_RGBA_SRGB;
      default: FWOG_UNREACHABLE; return {};
      }
    }
*/