using EngineKit.Graphics.RHI;

namespace EngineKit.Graphics;

internal record struct DepthStencilDescriptor(
    bool IsDepthTestEnabled = true,
    bool IsDepthWriteEnabled = true,
    CompareFunction DepthCompareFunction = CompareFunction.Less);