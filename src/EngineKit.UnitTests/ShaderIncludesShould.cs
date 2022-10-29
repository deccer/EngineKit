using EngineKit.Graphics;
using FluentAssertions;
using Xunit;

namespace EngineKit.UnitTests;

public class ShaderIncludesShould
{
    private readonly ShaderParser _shaderParser;

    public ShaderIncludesShould()
    {
        _shaderParser = new ShaderParser(
            new CompositeShaderIncludeHandler(
                new FileShaderIncludeHandler(),
                new VirtualFileShaderIncludeHandler()));
    }

    [Fact]
    public void BeInstantiable()
    {
        _shaderParser.Should().NotBeNull();
    }

    [Fact]
    public void ShouldReplaceIncludeWithContent()
    {
        // Arrange
        var shaderSource = $$"""
        #version 460 core

        #include "Common.glsl"
        #include "EngineKit.Graphics.GpuMaterial.virtual.glsl"

        void main()
        {
        }
        """;

        // Act
        var compiledString = _shaderParser.ParseShader(shaderSource);

        // Assert
        compiledString.Should().NotBeEmpty();
        compiledString.Should().NotContain("#include");
    }
}