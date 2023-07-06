using System;

namespace EngineKit.Graphics;

public record ImageInformation(
    string Name,
    string? MimeType,
    ReadOnlyMemory<byte>? ImageData,
    string? FileName);