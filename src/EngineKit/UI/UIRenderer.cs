using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EngineKit.Graphics;
using EngineKit.Input;
using EngineKit.Mathematics;
using EngineKit.Native.Glfw;
using EngineKit.Native.OpenGL;
using ImGuiNET;
using Serilog;
using Num = System.Numerics;
using Vector2 = EngineKit.Mathematics.Vector2;

namespace EngineKit.UI;

internal sealed class UIRenderer : IUIRenderer
{
    private const string ImGuiVertexShader = @"
    #version 460 core
    #extension GL_ARB_separate_shader_objects : enable
    #extension GL_ARB_explicit_uniform_location : enable

    layout(location = 0) in vec2 in_position;
    layout(location = 1) in vec2 in_uv;
    layout(location = 2) in vec4 in_color;

        out gl_PerVertex
    {
        vec4 gl_Position;
    };
    layout(location = 1) out vec4 fs_color;
    layout(location = 2) out vec2 fs_uv;

    layout(std140, binding = 0) uniform GlobalMatrices
    {
        mat4 ProjectionMatrix;
    };

    void main()
    {
        gl_Position = ProjectionMatrix * vec4(in_position, 0, 1);
        fs_color = in_color;
        fs_uv = in_uv;
    }
        ";

    private const string ImGuiFragmentShader = @"
    #version 460 core
    #extension GL_ARB_separate_shader_objects : enable
    #extension GL_ARB_explicit_uniform_location : enable

    layout(location = 1) in vec4 fs_color;
    layout(location = 2) in vec2 fs_uv;

    layout(location = 0) out vec4 out_color;

    layout(binding = 0) uniform sampler2D t_font;

    void main()
    {
        out_color = fs_color * texture(t_font, fs_uv);
    }";

    private IGraphicsPipeline? _imGuiGraphicsPipeline;
    private IUniformBuffer? _uniformBuffer;

    private bool _frameBegun;

    private IVertexBuffer? _vertexBuffer;
    private uint _vertexBufferSize;
    private IIndexBuffer? _indexBuffer;
    private uint _indexBufferSize;

    private ImGuiIOPtr _imGuiIo;

    private ITexture? _fontTexture;
    private Sampler? _fontSampler;

    private readonly ILogger _logger;
    private readonly IGraphicsContext _graphicsContext;
    private readonly IInputProvider _inputProvider;

    private int _framebufferWidth;
    private int _framebufferHeight;

    private int _scrollWheelValue;
    private readonly List<char> _pressedChars = new List<char>();

    private Num.Vector2 _scaleFactor = Num.Vector2.One;

    public UIRenderer(
        ILogger logger,
        IGraphicsContext graphicsContext,
        IInputProvider inputProvider)
    {
        _logger = logger.ForContext<UIRenderer>();
        _graphicsContext = graphicsContext;
        _inputProvider = inputProvider;
    }

    public bool Load(int width, int height, Action<ImGuiIOPtr>? configureIo = null)
    {
        _framebufferWidth = width;
        _framebufferHeight = height;

        var imGuiGraphicsPipelineResult = _graphicsContext.CreateGraphicsPipelineBuilder()
            .WithShadersFromStrings(ImGuiVertexShader, ImGuiFragmentShader)
            .WithTopology(PrimitiveTopology.Triangles)
            .WithVertexInput(new VertexInputDescriptorBuilder()
                .AddAttribute(0, DataType.Float, 2, 0)
                .AddAttribute(0, DataType.Float, 2, 8)
                .AddAttribute(0, DataType.UnsignedByte, 4, 16, true)
                .Build("UI"))
            .EnableBlending(ColorBlendAttachmentDescriptor.PreMultiplied)
            .DisableDepthTest()
            .DisableDepthWrite()
            .Build("ImGuiPipeline");
        if (imGuiGraphicsPipelineResult.IsFailure)
        {
            _logger.Error(
                "{Category}: Failed to create graphics pipeline - {Details}",
                nameof(UIRenderer),
                imGuiGraphicsPipelineResult.Error);
            return false;
        }

        _imGuiGraphicsPipeline = imGuiGraphicsPipelineResult.Value;

        var imGuiContext = ImGui.CreateContext();
        ImGui.SetCurrentContext(imGuiContext);

        _imGuiIo = ImGui.GetIO();
        _imGuiIo.DisplaySize = new Num.Vector2(_framebufferWidth, _framebufferHeight);
        _imGuiIo.DisplayFramebufferScale = new Num.Vector2(1.0f, 1.0f);
        _imGuiIo.ConfigFlags = ImGuiConfigFlags.DockingEnable;
        _imGuiIo.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset | ImGuiBackendFlags.HasSetMousePos |
                                 ImGuiBackendFlags.HasMouseCursors;
        if (configureIo == null)
        {
            _imGuiIo.Fonts.AddFontFromFileTTF("Fonts/RobotoCondensed-Regular.ttf", 18);
        }
        else
        {
            configureIo(_imGuiIo);
        }

        var mvp = Matrix.OrthoOffCenterRH(
            0.0f,
            _imGuiIo.DisplaySize.X,
            _imGuiIo.DisplaySize.Y,
            0.0f,
            -1.0f,
            1.0f);
        _uniformBuffer = _graphicsContext.CreateUniformBuffer("ImGuiProjectionMatrix", mvp);

        var style = ImGui.GetStyle();
        SetStyleDarker(style);
        //SetStylePurple(style);
        style.Colors[(int)ImGuiCol.DockingEmptyBg] = Num.Vector4.Zero;

        CreateDeviceResources();
        SetKeyMappings();
        SetPerFrameImGuiData(1.0f / 60.0f);

        return true;
    }

    public void WindowResized(int width, int height)
    {
        _framebufferWidth = width;
        _framebufferHeight = height;

        var mvp = Matrix.OrthoOffCenterRH(
            0.0f,
            _framebufferWidth,
            _framebufferHeight,
            0.0f,
            -1.0f,
            1.0f);
        _uniformBuffer?.Update(mvp, 0);
    }

    private void DestroyDeviceObjects()
    {
        Dispose();
    }

    private void CreateDeviceResources()
    {
        _vertexBufferSize = 64 * 1024;
        _indexBufferSize = 64 * 1024;

        _vertexBuffer = _graphicsContext.CreateVertexBuffer<ImDrawVert>("ImGuiVertices", _vertexBufferSize);
        _indexBuffer = _graphicsContext.CreateIndexBuffer<ushort>("ImGuiIndices", _indexBufferSize);

        RecreateFontDeviceTexture();
    }

    public void BeginLayout()
    {
        _frameBegun = true;
        ImGui.NewFrame();
        ImGui.DockSpaceOverViewport(null, ImGuiDockNodeFlags.PassthruCentralNode);
    }

    public void EndLayout()
    {
        if (_frameBegun)
        {
            _frameBegun = false;
            ImGui.Render();
            RenderDrawData(ImGui.GetDrawData());
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
        _imGuiIo.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out var width, out var height, out var bytesPerPixel);

        var createTextureDescriptor = new TextureCreateDescriptor
        {
            Size = new Int3(width, height, 1),
            Format = Format.R8G8B8A8UNorm,
            ImageType = ImageType.Texture2D,
            Label = "ImGuiFontAtlas",
            ArrayLayers = 0,
            MipLevels = 1,
            SampleCount = SampleCount.OneSample
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

        var samplerDescriptor = new SamplerDescriptor
        {
            Anisotropy = SampleCount.SixteenSamples,
            CompareOperation = CompareOperation.Always,
            IsCompareEnabled = false,
            LodBias = 0.0f,
            MinLod = -1000.0f,
            MaxLod = 1000.0f,
            MagFilter = Filter.Linear,
            MinFilter = Filter.Linear,
            AddressModeU = AddressMode.ClampToBorder,
            AddressModeV = AddressMode.ClampToBorder
        };
        _fontSampler = Sampler.Create(samplerDescriptor);

        _imGuiIo.Fonts.SetTexID((IntPtr)_fontTexture.Id);
        _imGuiIo.Fonts.ClearTexData();
    }

    private void SetPerFrameImGuiData(float deltaSeconds)
    {
        _imGuiIo.DisplaySize = new System.Numerics.Vector2(
            _framebufferWidth / _scaleFactor.X,
            _framebufferHeight / _scaleFactor.Y);
        _imGuiIo.DisplayFramebufferScale = new System.Numerics.Vector2(_scaleFactor.X, _scaleFactor.Y);
        _imGuiIo.DeltaTime = deltaSeconds;
    }

    private void UpdateImGuiInput()
    {
        var currentMouseState = _inputProvider.MouseState;
        var currentKeyboardState = _inputProvider.KeyboardState;

        _imGuiIo.MouseDown[0] = currentMouseState.IsButtonDown(Glfw.MouseButton.ButtonLeft);
        _imGuiIo.MouseDown[1] = currentMouseState.IsButtonDown(Glfw.MouseButton.ButtonRight);
        _imGuiIo.MouseDown[2] = currentMouseState.IsButtonDown(Glfw.MouseButton.ButtonMiddle);
        _imGuiIo.MousePos = new System.Numerics.Vector2(currentMouseState.X, currentMouseState.Y);
        var scrollDelta = 0;//currentMouseState.Scroll.Y - _scrollWheelValue;
        _imGuiIo.MouseWheel = scrollDelta > 0
            ? 1
            : scrollDelta < 0
                ? -1
                : 0;
        _scrollWheelValue = 0;//(int)currentMouseState.Scroll.Y;

        foreach (Glfw.Key key in Enum.GetValues(typeof(Glfw.Key)))
        {
            if (key == Glfw.Key.Unknown)
            {
                continue;
            }

            _imGuiIo.KeysDown[(int)key] = currentKeyboardState.IsKeyDown(key);
        }

        foreach (var c in _pressedChars)
        {
            _imGuiIo.AddInputCharacter(c);
        }

        _pressedChars.Clear();

        _imGuiIo.KeyCtrl = currentKeyboardState[Glfw.Key.KeyLeftCtrl] || currentKeyboardState[Glfw.Key.KeyRightCtrl];
        _imGuiIo.KeyAlt = currentKeyboardState[Glfw.Key.KeyLeftAlt] || currentKeyboardState[Glfw.Key.KeyRightAlt];
        _imGuiIo.KeyShift = currentKeyboardState[Glfw.Key.KeyLeftShift] || currentKeyboardState[Glfw.Key.KeyRightShift];
        //_imGuiIo.KeySuper = keyboardState[Glfw.Key.KeyLeftCtrl] || keyboardState[Glfw.Key.KeyRightCtrl];
    }

    internal void PressChar(char keyChar)
    {
        _pressedChars.Add(keyChar);
    }

    internal void MouseScroll(Vector2 offset)
    {
        _imGuiIo.MouseWheel = offset.Y;
        _imGuiIo.MouseWheelH = offset.X;
    }

    private void SetKeyMappings()
    {
        _imGuiIo.KeyMap[(int)ImGuiKey.Tab] = (int)Glfw.Key.KeyTab;
        _imGuiIo.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Glfw.Key.KeyArrowLeft;
        _imGuiIo.KeyMap[(int)ImGuiKey.RightArrow] = (int)Glfw.Key.KeyArrowRight;
        _imGuiIo.KeyMap[(int)ImGuiKey.UpArrow] = (int)Glfw.Key.KeyArrowUp;
        _imGuiIo.KeyMap[(int)ImGuiKey.DownArrow] = (int)Glfw.Key.KeyArrowDown;
        _imGuiIo.KeyMap[(int)ImGuiKey.PageUp] = (int)Glfw.Key.KeyPageUp;
        _imGuiIo.KeyMap[(int)ImGuiKey.PageDown] = (int)Glfw.Key.KeyPageDown;
        _imGuiIo.KeyMap[(int)ImGuiKey.Home] = (int)Glfw.Key.KeyHome;
        _imGuiIo.KeyMap[(int)ImGuiKey.End] = (int)Glfw.Key.KeyEnd;
        _imGuiIo.KeyMap[(int)ImGuiKey.Delete] = (int)Glfw.Key.KeyDelete;
        _imGuiIo.KeyMap[(int)ImGuiKey.Backspace] = (int)Glfw.Key.KeyBackspace;
        _imGuiIo.KeyMap[(int)ImGuiKey.Enter] = (int)Glfw.Key.KeyEnter;
        _imGuiIo.KeyMap[(int)ImGuiKey.Escape] = (int)Glfw.Key.KeyEscape;
        _imGuiIo.KeyMap[(int)ImGuiKey.A] = (int)Glfw.Key.KeyA;
        _imGuiIo.KeyMap[(int)ImGuiKey.C] = (int)Glfw.Key.KeyC;
        _imGuiIo.KeyMap[(int)ImGuiKey.V] = (int)Glfw.Key.KeyV;
        _imGuiIo.KeyMap[(int)ImGuiKey.X] = (int)Glfw.Key.KeyX;
        _imGuiIo.KeyMap[(int)ImGuiKey.Y] = (int)Glfw.Key.KeyY;
        _imGuiIo.KeyMap[(int)ImGuiKey.Z] = (int)Glfw.Key.KeyZ;
    }

    private void RenderDrawData(ImDrawDataPtr drawDataPtr)
    {
        if (drawDataPtr.CmdListsCount == 0)
        {
            return;
        }

        for (var i = 0; i < drawDataPtr.CmdListsCount; i++)
        {
            var commandList = drawDataPtr.CmdListsRange[i];
            var vertexSize = commandList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
            if (vertexSize > _vertexBufferSize)
            {
                var newSize = (uint)Math.Max(_vertexBufferSize * 1.5f, vertexSize);
                _vertexBuffer!.Resize(newSize);
                _vertexBufferSize = newSize;
            }

            var indexSize = commandList.IdxBuffer.Size * sizeof(ushort);
            if (indexSize > _indexBufferSize)
            {
                var newSize = (uint)Math.Max(_indexBufferSize * 1.5f, indexSize);
                _indexBuffer!.Resize(newSize);
                _indexBufferSize = newSize;
            }
        }

        _graphicsContext.BindGraphicsPipeline(_imGuiGraphicsPipeline!);

        _imGuiGraphicsPipeline!.BindUniformBuffer(_uniformBuffer!, 0);
        _imGuiGraphicsPipeline.BindVertexBuffer(_vertexBuffer!, 0, 0);
        _imGuiGraphicsPipeline.BindIndexBuffer(_indexBuffer!);
        _imGuiGraphicsPipeline.BindSampledTexture(_fontSampler!, _fontTexture!, 0);

        drawDataPtr.ScaleClipRects(_imGuiIo.DisplayFramebufferScale);

        //GL.Enable(GL.EnableType.Blend);
        GL.Enable(GL.EnableType.ScissorTest);
        //GL.BlendEquation(GL.BlendEquationMode.FuncAdd);
        //GL.BlendFunc(GL.BlendFactor.SrcAlpha, GL.BlendFactor.OneMinusSrcAlpha);
        GL.Disable(GL.EnableType.CullFace);
        GL.Disable(GL.EnableType.DepthTest);

        for (var n = 0; n < drawDataPtr.CmdListsCount; n++)
        {
            var commandList = drawDataPtr.CmdListsRange[n];

            _vertexBuffer!.Update(
                commandList.VtxBuffer.Data,
                (uint)(commandList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>()), 0);
            _indexBuffer!.Update(
                commandList.IdxBuffer.Data,
                (uint)commandList.IdxBuffer.Size * sizeof(ushort), 0);

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
                        (int)drawCmdPtr.IdxOffset * sizeof(ushort));
                }

                indexOffset += (int)drawCmdPtr.ElemCount;
            }

            vertexOffset += commandList.VtxBuffer.Size;
        }

        GL.Disable(GL.EnableType.Blend);
        GL.Disable(GL.EnableType.ScissorTest);
        //GL.PopDebugGroup();
    }

    public void Dispose()
    {
        _fontTexture?.Dispose();
    }

    private void SetStylePurple(ImGuiStylePtr style)
    {
        style.Colors[(int)ImGuiCol.Text] = new Num.Vector4(1.00f, 1.00f, 1.00f, 1.00f);
        style.Colors[(int)ImGuiCol.TextDisabled] = new Num.Vector4(0.50f, 0.50f, 0.50f, 1.00f);
        style.Colors[(int)ImGuiCol.WindowBg] = new Num.Vector4(0.08f, 0.08f, 0.09f, 1.00f);
        style.Colors[(int)ImGuiCol.ChildBg] = new Num.Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        style.Colors[(int)ImGuiCol.PopupBg] = new Num.Vector4(0.15f, 0.15f, 0.17f, 0.94f);
        style.Colors[(int)ImGuiCol.Border] = new Num.Vector4(0.37f, 0.31f, 0.57f, 1.00f);
        style.Colors[(int)ImGuiCol.BorderShadow] = new Num.Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        style.Colors[(int)ImGuiCol.FrameBg] = new Num.Vector4(0.24f, 0.22f, 0.33f, 1.00f);
        style.Colors[(int)ImGuiCol.FrameBgHovered] = new Num.Vector4(0.41f, 0.39f, 0.50f, 0.40f);
        style.Colors[(int)ImGuiCol.FrameBgActive] = new Num.Vector4(0.41f, 0.40f, 0.50f, 0.62f);
        style.Colors[(int)ImGuiCol.TitleBg] = new Num.Vector4(0.12f, 0.11f, 0.14f, 1.00f);
        style.Colors[(int)ImGuiCol.TitleBgActive] = new Num.Vector4(0.12f, 0.11f, 0.14f, 1.00f);
        style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new Num.Vector4(0.00f, 0.00f, 0.00f, 0.51f);
        style.Colors[(int)ImGuiCol.MenuBarBg] = new Num.Vector4(0.24f, 0.22f, 0.33f, 1.00f);
        style.Colors[(int)ImGuiCol.ScrollbarBg] = new Num.Vector4(0.02f, 0.02f, 0.02f, 0.53f);
        style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Num.Vector4(0.31f, 0.31f, 0.31f, 1.00f);
        style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Num.Vector4(0.41f, 0.41f, 0.41f, 1.00f);
        style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Num.Vector4(0.51f, 0.51f, 0.51f, 1.00f);
        style.Colors[(int)ImGuiCol.CheckMark] = new Num.Vector4(0.60f, 0.56f, 0.77f, 1.00f);
        style.Colors[(int)ImGuiCol.SliderGrab] = new Num.Vector4(0.56f, 0.54f, 0.66f, 0.40f);
        style.Colors[(int)ImGuiCol.SliderGrabActive] = new Num.Vector4(0.76f, 0.73f, 0.88f, 0.40f);
        style.Colors[(int)ImGuiCol.Button] = new Num.Vector4(0.24f, 0.22f, 0.33f, 1.00f);
        style.Colors[(int)ImGuiCol.ButtonHovered] = new Num.Vector4(0.32f, 0.29f, 0.44f, 1.00f);
        style.Colors[(int)ImGuiCol.ButtonActive] = new Num.Vector4(0.21f, 0.20f, 0.26f, 0.40f);
        style.Colors[(int)ImGuiCol.Header] = new Num.Vector4(0.31f, 0.29f, 0.37f, 0.40f);
        style.Colors[(int)ImGuiCol.HeaderHovered] = new Num.Vector4(0.47f, 0.45f, 0.57f, 0.40f);
        style.Colors[(int)ImGuiCol.HeaderActive] = new Num.Vector4(0.21f, 0.20f, 0.25f, 0.40f);
        style.Colors[(int)ImGuiCol.Separator] = new Num.Vector4(0.37f, 0.31f, 0.57f, 1.00f);
        style.Colors[(int)ImGuiCol.SeparatorHovered] = new Num.Vector4(0.10f, 0.40f, 0.75f, 0.78f);
        style.Colors[(int)ImGuiCol.SeparatorActive] = new Num.Vector4(0.10f, 0.40f, 0.75f, 1.00f);
        style.Colors[(int)ImGuiCol.ResizeGrip] = new Num.Vector4(0.47f, 0.45f, 0.57f, 0.74f);
        style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Num.Vector4(0.59f, 0.57f, 0.71f, 0.74f);
        style.Colors[(int)ImGuiCol.ResizeGripActive] = new Num.Vector4(0.35f, 0.33f, 0.41f, 0.74f);
        style.Colors[(int)ImGuiCol.Tab] = new Num.Vector4(0.24f, 0.22f, 0.33f, 1.00f);
        style.Colors[(int)ImGuiCol.TabHovered] = new Num.Vector4(0.38f, 0.34f, 0.53f, 1.00f);
        style.Colors[(int)ImGuiCol.TabActive] = new Num.Vector4(0.24f, 0.22f, 0.33f, 1.00f);
        style.Colors[(int)ImGuiCol.TabUnfocused] = new Num.Vector4(0.27f, 0.26f, 0.32f, 0.40f);
        style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new Num.Vector4(0.42f, 0.39f, 0.57f, 0.40f);
        //style.Colors[(int)ImGuiCol.TabUnfocusedBorder]     = new Num.Vector4(0.11f, 0.09f, 0.17f, 1.00f);
        style.Colors[(int)ImGuiCol.DockingPreview] = new Num.Vector4(0.58f, 0.54f, 0.80f, 0.78f);
        style.Colors[(int)ImGuiCol.DockingEmptyBg] = new Num.Vector4(0.12f, 0.11f, 0.14f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotLines] = new Num.Vector4(0.61f, 0.61f, 0.61f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotLinesHovered] = new Num.Vector4(1.00f, 0.43f, 0.35f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotHistogram] = new Num.Vector4(0.90f, 0.70f, 0.00f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new Num.Vector4(1.00f, 0.60f, 0.00f, 1.00f);
        style.Colors[(int)ImGuiCol.TextSelectedBg] = new Num.Vector4(0.26f, 0.59f, 0.98f, 0.35f);
        style.Colors[(int)ImGuiCol.DragDropTarget] = new Num.Vector4(1.00f, 1.00f, 0.00f, 0.90f);
        style.Colors[(int)ImGuiCol.NavHighlight] = new Num.Vector4(0.26f, 0.59f, 0.98f, 1.00f);
        style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new Num.Vector4(1.00f, 1.00f, 1.00f, 0.70f);
        style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new Num.Vector4(0.80f, 0.80f, 0.80f, 0.20f);
        style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new Num.Vector4(0.80f, 0.80f, 0.80f, 0.35f);
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

    private void SetStyleDarker(ImGuiStylePtr style)
    {
        style.WindowPadding = new Num.Vector2(12, 12);
        style.WindowRounding = 5.0f;
        style.FramePadding = new Num.Vector2(4, 4);
        style.FrameRounding = 4.0f;
        style.ItemSpacing = new Num.Vector2(8, 8);
        style.ItemInnerSpacing = new Num.Vector2(4, 4);
        style.IndentSpacing = 16.0f;
        style.ScrollbarSize = 16.0f;
        style.ScrollbarRounding = 8.0f;
        style.GrabMinSize = 4.0f;
        style.GrabRounding = 3.0f;

        style.Colors[(int)ImGuiCol.Text] = new Num.Vector4(0.80f, 0.80f, 0.83f, 1.00f);
        style.Colors[(int)ImGuiCol.TextDisabled] = new Num.Vector4(0.24f, 0.23f, 0.29f, 1.00f);
        style.Colors[(int)ImGuiCol.WindowBg] = new Num.Vector4(0.06f, 0.05f, 0.07f, 1.00f);
        style.Colors[(int)ImGuiCol.ChildBg] = new Num.Vector4(0.07f, 0.07f, 0.09f, 1.00f);
        style.Colors[(int)ImGuiCol.PopupBg] = new Num.Vector4(0.07f, 0.07f, 0.09f, 1.00f);
        style.Colors[(int)ImGuiCol.Border] = new Num.Vector4(0.20f, 0.20f, 0.23f, 0.88f);
        style.Colors[(int)ImGuiCol.BorderShadow] = new Num.Vector4(0.92f, 0.91f, 0.88f, 0.00f);
        style.Colors[(int)ImGuiCol.FrameBg] = new Num.Vector4(0.10f, 0.09f, 0.12f, 1.00f);
        style.Colors[(int)ImGuiCol.FrameBgHovered] = new Num.Vector4(0.24f, 0.23f, 0.29f, 1.00f);
        style.Colors[(int)ImGuiCol.FrameBgActive] = new Num.Vector4(0.56f, 0.56f, 0.58f, 1.00f);
        style.Colors[(int)ImGuiCol.TitleBg] = new Num.Vector4(0.10f, 0.09f, 0.12f, 1.00f);
        style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new Num.Vector4(1.00f, 0.98f, 0.95f, 0.75f);
        style.Colors[(int)ImGuiCol.TitleBgActive] = new Num.Vector4(0.07f, 0.07f, 0.09f, 1.00f);
        style.Colors[(int)ImGuiCol.MenuBarBg] = new Num.Vector4(0.10f, 0.09f, 0.12f, 1.00f);
        style.Colors[(int)ImGuiCol.ScrollbarBg] = new Num.Vector4(0.10f, 0.09f, 0.12f, 1.00f);
        style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Num.Vector4(0.80f, 0.80f, 0.83f, 0.31f);
        style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Num.Vector4(0.56f, 0.56f, 0.58f, 1.00f);
        style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Num.Vector4(0.06f, 0.05f, 0.07f, 1.00f);
        style.Colors[(int)ImGuiCol.CheckMark] = new Num.Vector4(0.80f, 0.80f, 0.83f, 0.31f);
        style.Colors[(int)ImGuiCol.SliderGrab] = new Num.Vector4(0.80f, 0.80f, 0.83f, 0.31f);
        style.Colors[(int)ImGuiCol.SliderGrabActive] = new Num.Vector4(0.06f, 0.05f, 0.07f, 1.00f);
        style.Colors[(int)ImGuiCol.Button] = new Num.Vector4(0.10f, 0.09f, 0.12f, 1.00f);
        style.Colors[(int)ImGuiCol.ButtonHovered] = new Num.Vector4(0.24f, 0.23f, 0.29f, 1.00f);
        style.Colors[(int)ImGuiCol.ButtonActive] = new Num.Vector4(0.56f, 0.56f, 0.58f, 1.00f);
        style.Colors[(int)ImGuiCol.Header] = new Num.Vector4(0.10f, 0.09f, 0.12f, 1.00f);
        style.Colors[(int)ImGuiCol.HeaderHovered] = new Num.Vector4(0.56f, 0.56f, 0.58f, 1.00f);
        style.Colors[(int)ImGuiCol.HeaderActive] = new Num.Vector4(0.06f, 0.05f, 0.07f, 1.00f);
        style.Colors[(int)ImGuiCol.ResizeGrip] = new Num.Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Num.Vector4(0.56f, 0.56f, 0.58f, 1.00f);
        style.Colors[(int)ImGuiCol.ResizeGripActive] = new Num.Vector4(0.06f, 0.05f, 0.07f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotLines] = new Num.Vector4(0.40f, 0.39f, 0.38f, 0.63f);
        style.Colors[(int)ImGuiCol.PlotLinesHovered] = new Num.Vector4(0.25f, 1.00f, 0.00f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotHistogram] = new Num.Vector4(0.40f, 0.39f, 0.38f, 0.63f);
        style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new Num.Vector4(0.25f, 1.00f, 0.00f, 1.00f);
        style.Colors[(int)ImGuiCol.TextSelectedBg] = new Num.Vector4(0.25f, 1.00f, 0.00f, 0.43f);
    }
}