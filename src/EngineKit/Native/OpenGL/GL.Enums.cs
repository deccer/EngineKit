using System;

namespace EngineKit.Native.OpenGL;

public static partial class GL
{
    public enum GpuMemoryInfo : uint
    {
        DedicatedVideoMemory = 0x9047,
        TotalAvailableMemory = 0x9048,
        CurrentAvailableVideoMemory = 0x9049,
        EvictionCount = 0x904A,
        EvictedMemory = 0x904B
    }

    public enum StringName : uint
    {
        Vendor = 7936,
        Renderer = 7937,
        Version = 7938,
        Extensions = 7939,
        ShadingLanguageVersion = 35724,
    }

    public enum GetName : uint
    {
        NumExtensions = 0x821D
    }

    [Flags]
    public enum FramebufferBit : uint
    {
        DepthBufferBit = 256,
        StencilBufferBit = 1024,
        ColorBufferBit = 16384,
        AccumBufferBit = 512,
    }

    public enum BlitFramebufferFilter : uint
    {
        Nearest = 9728,
        Linear = 9729,
    }

    public enum FramebufferTarget : uint
    {
        ReadFramebuffer = 0x8CA8,
        DrawFramebuffer = 0x8CA9,
        Framebuffer = 0x8D40
    }

    public enum CompareOperation : uint
    {
        Never = 0x0200,
        Always = 0x0207,
        Less = 0x0201,
        Greater = 0x0204,
        LessOrEqual = 0x0203,
        GreaterOrEqual = 0x0206,
        Equal = 0x0202,
        NotEqual = 0x0205
    }

    public enum Buffer : uint
    {
        Color = 6144,
        Depth = 6145,
        Stencil = 6146,
        DepthStencil = 34041
    }

    public enum PolygonModeType : uint
    {
        Back = 0x0405,
        Front = 0x0404,
        FrontAndBack = 0x0408
    }

    public enum FillMode : uint
    {
        Solid = 0x1B02,
        Line = 0x1B01,
        Point = 0x1B00
    }

    public enum FaceWinding : uint
    {
        Clockwise = 0x0900,
        CounterClockwise = 0x0901
    }

    public enum CullMode : uint
    {
        Front = 1028,
        Back = 1029,
        FrontAndBack = 1032,
    }

    public enum BlendFactor : uint
    {
        Zero = 0,
        One = 1,
        SrcColor = 768,
        OneMinusSrcColor = 769,
        SrcAlpha = 770,
        OneMinusSrcAlpha = 771,
        DstAlpha = 772,
        OneMinusDstAlpha = 773,
        DstColor = 774,
        OneMinusDstColor = 775,
        SrcAlphaSaturate = 776,
        ConstantColor = 32769,
        OneMinusConstantColor = 32770,
        ConstantAlpha = 32771,
        OneMinusConstantAlpha = 32772,
        Src1Alpha = 34185,
        Src1Color = 35065,
        OneMinusSrc1Color = 35066,
        OneMinusSrc1Alpha = 35067,
    }

    public enum BlendOperation : uint
    {
        Add = 0x8006,
        Subtract = 0x800A,
        ReverseSubtract = 0x800B,
        Min = 0x8007,
        Max = 0
    }

    public enum BufferTarget : uint
    {
        ParameterBuffer = 33006,
        ArrayBuffer = 34962,
        ElementArrayBuffer = 34963,
        PixelPackBuffer = 35051,
        PixelUnpackBuffer = 35052,
        TransformFeedbackBuffer = 35982,
        CopyReadBuffer = 36662,
        CopyWriteBuffer = 36663,
        UniformBuffer = 35345,
        DispatchIndirectBuffer = 37102,
        DrawIndirectBuffer = 36671,
        AtomicCounterBuffer = 37568,
        ShaderStorageBuffer = 37074,
        TextureBuffer = 35882,
    }

    public enum BufferUsage : uint
    {
        StreamDraw = 35040,
        StreamRead = 35041,
        StreamCopy = 35042,
        StaticDraw = 35044,
        StaticRead = 35045,
        StaticCopy = 35046,
        DynamicDraw = 35048,
        DynamicRead = 35049,
        DynamicCopy = 35050,
    }

    [Flags]
    public enum BufferStorageFlags : uint
    {
        MapReadBit = 1,
        MapWriteBit = 2,
        MapPersistentBit = 64,
        MapCoherentBit = 128,
        DynamicStorageBit = 256,
        ClientStorageBit = 512,
        SparseStorageBitArb = 1024,
        LgpuSeparateStorageBitNvx = 2048,
        PerGpuStorageBitNv = 2048,
    }

    public enum EnableType : uint
    {
        CullFace = 2884,
        DepthTest = 2929,
        StencilTest = 2960,
        Blend = 3042,
        ScissorTest = 3089,
        AlphaTest = 3008,
        ColorLogicOp = 3058,
        IndexLogicOp = 3057,
        Multisample = 32925,
        SampleAlphaToCoverage = 32926,
        SampleAlphaToOne = 32927,
        SampleCoverage = 32928,
        FramebufferSrgb = 36281,
        PrimitiveRestart = 36765,
        ProgramPointSize = 34370,
        DepthClamp = 34383,
        DebugOutputSynchronous = 33346,
        DebugOutput = 37600,
        PolygonOffsetPoint = 10753,
        PolygonOffsetLine = 10754,
        PolygonOffsetFill = 32823,
        TextureCubemapSeamless = 0x884F
    }

    public enum ShaderType : uint
    {
        VertexShader = 0x8B31,
        FragmentShader = 0x8B30,
        ComputeShader = 0x91B9,
        MeshShader = 0x9559,
        TaskShader = 0x955A
    }

    public enum UseProgramStageMask : uint
    {
        ComputeShaderBit = 32,
        VertexShaderBit = 1,
        FragmentShaderBit = 2,
        AllShaderBits = 4294967295,
        GeometryShaderBit = 4,
        TessControlShaderBit = 8,
        TessEvaluationShaderBit = 16,
        GeometryShaderBitExt = 4,
        VertexShaderBitExt = 1,
        FragmentShaderBitExt = 2,
        AllShaderBitsExt = 4294967295,
        TessControlShaderBitExt = 8,
        TessEvaluationShaderBitExt = 16,
        MeshShaderBitNv = 64,
        TaskShaderBitNv = 128,
        GeometryShaderBitOes = 4,
        TessControlShaderBitOes = 8,
        TessEvaluationShaderBitOes = 16,
    }

    public enum ProgramParameterType : uint
    {
        ProgramBinaryRetrievableHint = 33367,
        ProgramSeparable = 33368,
    }

    public enum ProgramProperty : uint
    {
        DeleteStatus = 35712,
        LinkStatus = 35714,
        ValidateStatus = 35715,
        InfoLogLength = 35716,
        AttachedShaders = 35717,
        ActiveUniforms = 35718,
        ActiveUniformMaxLength = 35719,
        ActiveAttributes = 35721,
        ActiveAttributeMaxLength = 35722,
        TransformFeedbackVaryingMaxLength = 35958,
        TransformFeedbackBufferMode = 35967,
        TransformFeedbackVaryings = 35971,
        ActiveUniformBlockMaxNameLength = 35381,
        ActiveUniformBlocks = 35382,
        GeometryVerticesOut = 35094,
        GeometryInputType = 35095,
        GeometryOutputType = 35096,
        ProgramBinaryLength = 34625,
        ActiveAtomicCounterBuffers = 37593,
        ComputeWorkGroupSize = 33383,
    }

    public enum LogicOperation : uint
    {
        Clear = 0x1500,
        Set = 0x150F,
        Copy = 0x1503,
        CopyInverted = 0x150C,
        NoOperation = 0x1505,
        Invert = 0x150A,
        And = 0x1501,
        Nand = 0x150E,
        Or = 0x1507,
        Nor = 0x1508,
        Xor = 0x1506,
        Equivalent = 0x1509,
        AndReverse = 0x1502,
        OrReverse = 0x150B,
        AndInverted = 0x1504,
        OrInverted = 0x150D,
    }

    public enum SizedInternalFormat : uint
    {
        R3G3B2 = 10768,
        Rgb4 = 32847,
        Rgb5 = 32848,
        Rgb8 = 32849,
        Rgb10 = 32850,
        Rgb12 = 32851,
        Rgb16 = 32852,
        Rgba2 = 32853,
        Rgba4 = 32854,
        Rgb5A1 = 32855,
        Rgba8 = 32856,
        Rgb10A2 = 32857,
        Rgba12 = 32858,
        Rgba16 = 32859,
        Alpha4 = 32827,
        Alpha8 = 32828,
        Alpha12 = 32829,
        Alpha16 = 32830,
        Luminance4 = 32831,
        Luminance8 = 32832,
        Luminance12 = 32833,
        Luminance16 = 32834,
        Luminance4Alpha4 = 32835,
        Luminance6Alpha2 = 32836,
        Luminance8Alpha8 = 32837,
        Luminance12Alpha4 = 32838,
        Luminance12Alpha12 = 32839,
        Luminance16Alpha16 = 32840,
        Intensity4 = 32842,
        Intensity8 = 32843,
        Intensity12 = 32844,
        Intensity16 = 32845,
        DepthComponent16 = 33189,
        DepthComponent24 = 33190,
        DepthComponent32 = 33191,
        Srgb8 = 35905,
        Srgb8Alpha8 = 35907,
        Rgba32f = 34836,
        Rgb32f = 34837,
        Rgba16f = 34842,
        Rgb16f = 34843,
        R11fG11fB10f = 35898,
        Rgb9E5 = 35901,
        Rgba32ui = 36208,
        Rgb32ui = 36209,
        Rgba16ui = 36214,
        Rgb16ui = 36215,
        Rgba8ui = 36220,
        Rgb8ui = 36221,
        Rgba32i = 36226,
        Rgb32i = 36227,
        Rgba16i = 36232,
        Rgb16i = 36233,
        Rgba8i = 36238,
        Rgb8i = 36239,
        DepthComponent32f = 36012,
        Depth32fStencil8 = 36013,
        Depth24Stencil8 = 35056,
        StencilIndex1 = 36166,
        StencilIndex4 = 36167,
        StencilIndex8 = 36168,
        StencilIndex16 = 36169,
        CompressedRedRgtc1 = 36283,
        CompressedSignedRedRgtc1 = 36284,
        CompressedRgRgtc2 = 36285,
        CompressedSignedRgRgtc2 = 36286,
        R8 = 33321,
        R16 = 33322,
        Rg8 = 33323,
        Rg16 = 33324,
        R16f = 33325,
        R32f = 33326,
        Rg16f = 33327,
        Rg32f = 33328,
        R8i = 33329,
        R8ui = 33330,
        R16i = 33331,
        R16ui = 33332,
        R32i = 33333,
        R32ui = 33334,
        Rg8i = 33335,
        Rg8ui = 33336,
        Rg16i = 33337,
        Rg16ui = 33338,
        Rg32i = 33339,
        Rg32ui = 33340,
        R8Snorm = 36756,
        Rg8Snorm = 36757,
        Rgb8Snorm = 36758,
        Rgba8Snorm = 36759,
        R16Snorm = 36760,
        Rg16Snorm = 36761,
        Rgb16Snorm = 36762,
        Rgba16Snorm = 36763,
        Rgb10A2ui = 36975,
        CompressedRgbaBptcUnorm = 36492,
        CompressedSrgbAlphaBptcUnorm = 36493,
        CompressedRgbBptcSignedFloat = 36494,
        CompressedRgbBptcUnsignedFloat = 36495,
        CompressedRgb8Etc2 = 37492,
        CompressedSrgb8Etc2 = 37493,
        CompressedRgb8PunchthroughAlpha1Etc2 = 37494,
        CompressedSrgb8PunchthroughAlpha1Etc2 = 37495,
        CompressedRgba8Etc2Eac = 37496,
        CompressedSrgb8Alpha8Etc2Eac = 37497,
        CompressedR11Eac = 37488,
        CompressedSignedR11Eac = 37489,
        CompressedRg11Eac = 37490,
        CompressedSignedRg11Eac = 37491,
        DepthComponent16Arb = 33189,
        DepthComponent24Arb = 33190,
        DepthComponent32Arb = 33191,
        CompressedRgbaBptcUnormArb = 36492,
        CompressedSrgbAlphaBptcUnormArb = 36493,
        CompressedRgbBptcSignedFloatArb = 36494,
        CompressedRgbBptcUnsignedFloatArb = 36495,
        Rgba32fArb = 34836,
        Rgb32fArb = 34837,
        Rgba16fArb = 34842,
        Rgb16fArb = 34843,
        StencilIndex1Ext = 36166,
        StencilIndex4Ext = 36167,
        StencilIndex8Ext = 36168,
        StencilIndex16Ext = 36169,
        Depth24Stencil8Ext = 35056,
        R11fG11fB10fExt = 35898,
        Alpha4Ext = 32827,
        Alpha8Ext = 32828,
        Alpha12Ext = 32829,
        Alpha16Ext = 32830,
        Luminance4Ext = 32831,
        Luminance8Ext = 32832,
        Luminance12Ext = 32833,
        Luminance16Ext = 32834,
        Luminance4Alpha4Ext = 32835,
        Luminance6Alpha2Ext = 32836,
        Luminance8Alpha8Ext = 32837,
        Luminance12Alpha4Ext = 32838,
        Luminance12Alpha12Ext = 32839,
        Luminance16Alpha16Ext = 32840,
        Intensity4Ext = 32842,
        Intensity8Ext = 32843,
        Intensity12Ext = 32844,
        Intensity16Ext = 32845,
        Rgb2Ext = 32846,
        Rgb4Ext = 32847,
        Rgb5Ext = 32848,
        Rgb8Ext = 32849,
        Rgb10Ext = 32850,
        Rgb5A1Ext = 32855,
        Rgba8Ext = 32856,
        Rgb10A2Ext = 32857,
        Rgba12Ext = 32858,
        Rgba16Ext = 32859,
        CompressedRedRgtc1Ext = 36283,
        CompressedSignedRedRgtc1Ext = 36284,
        CompressedRedGreenRgtc2Ext = 36285,
        CompressedSignedRedGreenRgtc2Ext = 36286,
        CompressedRgbS3tcDxt1Ext = 33776,
        CompressedRgbaS3tcDxt1Ext = 33777,
        CompressedRgbaS3tcDxt3Ext = 33778,
        CompressedRgbaS3tcDxt5Ext = 33779,
        Rgba32uiExt = 36208,
        Rgb32uiExt = 36209,
        Alpha32uiExt = 36210,
        Intensity32uiExt = 36211,
        Luminance32uiExt = 36212,
        LuminanceAlpha32uiExt = 36213,
        Rgba16uiExt = 36214,
        Rgb16uiExt = 36215,
        Alpha16uiExt = 36216,
        Intensity16uiExt = 36217,
        Luminance16uiExt = 36218,
        LuminanceAlpha16uiExt = 36219,
        Rgba8uiExt = 36220,
        Rgb8uiExt = 36221,
        Alpha8uiExt = 36222,
        Intensity8uiExt = 36223,
        Luminance8uiExt = 36224,
        LuminanceAlpha8uiExt = 36225,
        Rgba32iExt = 36226,
        Rgb32iExt = 36227,
        Alpha32iExt = 36228,
        Intensity32iExt = 36229,
        Luminance32iExt = 36230,
        LuminanceAlpha32iExt = 36231,
        Rgba16iExt = 36232,
        Rgb16iExt = 36233,
        Alpha16iExt = 36234,
        Intensity16iExt = 36235,
        Luminance16iExt = 36236,
        LuminanceAlpha16iExt = 36237,
        Rgba8iExt = 36238,
        Rgb8iExt = 36239,
        Alpha8iExt = 36240,
        Intensity8iExt = 36241,
        Luminance8iExt = 36242,
        LuminanceAlpha8iExt = 36243,
        Srgb8Ext = 35905,
        Srgb8Alpha8Ext = 35907,
        CompressedSrgbS3tcDxt1Ext = 35916,
        CompressedSrgbAlphaS3tcDxt1Ext = 35917,
        CompressedSrgbAlphaS3tcDxt3Ext = 35918,
        CompressedSrgbAlphaS3tcDxt5Ext = 35919,
        Rgb9E5Ext = 35901,
        CompressedRgbaAstc4x4Khr = 37808,
        CompressedRgbaAstc5x4Khr = 37809,
        CompressedRgbaAstc5x5Khr = 37810,
        CompressedRgbaAstc6x5Khr = 37811,
        CompressedRgbaAstc6x6Khr = 37812,
        CompressedRgbaAstc8x5Khr = 37813,
        CompressedRgbaAstc8x6Khr = 37814,
        CompressedRgbaAstc8x8Khr = 37815,
        CompressedRgbaAstc10x5Khr = 37816,
        CompressedRgbaAstc10x6Khr = 37817,
        CompressedRgbaAstc10x8Khr = 37818,
        CompressedRgbaAstc10x10Khr = 37819,
        CompressedRgbaAstc12x10Khr = 37820,
        CompressedRgbaAstc12x12Khr = 37821,
        CompressedSrgb8Alpha8Astc4x4Khr = 37840,
        CompressedSrgb8Alpha8Astc5x4Khr = 37841,
        CompressedSrgb8Alpha8Astc5x5Khr = 37842,
        CompressedSrgb8Alpha8Astc6x5Khr = 37843,
        CompressedSrgb8Alpha8Astc6x6Khr = 37844,
        CompressedSrgb8Alpha8Astc8x5Khr = 37845,
        CompressedSrgb8Alpha8Astc8x6Khr = 37846,
        CompressedSrgb8Alpha8Astc8x8Khr = 37847,
        CompressedSrgb8Alpha8Astc10x5Khr = 37848,
        CompressedSrgb8Alpha8Astc10x6Khr = 37849,
        CompressedSrgb8Alpha8Astc10x8Khr = 37850,
        CompressedSrgb8Alpha8Astc10x10Khr = 37851,
        CompressedSrgb8Alpha8Astc12x10Khr = 37852,
        CompressedSrgb8Alpha8Astc12x12Khr = 37853,
        DepthComponent32fNv = 36267,
        Depth32fStencil8Nv = 36268,
        DepthComponent16Sgix = 33189,
        DepthComponent24Sgix = 33190,
        DepthComponent32Sgix = 33191,
    }

    public enum BlendEquationMode : uint
    {
        FuncAdd = 32774,
        FuncReverseSubtract = 32779,
        FuncSubtract = 32778,
        Min = 32775,
        Max = 32776,
        MinExt = 32775,
        MaxExt = 32776,
        FuncAddExt = 32774,
        FuncSubtractExt = 32778,
        FuncReverseSubtractExt = 32779,
        AlphaMinSgix = 33568,
        AlphaMaxSgix = 33569,
    }

    public enum TextureUnit : uint
    {
        Texture0 = 33984,
        Texture1 = 33985,
        Texture2 = 33986,
        Texture3 = 33987,
        Texture4 = 33988,
        Texture5 = 33989,
        Texture6 = 33990,
        Texture7 = 33991,
        Texture8 = 33992,
        Texture9 = 33993,
        Texture10 = 33994,
        Texture11 = 33995,
        Texture12 = 33996,
        Texture13 = 33997,
        Texture14 = 33998,
        Texture15 = 33999,
        Texture16 = 34000,
        Texture17 = 34001,
        Texture18 = 34002,
        Texture19 = 34003,
        Texture20 = 34004,
        Texture21 = 34005,
        Texture22 = 34006,
        Texture23 = 34007,
        Texture24 = 34008,
        Texture25 = 34009,
        Texture26 = 34010,
        Texture27 = 34011,
        Texture28 = 34012,
        Texture29 = 34013,
        Texture30 = 34014,
        Texture31 = 34015,
    }

    public enum TextureTarget : uint
    {
        Texture1d = 3552,
        Texture2d = 3553,
        ProxyTexture1d = 32867,
        ProxyTexture2d = 32868,
        Texture3d = 32879,
        ProxyTexture3d = 32880,
        TextureCubeMap = 34067,
        TextureCubeMapPositiveX = 34069,
        TextureCubeMapNegativeX = 34070,
        TextureCubeMapPositiveY = 34071,
        TextureCubeMapNegativeY = 34072,
        TextureCubeMapPositiveZ = 34073,
        TextureCubeMapNegativeZ = 34074,
        ProxyTextureCubeMap = 34075,
        Texture1dArray = 35864,
        ProxyTexture1dArray = 35865,
        Texture2dArray = 35866,
        ProxyTexture2dArray = 35867,
        TextureBuffer = 35882,
        TextureRectangle = 34037,
        ProxyTextureRectangle = 34039,
        Texture2dMultisample = 37120,
        ProxyTexture2dMultisample = 37121,
        Texture2dMultisampleArray = 37122,
        ProxyTexture2dMultisampleArray = 37123,
        TextureCubeMapArray = 36873,
        ProxyTextureCubeMapArray = 36875,
        ProxyTextureCubeMapArb = 34075,
        TextureCubeMapArrayArb = 36873,
        ProxyTextureCubeMapArrayArb = 36875,
        ProxyTextureRectangleArb = 34039,
        ProxyTexture1dExt = 32867,
        ProxyTexture2dExt = 32868,
        Texture3dExt = 32879,
        ProxyTexture3dExt = 32880,
        ProxyTexture1dArrayExt = 35865,
        ProxyTexture2dArrayExt = 35867,
        ProxyTextureCubeMapExt = 34075,
        ProxyTextureRectangleNv = 34039,
        DetailTexture2dSgis = 32917,
        Texture4dSgis = 33076,
        ProxyTexture4dSgis = 33077,
    }

    public enum ObjectIdentifier : uint
    {
        Texture = 5890,
        VertexArray = 32884,
        Framebuffer = 36160,
        Renderbuffer = 36161,
        TransformFeedback = 36386,
        Buffer = 33504,
        Shader = 33505,
        Program = 33506,
        Query = 33507,
        ProgramPipeline = 33508,
        Sampler = 33510,
    }

    public enum DataType : uint
    {
        Byte = 5120,
        UnsignedByte = 5121,
        Short = 5122,
        UnsignedShort = 5123,
        Int = 5124,
        UnsignedInt = 5125,
        Float = 5126,
        Bitmap = 6656,
        UnsignedByte332 = 32818,
        UnsignedShort4444 = 32819,
        UnsignedShort5551 = 32820,
        UnsignedInt8888 = 32821,
        UnsignedInt1010102 = 32822,
        UnsignedByte332Ext = 32818,
        UnsignedShort4444Ext = 32819,
        UnsignedShort5551Ext = 32820,
        UnsignedInt8888Ext = 32821,
        UnsignedInt1010102Ext = 32822,
    }

    public enum PixelFormat : uint
    {
        UnsignedShort = 5123,
        UnsignedInt = 5125,
        StencilIndex = 6401,
        DepthComponent = 6402,
        Red = 6403,
        Green = 6404,
        Blue = 6405,
        Alpha = 6406,
        Rgb = 6407,
        Rgba = 6408,
        ColorIndex = 6400,
        Luminance = 6409,
        LuminanceAlpha = 6410,
        Bgr = 32992,
        Bgra = 32993,
        RedInteger = 36244,
        GreenInteger = 36245,
        BlueInteger = 36246,
        RgbInteger = 36248,
        RgbaInteger = 36249,
        BgrInteger = 36250,
        BgraInteger = 36251,
        DepthStencil = 34041,
        Rg = 33319,
        RgInteger = 33320,
        AbgrExt = 32768,
        CmykExt = 32780,
        CmykaExt = 32781,
        Ycrcb422Sgix = 33211,
        Ycrcb444Sgix = 33212,
    }

    public enum AddressMode : uint
    {
        LinearMipmapLinear = 9987,
        Repeat = 10497,
        Clamp = 10496,
        ClampToEdge = 33071,
        ClampToBorder = 33069,
        MirroredRepeat = 33648
    }

    public enum FramebufferAttachment : uint
    {
        ColorAttachment0 = 36064,
        DepthAttachment = 36096,
        StencilAttachment = 36128,
        DepthStencilAttachment = 33306,
        ColorAttachment1 = 36065,
        ColorAttachment2 = 36066,
        ColorAttachment3 = 36067,
        ColorAttachment4 = 36068,
        ColorAttachment5 = 36069,
        ColorAttachment6 = 36070,
        ColorAttachment7 = 36071,
        ColorAttachment8 = 36072,
        ColorAttachment9 = 36073,
        ColorAttachment10 = 36074,
        ColorAttachment11 = 36075,
        ColorAttachment12 = 36076,
        ColorAttachment13 = 36077,
        ColorAttachment14 = 36078,
        ColorAttachment15 = 36079,
        ColorAttachment16 = 36080,
        ColorAttachment17 = 36081,
        ColorAttachment18 = 36082,
        ColorAttachment19 = 36083,
        ColorAttachment20 = 36084,
        ColorAttachment21 = 36085,
        ColorAttachment22 = 36086,
        ColorAttachment23 = 36087,
        ColorAttachment24 = 36088,
        ColorAttachment25 = 36089,
        ColorAttachment26 = 36090,
        ColorAttachment27 = 36091,
        ColorAttachment28 = 36092,
        ColorAttachment29 = 36093,
        ColorAttachment30 = 36094,
        ColorAttachment31 = 36095,
    }

    public enum FramebufferAttachmentParameterName : uint
    {
        FramebufferAttachmentColorEncoding = 33296,
        FramebufferAttachmentComponentType = 33297,
        FramebufferAttachmentRedSize = 33298,
        FramebufferAttachmentGreenSize = 33299,
        FramebufferAttachmentBlueSize = 33300,
        FramebufferAttachmentAlphaSize = 33301,
        FramebufferAttachmentDepthSize = 33302,
        FramebufferAttachmentStencilSize = 33303,
        FramebufferAttachmentObjectType = 36048,
        FramebufferAttachmentObjectName = 36049,
        FramebufferAttachmentTextureLevel = 36050,
        FramebufferAttachmentTextureCubeMapFace = 36051,
        FramebufferAttachmentTextureLayer = 36052,
        FramebufferAttachmentLayered = 36263,
        FramebufferAttachmentLayeredArb = 36263,
        FramebufferAttachmentObjectTypeExt = 36048,
        FramebufferAttachmentObjectNameExt = 36049,
        FramebufferAttachmentTextureLevelExt = 36050,
        FramebufferAttachmentTextureCubeMapFaceExt = 36051,
        FramebufferAttachmentTexture3dZoffsetExt = 36052,
        FramebufferAttachmentLayeredExt = 36263,
        FramebufferAttachmentTextureLayerExt = 36052,
        FramebufferAttachmentTextureNumViewsOvr = 38448,
        FramebufferAttachmentTextureBaseViewIndexOvr = 38450,
    }

    public enum FramebufferStatus : uint
    {
        FramebufferUndefined = 33305,
        FramebufferComplete = 36053,
        FramebufferIncompleteAttachment = 36054,
        FramebufferIncompleteMissingAttachment = 36055,
        FramebufferIncompleteDrawBuffer = 36059,
        FramebufferIncompleteReadBuffer = 36060,
        FramebufferUnsupported = 36061,
        FramebufferIncompleteMultisample = 36182,
        FramebufferIncompleteLayerTargets = 36264,
    }

    public enum DrawBuffer : uint
    {
        None = 0,
        FrontLeft = 1024,
        FrontRight = 1025,
        BackLeft = 1026,
        BackRight = 1027,
        Front = 1028,
        Back = 1029,
        Left = 1030,
        Right = 1031,
        FrontAndBack = 1032,
        Aux0 = 1033,
        Aux1 = 1034,
        Aux2 = 1035,
        Aux3 = 1036,
        ColorAttachment0 = 36064,
        ColorAttachment1 = 36065,
        ColorAttachment2 = 36066,
        ColorAttachment3 = 36067,
        ColorAttachment4 = 36068,
        ColorAttachment5 = 36069,
        ColorAttachment6 = 36070,
        ColorAttachment7 = 36071,
        ColorAttachment8 = 36072,
        ColorAttachment9 = 36073,
        ColorAttachment10 = 36074,
        ColorAttachment11 = 36075,
        ColorAttachment12 = 36076,
        ColorAttachment13 = 36077,
        ColorAttachment14 = 36078,
        ColorAttachment15 = 36079,
        ColorAttachment16 = 36080,
        ColorAttachment17 = 36081,
        ColorAttachment18 = 36082,
        ColorAttachment19 = 36083,
        ColorAttachment20 = 36084,
        ColorAttachment21 = 36085,
        ColorAttachment22 = 36086,
        ColorAttachment23 = 36087,
        ColorAttachment24 = 36088,
        ColorAttachment25 = 36089,
        ColorAttachment26 = 36090,
        ColorAttachment27 = 36091,
        ColorAttachment28 = 36092,
        ColorAttachment29 = 36093,
        ColorAttachment30 = 36094,
        ColorAttachment31 = 36095,
    }

    public enum PrimitiveType : uint
    {
        Points = 0,
        Lines = 1,
        LineLoop = 2,
        LineStrip = 3,
        Triangles = 4,
        TriangleStrip = 5,
        TriangleFan = 6
    }

    public enum DebugSeverity : uint
    {
        DontCare = 4352,
        High = 37190,
        Medium = 37191,
        Low = 37192,
        Notification = 33387,
    }

    public enum DebugSource : uint
    {
        DontCare = 4352,
        Api = 33350,
        WindowSystem = 33351,
        ShaderCompiler = 33352,
        ThirdParty = 33353,
        Application = 33354,
        Other = 33355,
    }

    public enum DebugType : uint
    {
        DontCare = 4352,
        Error = 33356,
        DeprecatedBehavior = 33357,
        UndefinedBehavior = 33358,
        Portability = 33359,
        Performance = 33360,
        Other = 33361,
        Marker = 33384,
        PushGroup = 33385,
        PopGroup = 33386,
    }

    public enum IndexElementType : uint
    {
        UnsignedByte = 5121,
        UnsignedShort = 5123,
        UnsignedInt = 5125,
    }

    public enum SamplerParameterI : uint
    {
        TextureBorderColor = 4100,
        TextureMagFilter = 10240,
        TextureMinFilter = 10241,
        TextureWrapS = 10242,
        TextureWrapT = 10243,
        TextureWrapR = 32882,
        TextureCompareMode = 34892,
        TextureCompareFunc = 34893,
    }

    public enum SamplerParameterF : uint
    {
        TextureBorderColor = 4100,
        TextureMinLod = 33082,
        TextureMaxLod = 33083,
        TextureLodBias = 34049,
        TextureMaxAnisotropy = 34046,
    }

    public enum TextureCompareMode : uint
    {
        None = 0,
        CompareRefToTexture = 34894,
    }

    public enum Filter : uint
    {
        Nearest = 0x2600,
        Linear = 0x2601,
        NearestMipmapNearest = 0x2700,
        LinearMipmapNearest = 0x2701,
        NearestMipmapLinear = 0x2702,
        LinearMipmapLinear = 0x2703
    }

    public enum QueryObjectParameterName : uint
    {
        QueryResult = 34918,
        QueryResultAvailable = 34919,
        QueryResultNoWait = 37268,
        QueryTarget = 33514,
    }

    public enum QueryTarget : uint
    {
        SamplesPassed = 35092,
        PrimitivesGenerated = 35975,
        TransformFeedbackPrimitivesWritten = 35976,
        AnySamplesPassed = 35887,
        TimeElapsed = 35007,
        AnySamplesPassedConservative = 36202,
        VerticesSubmitted = 33518,
        PrimitivesSubmitted = 33519,
        VertexShaderInvocations = 33520,
        TransformFeedbackOverflow = 33516,
    }

    public enum TextureParameterName : uint
    {
        TextureWidth = 4096,
        TextureHeight = 4097,
        TextureBorderColor = 4100,
        TextureMagFilter = 10240,
        TextureMinFilter = 10241,
        TextureWrapS = 10242,
        TextureWrapT = 10243,
        TextureComponents = 4099,
        TextureBorder = 4101,
        TextureInternalFormat = 4099,
        TextureRedSize = 32860,
        TextureGreenSize = 32861,
        TextureBlueSize = 32862,
        TextureAlphaSize = 32863,
        TextureLuminanceSize = 32864,
        TextureIntensitySize = 32865,
        TexturePriority = 32870,
        TextureResident = 32871,
        TextureWrapR = 32882,
        TextureMinLod = 33082,
        TextureMaxLod = 33083,
        TextureBaseLevel = 33084,
        TextureMaxLevel = 33085,
        TextureLodBias = 34049,
        TextureCompareMode = 34892,
        TextureCompareFunc = 34893,
        GenerateMipmap = 33169,
        
        TextureSwizzleR = 36418,
        TextureSwizzleG = 36419,
        TextureSwizzleB = 36420,
        TextureSwizzleA = 36421,
        TextureSwizzleRgba = 36422,
        
        DepthStencilTextureMode = 37098,
        TextureMaxAnisotropy = 34046,
        TextureTilingExt = 38272,
        TextureDepthExt = 32881,
        TextureWrapRExt = 32882,
        TexturePriorityExt = 32870,
        TextureMemoryLayoutIntel = 33791,
    }

    [Flags]
    public enum MemoryBarrierMask : uint
    {
        VertexAttribArrayBarrierBit = 1,
        ElementArrayBarrierBit = 2,
        UniformBarrierBit = 4,
        TextureFetchBarrierBit = 8,
        ShaderGlobalAccessBarrierBitNv = 16,
        ShaderImageAccessBarrierBit = 32,
        CommandBarrierBit = 64,
        PixelBufferBarrierBit = 128,
        TextureUpdateBarrierBit = 256,
        BufferUpdateBarrierBit = 512,
        FramebufferBarrierBit = 1024,
        TransformFeedbackBarrierBit = 2048,
        AtomicCounterBarrierBit = 4096,
        ShaderStorageBarrierBit = 8192,
        ClientMappedBufferBarrierBit = 16384,
        QueryBufferBarrierBit = 32768,
        AllBarrierBits = 4294967295,
    }

    public enum MemoryAccess : uint
    {
        ReadOnly = 0x88B8,
        WriteOnly = 0x88B9,
        ReadWrite = 0x88BA
    }

    [Flags]
    public enum MapFlags : uint
    {
        Read = 0x0001,
        Write = 0x0002,
        InvalidateRange = 0x0004,
        InvalidateBuffer = 0x0008,
        FlushExplicit = 0x0010,
        UnSynchronized = 0x0020,
        Persistent = 0x0040,
        Coherent = 0x0080
    }

    public enum ClipControlOrigin : uint
    {
        LowerLeft = 36001,
        UpperLeft = 36002,
    }
    
    public enum ClipControlDepth : uint
    {
        NegativeOneToOne = 37726,
        ZeroToOne = 37727,
    }
}