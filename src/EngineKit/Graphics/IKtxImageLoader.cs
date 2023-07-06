using System;
using EngineKit.Native.Ktx;

namespace EngineKit.Graphics;

internal interface IKtxImageLoader
{
    Span<byte> LoadImageFromFile(string filePath, Ktx.TranscodeFormat transcodeFormat = Ktx.TranscodeFormat.Bc7Rgba);
}