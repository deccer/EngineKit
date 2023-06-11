using Serilog;

namespace EngineKit.Graphics.Shaders;

internal sealed class ShaderProgramFactory : IShaderProgramFactory
{
    private readonly ILogger _logger;
    private readonly IShaderParser _shaderParser;

    public ShaderProgramFactory(ILogger logger, IShaderParser shaderParser)
    {
        _logger = logger.ForContext<ShaderProgramFactory>();
        _shaderParser = shaderParser;
    }

    public ShaderProgram CreateShaderProgram(Label label, string computeShaderSource)
    {
        var parsedComputeShaderSource = _shaderParser.ParseShader(computeShaderSource);
        return new ShaderProgram(label, parsedComputeShaderSource);
    }

    public ShaderProgram CreateShaderProgram(Label label, string vertexShaderSource, string fragmentShaderSource)
    {
        var parsedVertexShaderSource = _shaderParser.ParseShader(vertexShaderSource);
        var parsedFragmentShaderSource = _shaderParser.ParseShader(fragmentShaderSource);
        return new ShaderProgram(
            label, 
            parsedVertexShaderSource,
            parsedFragmentShaderSource);
    }
}