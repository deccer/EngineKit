using CSharpFunctionalExtensions;

namespace EngineKit.Graphics;

public abstract class Pipeline : IPipeline
{
    protected ShaderProgram? ShaderProgram;

    public virtual void Bind()
    {
        ShaderProgram?.Use();
    }

    public virtual void Dispose()
    {
        ShaderProgram?.Dispose();
    }

    internal Result LinkPrograms()
    {
        if (ShaderProgram is null)
        {
            return Result.Failure("No shader program available");
        }

        var linkResult = ShaderProgram.Link();

        return linkResult.IsFailure
            ? linkResult
            : Result.Success();
    }

    public void BindUniformBuffer(
        IUniformBuffer buffer,
        uint bindingIndex)
    {
        buffer.Bind(bindingIndex);
    }

    public void BindShaderStorageBuffer(
        IShaderStorageBuffer buffer,
        uint bindingIndex)
    {
        buffer.Bind(bindingIndex);
    }
}