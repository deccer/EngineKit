using System;
using System.IO;
//using Ktx2Sharp;
using EngineKit.Native.Ktx;
using Serilog;

namespace EngineKit.Graphics;

internal sealed class KtxImageLoader : IKtxImageLoader
{
    private readonly ILogger _logger;

    public KtxImageLoader(ILogger logger)
    {
        _logger = logger.ForContext<KtxImageLoader>();
    }

    public Span<byte> LoadImageFromFile(string filePath, Ktx.TranscodeFormat transcodeFormat = Ktx.TranscodeFormat.Bc7Rgba)
    {
        if (!File.Exists(filePath))
        {
            _logger.Debug("{Category}: File does not exist {FileName}", nameof(KtxImageLoader), filePath);
            return Span<byte>.Empty;
        }

        unsafe
        {
            var ktxTexture = Ktx.LoadFromFile(filePath);
            if (ktxTexture->CompressionScheme != Ktx.SuperCompressionScheme.None || Ktx.NeedsTranscoding(ktxTexture))
            {
                var ktxTranscodeResult = Ktx.Transcode(ktxTexture, transcodeFormat, Ktx.TranscodeFlagBits.HighQuality);
                if (ktxTranscodeResult != Ktx.KtxErrorCode.KtxSuccess)
                {
                    _logger.Debug("{Category}: Unable to transcode file to {TranscodeFormat}", nameof(KtxImageLoader), transcodeFormat);
                    return Span<byte>.Empty;
                }
            }

            return new Span<byte>(ktxTexture->Data, (int)ktxTexture->DataSize);
        }
    }
}
