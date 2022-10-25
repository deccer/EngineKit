using CSharpFunctionalExtensions;

namespace EngineKit.Graphics;

public interface IGraphicsPipelineBuilder
{
    IGraphicsPipelineBuilder WithShaders(
        string vertexShaderFilePath,
        string fragmentShaderFilePath);

    IGraphicsPipelineBuilder WithVertexBindings(params VertexInputBindingDescriptor[] vertexBindings);

    IGraphicsPipelineBuilder WithVertexBindingsForVertexType(VertexType vertexType);

    IGraphicsPipelineBuilder WithTopology(
        PrimitiveTopology primitiveTopology,
        bool isPrimitiveRestartEnabled = false);

    Result<IGraphicsPipeline> Build(string label);

    IGraphicsPipelineBuilder DisableCulling();

    IGraphicsPipelineBuilder EnableCulling(CullMode cullMode);

    IGraphicsPipelineBuilder WithFillMode(FillMode fillMode);

    IGraphicsPipelineBuilder WithFaceWinding(FaceWinding faceWinding);

    IGraphicsPipelineBuilder EnableDepthTest();

    IGraphicsPipelineBuilder EnableDepthWrite();

    IGraphicsPipelineBuilder EnableDepthBias(float constantFactor, float slopeFactor);

    IGraphicsPipelineBuilder DisableDepthTest();

    IGraphicsPipelineBuilder DisableDepthWrite();

    IGraphicsPipelineBuilder DisableDepthBias();

    IGraphicsPipelineBuilder UseDepthComparison(CompareOperation compareOperation);
}