namespace EngineKit.Graphics;

public record struct RenderAttachment(
    ITexture Texture,
    ClearValue ClearValue,
    bool Clear);