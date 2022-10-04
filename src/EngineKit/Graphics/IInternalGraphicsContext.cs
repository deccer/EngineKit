using CSharpFunctionalExtensions;

namespace EngineKit.Graphics;

internal interface IInternalGraphicsContext
{
    Result<IGraphicsPipeline> CreateGraphicsPipeline(GraphicsPipelineDescriptor graphicsPipelineDescriptor);

    Result<IComputePipeline> CreateComputePipeline(ComputePipelineDescriptor computePipelineDescriptor);
}