using System;
using EngineKit.Native.Ktx;
//using Ktx2Sharp;

namespace EngineKit.Graphics.Assets;

internal interface IKtxImageLoader
{
    Span<byte> LoadImageFromFile(string filePath, Ktx.TranscodeFormat transcodeFormat = Ktx.TranscodeFormat.Bc7Rgba);
}
