using System.Linq;
using EngineKit.Graphics;
using EngineKit.Graphics.Shaders;
using FluentAssertions;
using Xunit;

namespace EngineKit.UnitTests;

public class ShaderIncludesShould
{
    private readonly ShaderParser _shaderParser;

    public ShaderIncludesShould()
    {
        _shaderParser = new ShaderParser(
            new IShaderIncludeHandler[]
            {
                new FileShaderIncludeHandler(),
                new VirtualFileShaderIncludeHandler()
            });
    }

    [Fact]
    public void BeInstantiable()
    {
        _shaderParser.Should().NotBeNull();
    }

    [Theory]
    [InlineData("EngineKit.Graphics." + nameof(GpuMaterial))]
    public void ShouldReplaceIncludeWithContent(string virtualShader)
    {
        // Arrange
        var shaderSource = """
        #version 460 core

        #include "Common.glsl"
        #include "@@VIRTUALSHADER@@.virtual.glsl"

        void main()
        {
        }
        """;

        // Act
        var compiledString = _shaderParser.ParseShader(shaderSource.Replace("@@VIRTUALSHADER@@", virtualShader));

        // Assert
        compiledString.Should().NotBeEmpty();
        compiledString.Should().Contain(virtualShader.Split(".").Last());
        compiledString.Should().NotContain("#include");
        compiledString.Should().NotContain("INVALID");
    }
}