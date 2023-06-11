namespace EngineKit.Graphics.Shaders;

internal interface IShaderProgramFactory
{
    ShaderProgram CreateShaderProgram(Label label, string computeShaderSource);
    ShaderProgram CreateShaderProgram(Label label, string vertexShaderSource, string fragmentShaderSource);
}