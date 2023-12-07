using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpFunctionalExtensions;
using EngineKit.Graphics.Shaders;

namespace EngineKit.Graphics;

internal sealed class GraphicsPipelineBuilder : IGraphicsPipelineBuilder
{
    private readonly IDictionary<int, IInputLayout> _inputLayoutCache;
    private readonly IDictionary<IPipeline, GraphicsPipelineDescriptor> _graphicsPipelineCache;
    private readonly IShaderProgramFactory _shaderProgramFactory;
    private GraphicsPipelineDescriptor _graphicsPipelineDescriptor;
    private string? _vertexShaderFilePath;
    private string? _fragmentShaderFilePath;
    private string? _vertexShaderSource;
    private string? _fragmentShaderSource;
    private bool _shadersFromFiles = false;

    public GraphicsPipelineBuilder(
        IDictionary<int, IInputLayout> inputLayoutCache,
        IDictionary<IPipeline, GraphicsPipelineDescriptor> graphicsPipelineCache,
        IShaderProgramFactory shaderProgramFactory)
    {
        _inputLayoutCache = inputLayoutCache;
        _graphicsPipelineCache = graphicsPipelineCache;
        _shaderProgramFactory = shaderProgramFactory;
        _graphicsPipelineDescriptor = new GraphicsPipelineDescriptor
        {
            InputAssembly = new InputAssemblyDescriptor
            {
                IsPrimitiveRestartEnabled = false,
                PrimitiveTopology = PrimitiveTopology.Triangles
            },
            RasterizationDescriptor = new RasterizationDescriptor
            {
                CullMode = CullMode.Back,
                FaceWinding = FaceWinding.CounterClockwise,
                FillMode = FillMode.Solid,
                PointSize = 1.0f,
                LineWidth = 1.0f
            },
            ColorBlendDescriptor = new ColorBlendDescriptor
            {
                BlendConstants = new[] { 0.0f, 0.0f, 0.0f, 0.0f },
                ColorBlendAttachmentDescriptors = new[]
                {
                    ColorBlendAttachmentDescriptor.Opaque
                }
            },
            DepthStencilDescriptor = new DepthStencilDescriptor
            {
                DepthCompareFunction = CompareFunction.Less,
                IsDepthTestEnabled = true,
                IsDepthWriteEnabled = true
            }
        };
    }

    public IGraphicsPipelineBuilder WithVertexInput(VertexInputDescriptor vertexInputDescriptor)
    {
        _graphicsPipelineDescriptor.VertexInput = vertexInputDescriptor;
        return this;
    }

    public IGraphicsPipelineBuilder WithVertexAttributesForVertexType(VertexType vertexType)
    {
        _graphicsPipelineDescriptor.VertexInput = VertexInputDescriptor.ForVertexType(vertexType);
        return this;
    }

    public IGraphicsPipelineBuilder WithShadersFromFiles(
        string vertexShaderFilePath,
        string fragmentShaderFilePath)
    {
        _vertexShaderFilePath = vertexShaderFilePath;
        _fragmentShaderFilePath = fragmentShaderFilePath;
        _shadersFromFiles = true;
        return this;
    }

    public IGraphicsPipelineBuilder WithShadersFromStrings(
        string vertexShaderSource,
        string fragmentShaderSource)
    {
        _vertexShaderSource = vertexShaderSource;
        _fragmentShaderSource = fragmentShaderSource;
        _shadersFromFiles = false;
        return this;
    }

    public IGraphicsPipelineBuilder DisableCulling()
    {
        _graphicsPipelineDescriptor.RasterizationDescriptor.IsCullingEnabled = false;
        return this;
    }

    public IGraphicsPipelineBuilder EnableCulling(CullMode cullMode)
    {
        _graphicsPipelineDescriptor.RasterizationDescriptor.IsCullingEnabled = true;
        _graphicsPipelineDescriptor.RasterizationDescriptor.CullMode = cullMode;
        return this;
    }

    public IGraphicsPipelineBuilder WithFillMode(FillMode fillMode)
    {
        _graphicsPipelineDescriptor.RasterizationDescriptor.FillMode = fillMode;
        return this;
    }

    public IGraphicsPipelineBuilder WithFaceWinding(FaceWinding faceWinding)
    {
        _graphicsPipelineDescriptor.RasterizationDescriptor.FaceWinding = faceWinding;
        return this;
    }

    public IGraphicsPipelineBuilder WithTopology(
        PrimitiveTopology primitiveTopology,
        bool isPrimitiveRestartEnabled = false)
    {
        _graphicsPipelineDescriptor.InputAssembly.PrimitiveTopology = primitiveTopology;
        _graphicsPipelineDescriptor.InputAssembly.IsPrimitiveRestartEnabled = isPrimitiveRestartEnabled;
        return this;
    }

    public IGraphicsPipelineBuilder WithLineWidth(float lineWidth)
    {
        _graphicsPipelineDescriptor.RasterizationDescriptor.LineWidth = lineWidth;
        return this;
    }

    public IGraphicsPipelineBuilder EnableDepthTest(CompareFunction compareFunction = CompareFunction.Less)
    {
        _graphicsPipelineDescriptor.DepthStencilDescriptor.IsDepthTestEnabled = true;
        _graphicsPipelineDescriptor.DepthStencilDescriptor.DepthCompareFunction = compareFunction;
        return this;
    }

    public IGraphicsPipelineBuilder EnableDepthWrite()
    {
        _graphicsPipelineDescriptor.DepthStencilDescriptor.IsDepthWriteEnabled = true;
        return this;
    }

    public IGraphicsPipelineBuilder EnableDepthBias(float constantFactor, float slopeFactor)
    {
        _graphicsPipelineDescriptor.RasterizationDescriptor.IsDepthBiasEnabled = true;
        _graphicsPipelineDescriptor.RasterizationDescriptor.DepthBiasConstantFactor = constantFactor;
        _graphicsPipelineDescriptor.RasterizationDescriptor.DepthBiasSlopeFactor = slopeFactor;
        return this;
    }

    public IGraphicsPipelineBuilder DisableDepthTest()
    {
        _graphicsPipelineDescriptor.DepthStencilDescriptor.IsDepthTestEnabled = false;
        return this;
    }

    public IGraphicsPipelineBuilder DisableDepthBias()
    {
        _graphicsPipelineDescriptor.RasterizationDescriptor.IsDepthBiasEnabled = false;
        _graphicsPipelineDescriptor.RasterizationDescriptor.DepthBiasConstantFactor = 0.0f;
        _graphicsPipelineDescriptor.RasterizationDescriptor.DepthBiasSlopeFactor = 0.0f;
        return this;
    }

    public IGraphicsPipelineBuilder DisableDepthWrite()
    {
        _graphicsPipelineDescriptor.DepthStencilDescriptor.IsDepthWriteEnabled = false;
        return this;
    }

    public IGraphicsPipelineBuilder DisableBlending()
    {
        var blendAttachmentCount = _graphicsPipelineDescriptor.ColorBlendDescriptor.ColorBlendAttachmentDescriptors.Length;
        var opaque = ColorBlendAttachmentDescriptor.Opaque;
        opaque.IsBlendEnabled = false;
        for (var i = 0; i < blendAttachmentCount; i++)
        {
            _graphicsPipelineDescriptor.ColorBlendDescriptor.ColorBlendAttachmentDescriptors[i] = opaque;
        }

        return this;
    }

    public IGraphicsPipelineBuilder EnableBlending(ColorBlendAttachmentDescriptor colorBlendAttachmentDescriptor)
    {
        _graphicsPipelineDescriptor.ColorBlendDescriptor.ColorBlendAttachmentDescriptors = new[]
        {
            colorBlendAttachmentDescriptor
        };

        return this;
    }

    public IGraphicsPipelineBuilder ClearResourceBindingsOnBind()
    {
        _graphicsPipelineDescriptor.ClearResourceBindings = true;
        return this;
    }

    public Result<IGraphicsPipeline> Build(Label label)
    {
        if (!_graphicsPipelineDescriptor.VertexInput.VertexBindingDescriptors.Any())
        {
            return Result.Failure<IGraphicsPipeline>("VertexBindingDescriptors not found. Did you forget adding Attributes?");
        }

        if (_shadersFromFiles)
        {
            if (!File.Exists(_vertexShaderFilePath))
            {
                return Result.Failure<IGraphicsPipeline>($"File {_vertexShaderFilePath} not found");
            }

            _vertexShaderSource = File.ReadAllText(_vertexShaderFilePath);

            if (!File.Exists(_fragmentShaderFilePath))
            {
                return Result.Failure<IGraphicsPipeline>($"File {_fragmentShaderFilePath} not found");
            }

            _fragmentShaderSource = File.ReadAllText(_fragmentShaderFilePath);
        }

        if (string.IsNullOrEmpty(_vertexShaderSource))
        {
            return Result.Failure<IGraphicsPipeline>($"Vertex shader source not provided");
        }

        if (string.IsNullOrEmpty(_fragmentShaderSource))
        {
            return Result.Failure<IGraphicsPipeline>($"Fragment shader source not provided");
        }

        _graphicsPipelineDescriptor.VertexShaderSource = _vertexShaderSource;
        _graphicsPipelineDescriptor.FragmentShaderSource = _fragmentShaderSource;
        _graphicsPipelineDescriptor.PipelineProgramLabel = label;

        var vertexInputHashCode = _graphicsPipelineDescriptor.VertexInput.VertexBindingDescriptors.GetHashCode();
        if (!_inputLayoutCache.TryGetValue(vertexInputHashCode, out var inputLayout))
        {
            inputLayout = new InputLayout(_graphicsPipelineDescriptor.VertexInput);
            _inputLayoutCache.Add(vertexInputHashCode, inputLayout);
        }

        var graphicsShaderProgram = _shaderProgramFactory.CreateShaderProgram(
            _graphicsPipelineDescriptor.PipelineProgramLabel,
            _graphicsPipelineDescriptor.VertexShaderSource,
            _graphicsPipelineDescriptor.FragmentShaderSource);
        var graphicsShaderProgramLinkResult = graphicsShaderProgram.Link();
        if (graphicsShaderProgramLinkResult.IsFailure)
        {
            return Result.Failure<IGraphicsPipeline>(graphicsShaderProgramLinkResult.Error);
        }

        var graphicsPipeline = new GraphicsPipeline(
            _graphicsPipelineDescriptor,
            graphicsShaderProgram,
            inputLayout);
        _graphicsPipelineCache[graphicsPipeline] = _graphicsPipelineDescriptor;

        return Result.Success<IGraphicsPipeline>(graphicsPipeline);        
    }
}