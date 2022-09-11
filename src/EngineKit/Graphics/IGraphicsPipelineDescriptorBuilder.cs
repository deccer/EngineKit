namespace EngineKit.Graphics;

public interface IGraphicsPipelineDescriptorBuilder
{
    IGraphicsPipelineDescriptorBuilder WithShaders(
        string vertexShaderFilePath,
        string fragmentShaderFilePath);

    IGraphicsPipelineDescriptorBuilder WithVertexBindings(params VertexBindingDescriptor[] vertexBindings);

    IGraphicsPipelineDescriptorBuilder WithVertexBindingsForVertexType(VertexType vertexType);

    IGraphicsPipelineDescriptorBuilder WithTopology(
        PrimitiveTopology primitiveTopology,
        bool isPrimitiveRestartEnabled = false);

    GraphicsPipelineDescriptor Build(string label);

    IGraphicsPipelineDescriptorBuilder DisableCulling();

    IGraphicsPipelineDescriptorBuilder EnableCulling(CullMode cullMode);

    IGraphicsPipelineDescriptorBuilder WithFillMode(FillMode fillMode);

    IGraphicsPipelineDescriptorBuilder WithFaceWinding(FaceWinding faceWinding);

    IGraphicsPipelineDescriptorBuilder EnableDepthTest();

    IGraphicsPipelineDescriptorBuilder EnableDepthWrite();

    IGraphicsPipelineDescriptorBuilder DisableDepthTest();

    IGraphicsPipelineDescriptorBuilder DisableDepthWrite();

    IGraphicsPipelineDescriptorBuilder UseDepthComparison(CompareOperation compareOperation);
}