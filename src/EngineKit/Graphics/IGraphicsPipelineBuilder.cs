using CSharpFunctionalExtensions;
using EngineKit.Core;
using EngineKit.Graphics.RHI;

namespace EngineKit.Graphics;

public interface IGraphicsPipelineBuilder
{
    IGraphicsPipelineBuilder WithShadersFromFiles(
        string vertexShaderFilePath,
        string fragmentShaderFilePath);

    IGraphicsPipelineBuilder WithShadersFromStrings(
        string vertexShaderSource,
        string fragmentShaderSource);

    IGraphicsPipelineBuilder WithVertexAttributesFromDescriptor(VertexInputDescriptor vertexInputDescriptor);
    
    IGraphicsPipelineBuilder WithVertexAttributesFromVertexType(VertexType vertexType);

    IGraphicsPipelineBuilder WithTopology(
        PrimitiveTopology primitiveTopology,
        bool isPrimitiveRestartEnabled = false);
    
    IGraphicsPipelineBuilder WithLineWidth(float lineWidth);

    Result<IGraphicsPipeline> Build(Label label);

    IGraphicsPipelineBuilder WithCullingDisabled();

    IGraphicsPipelineBuilder WithCullingEnabled(CullMode cullMode);

    IGraphicsPipelineBuilder WithFillMode(FillMode fillMode);

    IGraphicsPipelineBuilder WithFaceWinding(FaceWinding faceWinding);

    IGraphicsPipelineBuilder WithDepthTestEnabled(CompareFunction compareFunction);

    IGraphicsPipelineBuilder WithDepthWriteEnabled();

    IGraphicsPipelineBuilder WithDepthBiasEnabled(float constantFactor, float slopeFactor);

    IGraphicsPipelineBuilder WithClipControlDepth(ClipControlDepth clipControlDepth);

    IGraphicsPipelineBuilder WithBlendingEnabled(ColorBlendAttachmentDescriptor colorBlendAttachmentDescriptor);

    IGraphicsPipelineBuilder WithDepthTestDisabled();

    IGraphicsPipelineBuilder WithDepthWriteDisabled();

    IGraphicsPipelineBuilder WithDepthClampEnabled();

    IGraphicsPipelineBuilder WithDepthClampDisabled();

    IGraphicsPipelineBuilder WithDepthBiasDisabled();

    IGraphicsPipelineBuilder WithBlendingDisabled();
    
    IGraphicsPipelineBuilder ClearResourceBindingsOnBind();
}