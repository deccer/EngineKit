using System.Numerics;
using EngineKit.Mathematics;

namespace EngineKit.Graphics;

public interface IFramebufferDescriptorBuilder
{
    FramebufferDescriptorBuilder Reset();
    
    FramebufferDescriptorBuilder WithViewport(int width, int height, float minDepthRange = -1.0f, float maxDepthRange = 1.0f);

    FramebufferDescriptorBuilder WithColorAttachment(
        ITexture colorAttachment,
        bool clear,
        int clearColor);

    FramebufferDescriptorBuilder WithColorAttachment(
        ITexture colorAttachment,
        bool clear,
        uint clearColor);

    FramebufferDescriptorBuilder WithColorAttachment(
        ITexture colorAttachment,
        bool clear,
        Int2 clearColor);

    FramebufferDescriptorBuilder WithColorAttachment(
        ITexture colorAttachment,
        bool clear,
        Int3 clearColor);

    FramebufferDescriptorBuilder WithColorAttachment(
        ITexture colorAttachment,
        bool clear,
        Int4 clearColor);

    FramebufferDescriptorBuilder WithColorAttachment(
        ITexture colorAttachment,
        bool clear,
        float clearColor);

    FramebufferDescriptorBuilder WithColorAttachment(
        ITexture colorAttachment,
        bool clear,
        Vector2 clearColor);

    FramebufferDescriptorBuilder WithColorAttachment(
        ITexture colorAttachment,
        bool clear,
        Vector3 clearColor);

    FramebufferDescriptorBuilder WithColorAttachment(
        ITexture colorAttachment,
        bool clear,
        Vector4 clearColor);

    FramebufferDescriptorBuilder WithDepthAttachment(
        ITexture depthTexture,
        bool clear,
        float clearValue = 1.0f);

    FramebufferDescriptorBuilder WithStencilAttachment(
        ITexture stencilTexture,
        bool clear,
        int clearValue = 0);

    FramebufferDescriptor Build(string label);
}