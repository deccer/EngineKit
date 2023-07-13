using CSharpFunctionalExtensions;

namespace EngineKit.Graphics;

public interface IGraphicsPipelineBuilder
{
    IGraphicsPipelineBuilder WithShadersFromFiles(
        string vertexShaderFilePath,
        string fragmentShaderFilePath);

    IGraphicsPipelineBuilder WithShadersFromStrings(
        string vertexShaderSource,
        string fragmentShaderSource);

    IGraphicsPipelineBuilder WithVertexInput(VertexInputDescriptor vertexInputDescriptor);

    IGraphicsPipelineBuilder WithTopology(
        PrimitiveTopology primitiveTopology,
        bool isPrimitiveRestartEnabled = false);
    
    IGraphicsPipelineBuilder WithLineWidth(float lineWidth);

    Result<IGraphicsPipeline> Build(Label label);

    IGraphicsPipelineBuilder DisableCulling();

    IGraphicsPipelineBuilder EnableCulling(CullMode cullMode);

    IGraphicsPipelineBuilder WithFillMode(FillMode fillMode);

    IGraphicsPipelineBuilder WithFaceWinding(FaceWinding faceWinding);

    IGraphicsPipelineBuilder EnableDepthTest(CompareFunction compareFunction = CompareFunction.Less);

    IGraphicsPipelineBuilder EnableDepthWrite();

    IGraphicsPipelineBuilder EnableDepthBias(float constantFactor, float slopeFactor);

    IGraphicsPipelineBuilder EnableBlending(ColorBlendAttachmentDescriptor colorBlendAttachmentDescriptor);

    IGraphicsPipelineBuilder DisableDepthTest();

    IGraphicsPipelineBuilder DisableDepthWrite();

    IGraphicsPipelineBuilder DisableDepthBias();

    IGraphicsPipelineBuilder DisableBlending();
}