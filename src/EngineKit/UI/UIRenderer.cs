using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EngineKit.Graphics;
using EngineKit.Graphics.RHI;
using EngineKit.Input;
using EngineKit.Mathematics;
using EngineKit.Native.Glfw;
using EngineKit.Native.OpenGL;
using ImGuiNET;
using Serilog;

namespace EngineKit.UI;

internal sealed class UIRenderer : IUIRenderer
{
    private const string ImGuiVertexShader = @"
    #version 460 core
    #extension GL_ARB_separate_shader_objects : enable
    #extension GL_ARB_explicit_uniform_location : enable

    layout(location = 0) in vec2 i_position;
    layout(location = 1) in vec2 i_uv;
    layout(location = 2) in vec4 i_color;

    out gl_PerVertex
    {
        vec4 gl_Position;
    };
    layout(location = 1) out vec4 v_color;
    layout(location = 2) out vec2 v_uv;

    layout(std140, binding = 0) uniform GlobalMatrices
    {
        mat4 ProjectionMatrix;
    };

    void main()
    {
        gl_Position = ProjectionMatrix * vec4(i_position, 0, 1);
        v_color = i_color;
        v_uv = i_uv;
    }
    ";

    private const string ImGuiFragmentShader = @"
    #version 460 core
    #extension GL_ARB_separate_shader_objects : enable
    #extension GL_ARB_explicit_uniform_location : enable

    layout(location = 1) in vec4 v_color;
    layout(location = 2) in vec2 v_uv;

    layout(location = 0) out vec4 out_color;

    layout(binding = 0) uniform sampler2D t_font;

    void main()
    {
        out_color = v_color * texture(t_font, v_uv);
    }";

    private static readonly unsafe uint ImDrawVertStride = (uint)sizeof(ImDrawVert);

    private readonly ILogger _logger;
    private readonly IApplicationContext _applicationContext;
    private readonly IGraphicsContext _graphicsContext;
    private readonly IInputProvider _inputProvider;

    private IGraphicsPipeline? _imGuiGraphicsPipeline;
    private IBuffer? _uniformBuffer;

    private bool _frameBegun;

    private IBuffer? _vertexBuffer;
    private uint _vertexBufferSize;
    private IBuffer? _indexBuffer;
    private uint _indexBufferSize;

    private ImGuiIOPtr _imGuiIo;

    private ITexture? _fontTexture;
    private ISampler? _fontSampler;

    private int _framebufferWidth;
    private int _framebufferHeight;

    private int _scrollWheelValue;
    private readonly List<char> _pressedChars;
    private readonly Vector2 _scaleFactor = Vector2.One;

    private readonly IDictionary<string, ImFontPtr> _fonts;
    private readonly IDictionary<Glfw.Key, ImGuiKey> _keyMap;

    private Array _keyValues;

    public UIRenderer(
        ILogger logger,
        IApplicationContext applicationContext,
        IGraphicsContext graphicsContext,
        IInputProvider inputProvider)
    {
        _logger = logger.ForContext<UIRenderer>();
        _applicationContext = applicationContext;
        _graphicsContext = graphicsContext;
        _inputProvider = inputProvider;
        _keyValues = Array.Empty<Glfw.Key>();
        _pressedChars = new List<char>();
        _fonts = new Dictionary<string, ImFontPtr>(16);
        _keyMap = CreateKeyMap();
    }

    public bool AddFont(string name, string filePath, float fontSize)
    {
        if (_fonts.ContainsKey(name))
        {
            return false;
        }

        if (!File.Exists(filePath))
        {
            _logger.Error("{Category} Unable to load font {FileName}", "UIRenderer", filePath);
            return false;
        }

        var fontPtr = _imGuiIo.Fonts.AddFontFromFileTTF(filePath, fontSize);
        RecreateFontDeviceTexture();
        _fonts.Add(name, fontPtr);

        return true;
    }

    public bool Load()
    {
        _framebufferWidth = _applicationContext.WindowFramebufferSize.X;
        _framebufferHeight = _applicationContext.WindowFramebufferSize.Y;

        _keyValues = Enum.GetValuesAsUnderlyingType<Glfw.Key>();

        var imGuiContext = ImGui.CreateContext();
        ImGui.SetCurrentContext(imGuiContext);

        _imGuiIo = ImGui.GetIO();
        _imGuiIo.DisplaySize = new Vector2(_framebufferWidth, _framebufferHeight);
        _imGuiIo.DisplayFramebufferScale = new Vector2(1.0f, 1.0f);
        _imGuiIo.ConfigFlags = ImGuiConfigFlags.DockingEnable;
        _imGuiIo.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset | ImGuiBackendFlags.HasSetMousePos |
                                 ImGuiBackendFlags.HasMouseCursors;
        if (!AddFont("RobotoCondensed-Regular", "Fonts/RobotoCondensed-Regular.ttf", 18))
        {
            return false;
        }

        AddIconFont(15.0f);

        var imGuiGraphicsPipelineResult = _graphicsContext.CreateGraphicsPipelineBuilder()
            .WithShadersFromStrings(ImGuiVertexShader, ImGuiFragmentShader)
            .WithTopology(PrimitiveTopology.Triangles)
            .WithVertexAttributesFromDescriptor(VertexInputDescriptor.ForVertexType(VertexType.ImGui))
            .WithBlendingEnabled(ColorBlendAttachmentDescriptor.PreMultiplied)
            .WithDepthTestDisabled()
            .WithDepthWriteDisabled()
            .Build("ImGuiPass");
        if (imGuiGraphicsPipelineResult.IsFailure)
        {
            _logger.Error(
                "{Category}: Failed to create graphics pipeline - {Details}",
                nameof(UIRenderer),
                imGuiGraphicsPipelineResult.Error);
            return false;
        }

        _imGuiGraphicsPipeline = imGuiGraphicsPipelineResult.Value;

        var mvp = Matrix4x4.CreateOrthographicOffCenter(
            0.0f,
            _imGuiIo.DisplaySize.X,
            _imGuiIo.DisplaySize.Y,
            0.0f,
            -1.0f,
            1.0f);
        _uniformBuffer = _graphicsContext.CreateTypedBuffer<Matrix4x4>("ImGuiProjectionMatrix", 1, BufferStorageFlags.DynamicStorage);
        _uniformBuffer.UpdateElement(mvp, 0u);

        var style = ImGui.GetStyle();
        //SetStyleDarker(style);
        //SetStylePurple(style);
        SetStyleBlack(style);
        style.Colors[(int)ImGuiCol.DockingEmptyBg] = Vector4.Zero;
        style.WindowMenuButtonPosition = ImGuiDir.None;

        CreateDeviceResources();
        SetPerFrameImGuiData(1.0f / 60.0f);

        return true;
    }

    public void ResizeWindow(int width, int height)
    {
        _framebufferWidth = width;
        _framebufferHeight = height;

        var mvp = Matrix4x4.CreateOrthographicOffCenter(
            0.0f,
            _framebufferWidth,
            _framebufferHeight,
            0.0f,
            -1.0f,
            1.0f);
        _uniformBuffer?.UpdateElement(mvp, 0);
    }

    private unsafe void AddIconFont(float fontSize)
    {
        var fonts = _imGuiIo.Fonts;
        var fontRanges = new ushort[]{ MaterialDesignIcons.Min, MaterialDesignIcons.Max, 0 };
        var fontRangesPtr = GCHandle.Alloc(fontRanges, GCHandleType.Pinned);
        {
            ImFontConfigPtr config = ImGuiNative.ImFontConfig_ImFontConfig();
            config.MergeMode = true;
            config.PixelSnapH = true;
            config.GlyphMinAdvanceX = 4.0f;
            config.GlyphOffset.Y = 1.0f;
            config.OversampleH = 1;
            config.OversampleV = 1;
            config.SizePixels = fontSize;
            config.GlyphRanges = fontRangesPtr.AddrOfPinnedObject();

            var fontPtr = GCHandle.Alloc(MaterialDesignIcons.MaterialDesign_compressed_data, GCHandleType.Pinned);
            fonts.AddFontFromMemoryCompressedTTF(fontPtr.AddrOfPinnedObject(), 361568 / 4, fontSize, config);
            fontPtr.Free();
        }

        fonts.Build();
        fontRangesPtr.Free();
    }

    private void DestroyDeviceObjects()
    {
        _fontSampler?.Dispose();
        _fontTexture?.Dispose();
        _indexBuffer?.Dispose();
        _vertexBuffer?.Dispose();
        _uniformBuffer?.Dispose();
    }

    private void CreateDeviceResources()
    {
        _vertexBufferSize = 64 * 1024u;
        _indexBufferSize = 64 * 1024u;

        _vertexBuffer = _graphicsContext.CreateUntypedBuffer("ImGuiVertices", _vertexBufferSize, BufferStorageFlags.DynamicStorage);
        _indexBuffer = _graphicsContext.CreateUntypedBuffer("ImGuiIndices", _indexBufferSize, BufferStorageFlags.DynamicStorage);

        RecreateFontDeviceTexture();

        var samplerDescriptor = new SamplerDescriptor
        {
            Label = "ImGui",
            Anisotropy = TextureSampleCount.SixteenSamples,
            CompareFunction = CompareFunction.Always,
            IsCompareEnabled = false,
            LodBias = 0.0f,
            MinLod = -1000.0f,
            MaxLod = 1000.0f,
            InterpolationFilter = TextureInterpolationFilter.Linear,
            MipmapFilter = TextureMipmapFilter.Linear,
            TextureAddressModeU = TextureAddressMode.ClampToBorder,
            TextureAddressModeV = TextureAddressMode.ClampToBorder
        };
        _fontSampler = _graphicsContext.CreateSampler(samplerDescriptor);
    }

    public void BeginLayout()
    {
        _frameBegun = true;
        ImGui.NewFrame();
        ImGui.DockSpaceOverViewport(0, null, ImGuiDockNodeFlags.PassthruCentralNode);
    }

    public void EndLayout()
    {
        if (_frameBegun)
        {
            _frameBegun = false;
            ImGui.Render();
            GL.Disable(GL.EnableType.FramebufferSrgb);
            GL.Viewport(new Int4(0, 0, _applicationContext.WindowFramebufferSize.X, _applicationContext.WindowFramebufferSize.Y));
            RenderDrawData(ImGui.GetDrawData());
            GL.Enable(GL.EnableType.FramebufferSrgb);
        }
    }

    public void Update(float deltaSeconds)
    {
        SetPerFrameImGuiData(deltaSeconds);
        UpdateImGuiInput();
    }

    public void ShowDemoWindow()
    {
        ImGui.ShowDemoWindow();
    }

    private void RecreateFontDeviceTexture()
    {
        _imGuiIo.Fonts.GetTexDataAsRGBA32(out nint pixels, out var width, out var height, out var bytesPerPixel);

        _fontTexture?.Dispose();
        var createTextureDescriptor = new TextureCreateDescriptor
        {
            Size = new Int3(width, height, 1),
            Format = Format.R8G8B8A8UNorm,
            TextureType = TextureType.Texture2D,
            Label = "ImGuiFontAtlas",
            ArrayLayers = 0,
            MipLevels = 1,
            TextureSampleCount = TextureSampleCount.OneSample
        };
        _fontTexture = _graphicsContext.CreateTexture(createTextureDescriptor);

        var updateTextureDescriptor = new TextureUpdateDescriptor
        {
            Level = 0,
            Offset = Int3.Zero,
            Size = new Int3(width, height, 1),
            UploadDimension = UploadDimension.Two,
            UploadFormat = UploadFormat.BlueGreenRedAlpha,
            UploadType = UploadType.UnsignedByte
        };

        _fontTexture.Update(updateTextureDescriptor, pixels);

        _imGuiIo.Fonts.SetTexID((nint)_fontTexture.Id);
    }

    private void SetPerFrameImGuiData(float deltaSeconds)
    {
        _imGuiIo.DisplaySize = new Vector2(
            _framebufferWidth / _scaleFactor.X,
            _framebufferHeight / _scaleFactor.Y);
        _imGuiIo.DisplayFramebufferScale = new Vector2(_scaleFactor.X, _scaleFactor.Y);
        _imGuiIo.DeltaTime = deltaSeconds;
    }

    private void UpdateImGuiInput()
    {
        var currentMouseState = _inputProvider.MouseState;
        var currentKeyboardState = _inputProvider.KeyboardState;

        _imGuiIo.AddMouseButtonEvent(0, currentMouseState.IsButtonDown(Glfw.MouseButton.ButtonLeft));
        _imGuiIo.AddMouseButtonEvent(1, currentMouseState.IsButtonDown(Glfw.MouseButton.ButtonRight));
        _imGuiIo.AddMouseButtonEvent(2, currentMouseState.IsButtonDown(Glfw.MouseButton.ButtonMiddle));
        _imGuiIo.AddMousePosEvent(currentMouseState.X, currentMouseState.Y);

        var scrollDelta = currentMouseState.Scroll.Y - _scrollWheelValue;
        _imGuiIo.MouseWheel = scrollDelta > 0
            ? 1
            : scrollDelta < 0
                ? -1
                : 0;
        _scrollWheelValue = (int)currentMouseState.Scroll.Y;

        foreach (Glfw.Key key in _keyValues)
        {
            if (key == Glfw.Key.Unknown)
            {
                continue;
            }

            if (_keyMap.TryGetValue(key, out var igKey))
            {
                _imGuiIo.AddKeyEvent(igKey, currentKeyboardState.IsKeyDown(key));
            }
        }

        _imGuiIo.AddKeyEvent(ImGuiKey.ModCtrl, currentKeyboardState[Glfw.Key.KeyLeftCtrl] || currentKeyboardState[Glfw.Key.KeyRightCtrl]);
        _imGuiIo.AddKeyEvent(ImGuiKey.ModAlt, currentKeyboardState[Glfw.Key.KeyLeftAlt] || currentKeyboardState[Glfw.Key.KeyRightAlt]);
        _imGuiIo.AddKeyEvent(ImGuiKey.ModShift, currentKeyboardState[Glfw.Key.KeyLeftShift] || currentKeyboardState[Glfw.Key.KeyRightShift]);
        _imGuiIo.AddKeyEvent(ImGuiKey.ModSuper, currentKeyboardState[Glfw.Key.KeyLeftCtrl] || currentKeyboardState[Glfw.Key.KeyRightCtrl]);

        foreach (var c in _pressedChars)
        {
            _imGuiIo.AddInputCharacter(c);
        }

        _pressedChars.Clear();
    }

    public void CharacterInput(char keyChar)
    {
        _pressedChars.Add(keyChar);
    }

    public void MouseScroll(Vector2 offset)
    {
        _imGuiIo.AddMouseWheelEvent(offset.X, offset.Y);
    }

    private static Dictionary<Glfw.Key, ImGuiKey> CreateKeyMap()
    {
        return new Dictionary<Glfw.Key, ImGuiKey>
        {
            { Glfw.Key.KeyTab, ImGuiKey.Tab },
            { Glfw.Key.KeyArrowLeft, ImGuiKey.LeftArrow },
            { Glfw.Key.KeyArrowRight, ImGuiKey.RightArrow },
            { Glfw.Key.KeyArrowUp, ImGuiKey.UpArrow },
            { Glfw.Key.KeyArrowDown, ImGuiKey.DownArrow },
            { Glfw.Key.KeyPageUp, ImGuiKey.PageUp },
            { Glfw.Key.KeyPageDown, ImGuiKey.PageDown },
            { Glfw.Key.KeyHome, ImGuiKey.Home },
            { Glfw.Key.KeyEnd, ImGuiKey.End },
            { Glfw.Key.KeyInsert, ImGuiKey.Insert },
            { Glfw.Key.KeyDelete, ImGuiKey.Delete },
            { Glfw.Key.KeyBackspace, ImGuiKey.Backspace },
            { Glfw.Key.KeyEnter, ImGuiKey.Enter },

            { Glfw.Key.KeyApostrophe, ImGuiKey.Apostrophe },
            { Glfw.Key.KeyBackslash, ImGuiKey.Backslash },
            { Glfw.Key.KeyBacktick, ImGuiKey.GraveAccent },

            { Glfw.Key.KeyF1, ImGuiKey.F1 },
            { Glfw.Key.KeyF2, ImGuiKey.F2 },
            { Glfw.Key.KeyF3, ImGuiKey.F3 },
            { Glfw.Key.KeyF4, ImGuiKey.F4 },
            { Glfw.Key.KeyF5, ImGuiKey.F5 },
            { Glfw.Key.KeyF6, ImGuiKey.F6 },
            { Glfw.Key.KeyF7, ImGuiKey.F7 },
            { Glfw.Key.KeyF8, ImGuiKey.F8 },
            { Glfw.Key.KeyF9, ImGuiKey.F9 },
            { Glfw.Key.KeyF10, ImGuiKey.F10 },
            { Glfw.Key.KeyF11, ImGuiKey.F11 },
            { Glfw.Key.KeyF12, ImGuiKey.F12 },

            { Glfw.Key.Key0, ImGuiKey._0 },
            { Glfw.Key.Key1, ImGuiKey._1 },
            { Glfw.Key.Key2, ImGuiKey._2 },
            { Glfw.Key.Key3, ImGuiKey._3 },
            { Glfw.Key.Key4, ImGuiKey._4 },
            { Glfw.Key.Key5, ImGuiKey._5 },
            { Glfw.Key.Key6, ImGuiKey._6 },
            { Glfw.Key.Key7, ImGuiKey._7 },
            { Glfw.Key.Key8, ImGuiKey._8 },
            { Glfw.Key.Key9, ImGuiKey._9 },
            { Glfw.Key.KeyA, ImGuiKey.A },
            { Glfw.Key.KeyB, ImGuiKey.B },
            { Glfw.Key.KeyC, ImGuiKey.C },
            { Glfw.Key.KeyD, ImGuiKey.D },
            { Glfw.Key.KeyE, ImGuiKey.E },
            { Glfw.Key.KeyF, ImGuiKey.F },
            { Glfw.Key.KeyG, ImGuiKey.G },
            { Glfw.Key.KeyH, ImGuiKey.H },
            { Glfw.Key.KeyI, ImGuiKey.I },
            { Glfw.Key.KeyJ, ImGuiKey.J },
            { Glfw.Key.KeyK, ImGuiKey.K },
            { Glfw.Key.KeyL, ImGuiKey.L },
            { Glfw.Key.KeyM, ImGuiKey.M },
            { Glfw.Key.KeyN, ImGuiKey.N },
            { Glfw.Key.KeyO, ImGuiKey.O },
            { Glfw.Key.KeyP, ImGuiKey.P },
            { Glfw.Key.KeyQ, ImGuiKey.Q },
            { Glfw.Key.KeyR, ImGuiKey.R },
            { Glfw.Key.KeyS, ImGuiKey.S },
            { Glfw.Key.KeyT, ImGuiKey.T },
            { Glfw.Key.KeyU, ImGuiKey.U },
            { Glfw.Key.KeyV, ImGuiKey.V },
            { Glfw.Key.KeyW, ImGuiKey.W },
            { Glfw.Key.KeyX, ImGuiKey.X },
            { Glfw.Key.KeyY, ImGuiKey.Y },
            { Glfw.Key.KeyZ, ImGuiKey.Z }
        };
    }

    private unsafe void RenderDrawData(ImDrawDataPtr drawDataPtr)
    {
        if (drawDataPtr.CmdListsCount == 0)
        {
            return;
        }

        for (var i = 0; i < drawDataPtr.CmdListsCount; i++)
        {
            var commandList = drawDataPtr.CmdLists[i];
            var vertexSize = commandList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
            if (vertexSize > _vertexBufferSize)
            {
                _vertexBufferSize = (uint)Math.Max(_vertexBufferSize * 1.5f, vertexSize);

                _vertexBuffer?.Dispose();
                _vertexBuffer = _graphicsContext.CreateUntypedBuffer("ImGuiVertices", _vertexBufferSize, BufferStorageFlags.DynamicStorage);
            }

            var indexSize = commandList.IdxBuffer.Size * sizeof(ushort);
            if (indexSize > _indexBufferSize)
            {
                _indexBufferSize = (uint)Math.Max(_indexBufferSize * 1.5f, indexSize);

                _indexBuffer?.Dispose();
                _indexBuffer = _graphicsContext.CreateUntypedBuffer("ImGuiIndices", _indexBufferSize, BufferStorageFlags.DynamicStorage);
            }
        }

        _graphicsContext.BindGraphicsPipeline(_imGuiGraphicsPipeline!);

        _imGuiGraphicsPipeline!.BindAsUniformBuffer(_uniformBuffer!, 0, Offset.Zero, SizeInBytes.Whole);
        _imGuiGraphicsPipeline.BindAsVertexBuffer(_vertexBuffer!, 0, ImDrawVertStride, Offset.Zero);
        _imGuiGraphicsPipeline.BindAsIndexBuffer(_indexBuffer!);
        _imGuiGraphicsPipeline.BindSampledTexture(_fontSampler!, _fontTexture!, 0);

        drawDataPtr.ScaleClipRects(_imGuiIo.DisplayFramebufferScale);

        GL.Enable(GL.EnableType.ScissorTest);
        GL.Disable(GL.EnableType.CullFace);
        GL.Disable(GL.EnableType.DepthTest);

        for (var n = 0; n < drawDataPtr.CmdListsCount; n++)
        {
            var commandList = drawDataPtr.CmdLists[n];

            _vertexBuffer!.UpdateData(
                commandList.VtxBuffer.Data,
                0,
                (uint)(commandList.VtxBuffer.Size * sizeof(ImDrawVert)));
            _indexBuffer!.UpdateData(
                commandList.IdxBuffer.Data,
                0,
                (uint)(commandList.IdxBuffer.Size * sizeof(ushort)));

            var vertexOffset = 0;
            var indexOffset = 0;

            for (var commandIndex = 0; commandIndex < commandList.CmdBuffer.Size; commandIndex++)
            {
                var drawCmdPtr = commandList.CmdBuffer[commandIndex];
                if (drawCmdPtr.UserCallback != IntPtr.Zero)
                {
                    throw new NotImplementedException();
                }

                _imGuiGraphicsPipeline.BindSampledTexture(_fontSampler!, (uint)drawCmdPtr.TextureId, 0);

                var clip = drawCmdPtr.ClipRect;
                GL.Scissor(
                    (int)clip.X,
                    _framebufferHeight - (int)clip.W,
                    (int)(clip.Z - clip.X),
                    (int)(clip.W - clip.Y));

                if ((_imGuiIo.BackendFlags & ImGuiBackendFlags.RendererHasVtxOffset) != 0)
                {
                    GL.DrawElementsBaseVertex(
                        GL.PrimitiveType.Triangles,
                        (int)drawCmdPtr.ElemCount,
                        GL.IndexElementType.UnsignedShort,
                        indexOffset * sizeof(ushort),
                        vertexOffset);
                }
                else
                {
                    GL.DrawElements(
                        GL.PrimitiveType.Triangles,
                        (int)drawCmdPtr.ElemCount,
                        GL.IndexElementType.UnsignedShort,
                        (nint)drawCmdPtr.IdxOffset * sizeof(ushort));
                }

                indexOffset += (int)drawCmdPtr.ElemCount;
            }

            //vertexOffset += commandList.VtxBuffer.Size;
        }

        GL.Disable(GL.EnableType.Blend);
        GL.Disable(GL.EnableType.ScissorTest);
    }

    public void Dispose()
    {
        DestroyDeviceObjects();
    }

    private static void SetStyleBlack(ImGuiStylePtr style)
    {
        ImGui.StyleColorsDark();
        style.Colors[(int)ImGuiCol.Text] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
        style.Colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.50f, 0.50f, 0.50f, 1.00f);
        style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.10f, 0.10f, 0.10f, 1.00f);
        style.Colors[(int)ImGuiCol.ChildBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        style.Colors[(int)ImGuiCol.PopupBg] = new Vector4(0.19f, 0.19f, 0.19f, 0.92f);
        style.Colors[(int)ImGuiCol.Border] = new Vector4(0.19f, 0.19f, 0.19f, 0.29f);
        style.Colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.00f, 0.00f, 0.00f, 0.24f);
        style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.19f, 0.19f, 0.19f, 0.54f);
        style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.00f, 0.00f, 0.00f, 1.00f);
        style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.06f, 0.06f, 0.06f, 1.00f);
        style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.00f, 0.00f, 0.00f, 1.00f);
        style.Colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        style.Colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.34f, 0.34f, 0.34f, 0.54f);
        style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.40f, 0.40f, 0.40f, 0.54f);
        style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.56f, 0.56f, 0.56f, 0.54f);
        style.Colors[(int)ImGuiCol.CheckMark] = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
        style.Colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.34f, 0.34f, 0.34f, 0.54f);
        style.Colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.56f, 0.56f, 0.56f, 0.54f);
        style.Colors[(int)ImGuiCol.Button] = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.19f, 0.19f, 0.19f, 0.54f);
        style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        style.Colors[(int)ImGuiCol.Header] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.00f, 0.00f, 0.00f, 0.36f);
        style.Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.20f, 0.22f, 0.23f, 0.33f);
        style.Colors[(int)ImGuiCol.Separator] = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
        style.Colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.44f, 0.44f, 0.44f, 0.29f);
        style.Colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.40f, 0.44f, 0.47f, 1.00f);
        style.Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
        style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.44f, 0.44f, 0.44f, 0.29f);
        style.Colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.40f, 0.44f, 0.47f, 1.00f);
        style.Colors[(int)ImGuiCol.Tab] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        style.Colors[(int)ImGuiCol.TabHovered] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        style.Colors[(int)ImGuiCol.TabSelected] = new Vector4(0.20f, 0.20f, 0.20f, 0.36f);
        style.Colors[(int)ImGuiCol.TabDimmedSelected] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        style.Colors[(int)ImGuiCol.TabDimmed] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        style.Colors[(int)ImGuiCol.DockingPreview] = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
        style.Colors[(int)ImGuiCol.DockingEmptyBg] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotLines] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotHistogram] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        style.Colors[(int)ImGuiCol.TableHeaderBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        style.Colors[(int)ImGuiCol.TableBorderStrong] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        style.Colors[(int)ImGuiCol.TableBorderLight] = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
        style.Colors[(int)ImGuiCol.TableRowBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        style.Colors[(int)ImGuiCol.TableRowBgAlt] = new Vector4(1.00f, 1.00f, 1.00f, 0.06f);
        style.Colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        style.Colors[(int)ImGuiCol.DragDropTarget] = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
        style.Colors[(int)ImGuiCol.NavHighlight] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 0.00f, 0.00f, 0.70f);
        style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.35f);
        style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.35f);
    }

    private static void SetStylePurple(ImGuiStylePtr style)
    {
        style.Colors[(int)ImGuiCol.Text] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
        style.Colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.50f, 0.50f, 0.50f, 1.00f);
        style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.08f, 0.08f, 0.09f, 1.00f);
        style.Colors[(int)ImGuiCol.ChildBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        style.Colors[(int)ImGuiCol.PopupBg] = new Vector4(0.15f, 0.15f, 0.17f, 0.94f);
        style.Colors[(int)ImGuiCol.Border] = new Vector4(0.37f, 0.31f, 0.57f, 1.00f);
        style.Colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.24f, 0.22f, 0.33f, 1.00f);
        style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.41f, 0.39f, 0.50f, 0.40f);
        style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.41f, 0.40f, 0.50f, 0.62f);
        style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.12f, 0.11f, 0.14f, 1.00f);
        style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.12f, 0.11f, 0.14f, 1.00f);
        style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.00f, 0.00f, 0.00f, 0.51f);
        style.Colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.24f, 0.22f, 0.33f, 1.00f);
        style.Colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.02f, 0.02f, 0.02f, 0.53f);
        style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.31f, 0.31f, 0.31f, 1.00f);
        style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.41f, 0.41f, 0.41f, 1.00f);
        style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.51f, 0.51f, 0.51f, 1.00f);
        style.Colors[(int)ImGuiCol.CheckMark] = new Vector4(0.60f, 0.56f, 0.77f, 1.00f);
        style.Colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.56f, 0.54f, 0.66f, 0.40f);
        style.Colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.76f, 0.73f, 0.88f, 0.40f);
        style.Colors[(int)ImGuiCol.Button] = new Vector4(0.24f, 0.22f, 0.33f, 1.00f);
        style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.32f, 0.29f, 0.44f, 1.00f);
        style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.21f, 0.20f, 0.26f, 0.40f);
        style.Colors[(int)ImGuiCol.Header] = new Vector4(0.31f, 0.29f, 0.37f, 0.40f);
        style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.47f, 0.45f, 0.57f, 0.40f);
        style.Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.21f, 0.20f, 0.25f, 0.40f);
        style.Colors[(int)ImGuiCol.Separator] = new Vector4(0.37f, 0.31f, 0.57f, 1.00f);
        style.Colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.10f, 0.40f, 0.75f, 0.78f);
        style.Colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.10f, 0.40f, 0.75f, 1.00f);
        style.Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.47f, 0.45f, 0.57f, 0.74f);
        style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.59f, 0.57f, 0.71f, 0.74f);
        style.Colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.35f, 0.33f, 0.41f, 0.74f);
        style.Colors[(int)ImGuiCol.Tab] = new Vector4(0.24f, 0.22f, 0.33f, 1.00f);
        style.Colors[(int)ImGuiCol.TabHovered] = new Vector4(0.38f, 0.34f, 0.53f, 1.00f);
        style.Colors[(int)ImGuiCol.TabSelected] = new Vector4(0.24f, 0.22f, 0.33f, 1.00f);
        style.Colors[(int)ImGuiCol.TabDimmed] = new Vector4(0.27f, 0.26f, 0.32f, 0.40f);
        style.Colors[(int)ImGuiCol.TabDimmedSelected] = new Vector4(0.42f, 0.39f, 0.57f, 0.40f);
        //style.Colors[(int)ImGuiCol.TabUnfocusedBorder]     = new Vector4(0.11f, 0.09f, 0.17f, 1.00f);
        style.Colors[(int)ImGuiCol.DockingPreview] = new Vector4(0.58f, 0.54f, 0.80f, 0.78f);
        style.Colors[(int)ImGuiCol.DockingEmptyBg] = new Vector4(0.12f, 0.11f, 0.14f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotLines] = new Vector4(0.61f, 0.61f, 0.61f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.00f, 0.43f, 0.35f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.90f, 0.70f, 0.00f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.00f, 0.60f, 0.00f, 1.00f);
        style.Colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.26f, 0.59f, 0.98f, 0.35f);
        style.Colors[(int)ImGuiCol.DragDropTarget] = new Vector4(1.00f, 1.00f, 0.00f, 0.90f);
        style.Colors[(int)ImGuiCol.NavHighlight] = new Vector4(0.26f, 0.59f, 0.98f, 1.00f);
        style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 1.00f, 1.00f, 0.70f);
        style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.20f);
        style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.35f);
        style.WindowBorderSize = 1.0f;
        style.PopupBorderSize = 0.0f;
        style.FrameRounding = 0.0f;
        style.PopupRounding = 1.0f;
        style.WindowRounding = 0.0f;
        style.ScrollbarRounding = 0.0f;
        style.GrabRounding = 0.0f;
        style.ChildBorderSize = 0.0f;
        style.TabBorderSize = 2.0f;
        style.AntiAliasedLinesUseTex = false;
    }

    private static void SetStyleDarker(ImGuiStylePtr style)
    {
        style.WindowPadding = new Vector2(12, 12);
        style.WindowRounding = 5.0f;
        style.FramePadding = new Vector2(4, 4);
        style.FrameRounding = 4.0f;
        style.ItemSpacing = new Vector2(8, 8);
        style.ItemInnerSpacing = new Vector2(4, 4);
        style.IndentSpacing = 16.0f;
        style.ScrollbarSize = 16.0f;
        style.ScrollbarRounding = 8.0f;
        style.GrabMinSize = 4.0f;
        style.GrabRounding = 3.0f;

        style.Colors[(int)ImGuiCol.Text] = new Vector4(0.80f, 0.80f, 0.83f, 1.00f);
        style.Colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.24f, 0.23f, 0.29f, 1.00f);
        style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.06f, 0.05f, 0.07f, 1.00f);
        style.Colors[(int)ImGuiCol.ChildBg] = new Vector4(0.07f, 0.07f, 0.09f, 1.00f);
        style.Colors[(int)ImGuiCol.PopupBg] = new Vector4(0.07f, 0.07f, 0.09f, 1.00f);
        style.Colors[(int)ImGuiCol.Border] = new Vector4(0.20f, 0.20f, 0.23f, 0.88f);
        style.Colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.92f, 0.91f, 0.88f, 0.00f);
        style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.10f, 0.09f, 0.12f, 1.00f);
        style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.24f, 0.23f, 0.29f, 1.00f);
        style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.56f, 0.56f, 0.58f, 1.00f);
        style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.10f, 0.09f, 0.12f, 1.00f);
        style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(1.00f, 0.98f, 0.95f, 0.75f);
        style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.07f, 0.07f, 0.09f, 1.00f);
        style.Colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.10f, 0.09f, 0.12f, 1.00f);
        style.Colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.10f, 0.09f, 0.12f, 1.00f);
        style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.80f, 0.80f, 0.83f, 0.31f);
        style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.56f, 0.56f, 0.58f, 1.00f);
        style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.06f, 0.05f, 0.07f, 1.00f);
        style.Colors[(int)ImGuiCol.CheckMark] = new Vector4(0.80f, 0.80f, 0.83f, 0.31f);
        style.Colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.80f, 0.80f, 0.83f, 0.31f);
        style.Colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.06f, 0.05f, 0.07f, 1.00f);
        style.Colors[(int)ImGuiCol.Button] = new Vector4(0.10f, 0.09f, 0.12f, 1.00f);
        style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.24f, 0.23f, 0.29f, 1.00f);
        style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.56f, 0.56f, 0.58f, 1.00f);
        style.Colors[(int)ImGuiCol.Header] = new Vector4(0.10f, 0.09f, 0.12f, 1.00f);
        style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.56f, 0.56f, 0.58f, 1.00f);
        style.Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.06f, 0.05f, 0.07f, 1.00f);
        style.Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.56f, 0.56f, 0.58f, 1.00f);
        style.Colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.06f, 0.05f, 0.07f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotLines] = new Vector4(0.40f, 0.39f, 0.38f, 0.63f);
        style.Colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(0.25f, 1.00f, 0.00f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.40f, 0.39f, 0.38f, 0.63f);
        style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(0.25f, 1.00f, 0.00f, 1.00f);
        style.Colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.25f, 1.00f, 0.00f, 0.43f);
    }
}
