using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using EngineKit;
using EngineKit.Graphics;
using EngineKit.Native.OpenGL;
using ImGuiNET;
using OpenTK.Mathematics;
using Serilog;
using SpaceGame.Game;
using SpaceGame.Game.Ecs;
using SpaceGame.Game.Messages;
using MathHelper = EngineKit.MathHelper;

namespace SpaceGame;

internal sealed class DeferredRenderer : IRenderer
{
    private float _shadowSettingBias1 = 0.02f;
    private float _shadowSettingBias2 = 0.0015f;
    private float _shadowSettingRMax = 0.005f;
    private int _shadowSettingSamples = 4;

    private readonly ILogger _logger;
    private readonly IMessageBus _messageBus;
    private readonly IGraphicsContext _graphicsContext;
    private readonly IApplicationContext _applicationContext;
    private readonly IMeshLoader _meshLoader;
    private readonly IImageLibrary _imageLibrary;
    private readonly IMaterialLibrary _materialLibrary;
    private readonly ITextureLibrary _textureLibrary;
    private readonly IRendererContext _rendererContext;

    private ITexture? _deferredGBufferBaseColorTexture;
    private ITexture? _deferredGBufferNormalTexture;
    private ITexture? _deferredGBufferMaterialIdTexture;
    private ITexture? _deferredGBufferDepthTexture;
    private ITexture? _deferredLBufferLightTexture;
    private ITexture? _deferredFinalFramebufferTexture;

    private ITexture? _skyboxTexture;
    private ITexture? _skyboxConvolvedTexture;

    private ITexture? _directionalLightShadowTexture;

    private ISampler? _gBufferSampler;
    private ISampler? _textureSampler;
    private ISampler? _shadowSampler;

    private ISampler? _blueNoiseSampler;
    private ITexture? _blueNoise16Texture;
    private ITexture? _blueNoise32Texture;

    private IGraphicsPipeline? _deferredPipeline;
    private IGraphicsPipeline? _globalLightsAndShadowPipeline;
    private IGraphicsPipeline? _localLightsAndShadowPipeline;
    private IGraphicsPipeline? _resolveDeferredPipeline;
    private IGraphicsPipeline? _debugGraphicsPipeline;
    private IGraphicsPipeline? _directionalLightShadowGraphicsPipeline;

    private IVertexBuffer? _lightSphereVertexBuffer;
    private IIndexBuffer? _lightSphereIndexBuffer;
    private IVertexBuffer? _lightConeVertexBuffer;
    private IIndexBuffer? _lightConeIndexBuffer;

    private FramebufferRenderDescriptor _deferredGeometryFramebufferRenderDescriptor;
    private FramebufferRenderDescriptor _deferredLightFramebufferRenderDescriptor;
    private FramebufferRenderDescriptor? _directionalLightShadowFramebufferRenderDescriptor;
    private FramebufferRenderDescriptor _deferredFinalFramebufferRenderDescriptor;

    private GpuEnvironment _gpuEnvironment;
    private IUniformBuffer? _gpuEnvironmentBuffer;
    private GpuBaseInformation _gpuBaseInformation;
    private IUniformBuffer? _gpuBaseInformationBuffer;

    private GpuDirectionalShadowSettings _gpuDirectionalShadowSettings;
    private IUniformBuffer? _gpuDirectionalShadowSettingsBuffer;

    private IVertexBuffer? _vertexBuffer;
    private IIndexBuffer? _indexBuffer;
    private IShaderStorageBuffer? _gpuInstanceDataBuffer;
    private IIndirectBuffer? _gpuIndirectDrawBuffer;

    private IUniformBuffer? _lightInformationBuffer;

    private readonly IDictionary<string, int> _knownMeshDatesRefCounter;
    private readonly IDictionary<string, MeshData> _knownMeshDates; // all meshdates to be used to create vb/ib/material/textures
    private readonly IDictionary<string, MeshData> _meshDatesToBeAdded; // entity was added, now add
    private readonly IList<string> _meshDatesToBeRemoved; // entity was removed, now remove from known meshdates, to reconstruct vb/ib/material/textures

    private readonly IDictionary<string, int> _knownMaterialsRefCounter;
    private readonly IDictionary<string, Material> _knownMaterials;
    private readonly IDictionary<string, Material> _materialsToBeAdded;
    private readonly IList<string> _materialsToBeRemoved;

    private readonly IDictionary<string, MeshDataDrawElement> _knownMeshDataDrawElements;

    private IShaderStorageBuffer? _gpuMaterialBuffer;
    private IDictionary<string, int>? _materialNameIndexMap;
    private IReadOnlyCollection<ITexture>? _textureArrays;
    private readonly IList<GpuInstanceData> _instances;
    private readonly IList<GpuIndirectElementData> _indirectElements;

    private int _lightMapOrthoWidth = 400;
    private int _lightMapOrthoHeight = 400;
    private int _lightMapOrthoNear = 0;
    private int _lightMapOrthoFar = 512;

    private float _cameraNear = 0.1f;
    private float _cameraFar = 2048f;

    public DeferredRenderer(
        ILogger logger,
        IMessageBus messageBus,
        IGraphicsContext graphicsContext,
        IApplicationContext applicationContext,
        IMeshLoader meshLoader,
        IImageLibrary imageLibrary,
        IMaterialLibrary materialLibrary,
        ITextureLibrary textureLibrary,
        IRendererContext rendererContext)
    {
        _logger = logger.ForContext<DeferredRenderer>();
        _messageBus = messageBus;
        _messageBus.Subscribe<AddModelToScene>(HandleAddModelToScene);
        _messageBus.Subscribe<AddModelMeshToScene>(HandleAddModelMeshToScene);
        _messageBus.Subscribe<RemoveModelFromScene>(HandleRemoveModelFromScene);

        _graphicsContext = graphicsContext;
        _applicationContext = applicationContext;
        _meshLoader = meshLoader;
        _imageLibrary = imageLibrary;
        _materialLibrary = materialLibrary;
        _textureLibrary = textureLibrary;
        _rendererContext = rendererContext;

        _instances = new List<GpuInstanceData>(2048);
        _indirectElements = new List<GpuIndirectElementData>(2048);

        _knownMeshDates = new Dictionary<string, MeshData>(256);
        _knownMeshDatesRefCounter = new Dictionary<string, int>(256);
        _meshDatesToBeAdded = new Dictionary<string, MeshData>(256);
        _meshDatesToBeRemoved = new List<string>(256);

        _knownMaterials = new Dictionary<string, Material>(256);
        _knownMaterialsRefCounter = new Dictionary<string, int>(256);
        _materialsToBeAdded = new Dictionary<string, Material>(256);
        _materialsToBeRemoved = new List<string>(256);

        _knownMeshDataDrawElements = new Dictionary<string, MeshDataDrawElement>();
    }

    public void Dispose()
    {
        DestroySizeDependentResources();

        _blueNoiseSampler?.Dispose();
        _blueNoise16Texture?.Dispose();
        _blueNoise32Texture?.Dispose();

        _textureSampler?.Dispose();
        _gBufferSampler?.Dispose();
        _shadowSampler?.Dispose();

        _skyboxTexture?.Dispose();
        _skyboxConvolvedTexture?.Dispose();

        _deferredPipeline?.Dispose();
        _localLightsAndShadowPipeline?.Dispose();
        _resolveDeferredPipeline?.Dispose();
        _debugGraphicsPipeline?.Dispose();

        _deferredGBufferBaseColorTexture?.Dispose();
        _deferredGBufferNormalTexture?.Dispose();
        _deferredGBufferMaterialIdTexture?.Dispose();
        _deferredGBufferDepthTexture?.Dispose();
        _deferredLBufferLightTexture?.Dispose();

        _directionalLightShadowTexture?.Dispose();
        _directionalLightShadowGraphicsPipeline?.Dispose();

        _lightConeIndexBuffer?.Dispose();
        _lightConeVertexBuffer?.Dispose();
        _lightSphereIndexBuffer?.Dispose();
        _lightSphereVertexBuffer?.Dispose();
    }

    public bool Load()
    {
        if (!LoadGraphicsPipelines())
        {
            return false;
        }

        if (!LoadSamplers())
        {
            return false;
        }

        if (!LoadSkybox("Miramar"))
        {
            return false;
        }

        if (!CreateConvolveSkybox())
        {
            return false;
        }

        CreateSizeDependentResources();
        CreateShadowMaps(1024, 1024);

        var lightSphereMeshDates = _meshLoader.LoadModel(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/Lights/SM_Icosphere.gltf")).ToArray();
        _lightSphereVertexBuffer = _graphicsContext.CreateVertexBuffer("PointLightVertices", lightSphereMeshDates, VertexType.Position);
        _lightSphereIndexBuffer = _graphicsContext.CreateIndexBuffer("PointLightIndices", lightSphereMeshDates);

        var lightConeMeshDates = _meshLoader.LoadModel(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/Lights/SM_Cone_Z.gltf")).ToArray();
        _lightConeVertexBuffer = _graphicsContext.CreateVertexBuffer("SpotLightVertices", lightConeMeshDates, VertexType.Position);
        _lightConeIndexBuffer = _graphicsContext.CreateIndexBuffer("SpotLightIndices", lightConeMeshDates);

        _gpuDirectionalShadowSettings = new GpuDirectionalShadowSettings
        {
            Bias1 = 0.02f,
            Bias2 = 0.0015f,
            rMax = 0.005f,
            AccumFactor = 1.0f,
            Samples = 4,
            RandomOffset = 10000
        };
        _gpuDirectionalShadowSettingsBuffer = _graphicsContext.CreateUniformBuffer<GpuDirectionalShadowSettings>("ShadowSettings");
        _gpuDirectionalShadowSettingsBuffer.AllocateStorage(_gpuDirectionalShadowSettings, StorageAllocationFlags.Dynamic);

        _gpuBaseInformationBuffer = _graphicsContext.CreateUniformBuffer<GpuBaseInformation>("BaseInformation");
        _gpuBaseInformationBuffer.AllocateStorage(_gpuBaseInformation, StorageAllocationFlags.Dynamic);

        _gpuEnvironmentBuffer = _graphicsContext.CreateUniformBuffer<GpuBaseInformation>("Environment");
        _gpuEnvironmentBuffer.AllocateStorage(_gpuEnvironment, StorageAllocationFlags.Dynamic);

        _gpuMaterialBuffer = _graphicsContext.CreateShaderStorageBuffer<GpuMaterial>("SceneMaterials");
        _gpuMaterialBuffer.AllocateStorage(Marshal.SizeOf<GpuMaterial>() * 256, StorageAllocationFlags.Dynamic);

        _gpuInstanceDataBuffer = _graphicsContext.CreateShaderStorageBuffer<GpuInstanceData>("SceneInstances");
        _gpuInstanceDataBuffer.AllocateStorage(Marshal.SizeOf<GpuInstanceData>() * 1024, StorageAllocationFlags.Dynamic);

        _gpuIndirectDrawBuffer = _graphicsContext.CreateIndirectBuffer("SceneDrawElements");
        _gpuIndirectDrawBuffer.AllocateStorage(Marshal.SizeOf<GpuIndirectElementData>() * 1024, StorageAllocationFlags.Dynamic);

        return true;
    }

    public void Resize()
    {
        DestroySizeDependentResources();
        CreateSizeDependentResources();
    }

    public void PrepareScene(EntityWorld entityWorld)
    {
        var requireStructuralBufferRenewal = IsStructuralBufferRenewalRequired();
        if (requireStructuralBufferRenewal)
        {
            RebuildStructuralBuffers();
        }

        _instances.Clear();
        _indirectElements.Clear();

        var entitiesToBeRendered = entityWorld.GetEntitiesWithComponents<ModelMeshComponent, TransformComponent>();
        var entitiesSpan = CollectionsMarshal.AsSpan(entitiesToBeRendered);
        ref var entityRef = ref MemoryMarshal.GetReference(entitiesSpan);
        for (var i = 0; i < entitiesSpan.Length; i++)
        {
            var entity = Unsafe.Add(ref entityRef, i);
            var modelMeshComponent = entityWorld.GetComponent<ModelMeshComponent>(entity.Id);
            var transformComponent = entityWorld.GetComponent<TransformComponent>(entity.Id);

            _instances.Add(new GpuInstanceData
            {
                MaterialIndex = _materialNameIndexMap == null
                    ? new Vector4i(-1, -1, -1, -1)
                    : new Vector4i(_materialNameIndexMap.TryGetValue(modelMeshComponent.MeshData.MaterialName, out var index)
                        ? index
                        : -1, -1, -1, -1),
                /*
                ModelToWorldMatrix = Matrix4.CreateTranslation(transformComponent.GlobalPosition) *
                                     Matrix4.CreateFromQuaternion(transformComponent.GlobalRotation) *
                                     Matrix4.CreateScale(transformComponent.GlobalScale)
                                     */
                ModelToWorldMatrix = transformComponent.GlobalWorldMatrix
            });

            _indirectElements.Add(new GpuIndirectElementData
            {
                IndexCount = (uint)modelMeshComponent.MeshData.IndexCount,
                InstanceCount = 1,
                FirstIndex = (uint)modelMeshComponent.MeshData.IndexOffset,
                BaseVertex = modelMeshComponent.MeshData.VertexOffset,
                BaseInstance = 0
            });
        }

        if (_instances.Count > 0)
        {
            _gpuInstanceDataBuffer.Update(_instances.ToArray(), 0);
        }

        if (_indirectElements.Count > 0)
        {
            _gpuIndirectDrawBuffer.Update(_indirectElements.ToArray(), 0);
        }
    }

    public bool RenderScene(ICamera camera, Vector3 directionalLightPosition)
    {
        _gpuBaseInformation.WorldToViewMatrix = camera.ViewMatrix;
        _gpuBaseInformation.ViewToClipMatrix = camera.ProjectionMatrix;

        _gpuBaseInformation.ClipToViewMatrix = Matrix4.Invert(camera.ProjectionMatrix);
        _gpuBaseInformation.ViewToWorldMatrix = Matrix4.Invert(camera.ViewMatrix);
        _gpuBaseInformation.CameraPosition = new Vector4(camera.Position, MathHelper.ToRadians(camera.FieldOfView));
        _gpuBaseInformation.CameraDirection = new Vector4(camera.Direction, camera.AspectRatio);
        _gpuBaseInformationBuffer!.Update(_gpuBaseInformation, 0);

        _gpuDirectionalShadowSettingsBuffer!.Update(_gpuDirectionalShadowSettings, 0);

        camera.NearPlane = _cameraNear;
        camera.FarPlane = _cameraFar;

        //if (_gpuIndirectDrawBuffer.Count > 0)
        {
            GL.PushDebugGroup("Deferred");
            {
                GL.PushDebugGroup("GBuffer");
                {
                    _graphicsContext.BindGraphicsPipeline(_deferredPipeline!);
                    _graphicsContext.BeginRenderToFramebuffer(_deferredGeometryFramebufferRenderDescriptor);

                    _deferredPipeline!.BindVertexBuffer(_vertexBuffer!, 0, 0);
                    _deferredPipeline!.BindIndexBuffer(_indexBuffer!);
                    _deferredPipeline!.BindUniformBuffer(_gpuBaseInformationBuffer!, 1);
                    _deferredPipeline!.BindShaderStorageBuffer(_gpuInstanceDataBuffer!, 2);

                    if (_gpuMaterialBuffer != null)
                    {
                        _deferredPipeline.BindShaderStorageBuffer(_gpuMaterialBuffer!, 4);
                        var bindingIndex = 0u;
                        if (_textureArrays != null)
                        {
                            foreach (var textureArray in _textureArrays!)
                            {
                                _deferredPipeline.BindSampledTexture(_textureSampler!, textureArray, bindingIndex++);
                            }
                        }
                    }

                    _deferredPipeline.MultiDrawElementsIndirect(_gpuIndirectDrawBuffer!, _indirectElements.Count);

                    _graphicsContext.EndRender();
                    GL.PopDebugGroup();
                }

                GL.PushDebugGroup("Render Shadowmap Directional Light");
                {
                    _gpuBaseInformation.WorldToViewMatrix = Matrix4.LookAt(directionalLightPosition, Vector3.Zero, Vector3.UnitY);
                    _gpuBaseInformation.ViewToClipMatrix = Matrix4.CreateOrthographicOffCenter(
                        -_lightMapOrthoWidth / 2,
                        _lightMapOrthoWidth / 2,
                        -_lightMapOrthoHeight / 2,
                    _lightMapOrthoHeight / 2,
                        -_lightMapOrthoNear,
                        _lightMapOrthoFar);
                    _gpuBaseInformation.ViewToWorldMatrix = Matrix4.Identity; // unused
                    _gpuBaseInformation.ClipToViewMatrix = Matrix4.Identity; // unused
                    _gpuBaseInformationBuffer!.Update(_gpuBaseInformation, 0);

                    _graphicsContext.BindGraphicsPipeline(_directionalLightShadowGraphicsPipeline!);
                    _graphicsContext.BeginRenderToFramebuffer(_directionalLightShadowFramebufferRenderDescriptor.Value);
                    _directionalLightShadowGraphicsPipeline!.BindVertexBuffer(_vertexBuffer!, 0, 0);
                    _directionalLightShadowGraphicsPipeline.BindIndexBuffer(_indexBuffer!);
                    _directionalLightShadowGraphicsPipeline.BindUniformBuffer(_gpuBaseInformationBuffer, 1);
                    _directionalLightShadowGraphicsPipeline.BindShaderStorageBuffer(_gpuInstanceDataBuffer!, 2);
                    _deferredPipeline.MultiDrawElementsIndirect(_gpuIndirectDrawBuffer!, _indirectElements.Count);
                    _graphicsContext.EndRender();

                    GL.PopDebugGroup();
                }
            }

            _gpuBaseInformation.ViewToClipMatrix = camera.ProjectionMatrix;
            _gpuBaseInformation.ClipToViewMatrix = Matrix4.Invert(camera.ProjectionMatrix);
            _gpuBaseInformation.WorldToViewMatrix = camera.ViewMatrix;
            _gpuBaseInformation.ViewToWorldMatrix = Matrix4.Invert(camera.ViewMatrix);
            _gpuBaseInformationBuffer!.Update(_gpuBaseInformation, 0);

            RenderLightsDeferred(directionalLightPosition);

            ResolveGAndLBufferDeferred();
        }

        return true;
    }

    public void RenderShadowDebugUi()
    {
        if (ImGui.CollapsingHeader("Camera Planes"))
        {
            ImGui.SliderFloat("Camera Near", ref _cameraNear, 0.1f, 16f);
            ImGui.SliderFloat("Camera Far", ref _cameraFar, 16f, 8192f);
        }
        if (ImGui.CollapsingHeader("Shadows"))
        {
            ImGui.DragInt("Ortho Width", ref _lightMapOrthoWidth, 8, 8, 2048);
            ImGui.DragInt("Ortho Height", ref _lightMapOrthoHeight, 8, 8, 2048);
            ImGui.DragInt("Ortho Near", ref _lightMapOrthoNear, 8, -1024, 8192);
            ImGui.DragInt("Ortho Far", ref _lightMapOrthoFar, 8, 0, 8192);

            ImGui.DragFloat("Linear Bias", ref _shadowSettingBias1, 0.0001f, 0.001f, 1.0f);
            ImGui.SliderFloat("Constant Bias", ref _shadowSettingBias2, 0.001f, 0.01f);
            ImGui.DragFloat("rMax", ref _shadowSettingRMax, 0.001f, 0.001f, 1.0f);

            ImGui.SliderInt("Samples", ref _shadowSettingSamples, 1, 32);

            _gpuDirectionalShadowSettings.Bias1 = _shadowSettingBias1;
            _gpuDirectionalShadowSettings.Bias2 = _shadowSettingBias2;
            _gpuDirectionalShadowSettings.rMax = _shadowSettingRMax;
            _gpuDirectionalShadowSettings.AccumFactor = 1.0f;
            _gpuDirectionalShadowSettings.Samples = _shadowSettingSamples;
            _gpuDirectionalShadowSettings.RandomOffset = 0;
        }
    }

    public void CreateShadowMaps(int width, int height)
    {
        _directionalLightShadowTexture?.Dispose();
        _directionalLightShadowTexture = _graphicsContext.CreateTexture2D(width, height, Format.D32Float, "DirectionalLightShadowMap");

        if (_directionalLightShadowFramebufferRenderDescriptor != null)
        {
            _graphicsContext.RemoveFramebuffer(_directionalLightShadowFramebufferRenderDescriptor.Value);
        }

        _directionalLightShadowFramebufferRenderDescriptor = new FramebufferRenderDescriptorBuilder()
            .WithDepthAttachment(_directionalLightShadowTexture, true, 1.0f)
            .WithViewport(width, height)
            .Build("DirectionalLightShadow");
    }


    private void RenderLightsDeferred(Vector3 directionalLightPosition)
    {
        GL.PushDebugGroup("LBuffer");
        {
            _graphicsContext.BeginRenderToFramebuffer(_deferredLightFramebufferRenderDescriptor);

            GL.PushDebugGroup("GlobalLights");
            {
                var directionalLightWorldToViewMatrix = Matrix4.LookAt(directionalLightPosition, Vector3.Zero, Vector3.UnitY);
                var directionalLightViewToClipMatrix = Matrix4.CreateOrthographicOffCenter(
                    -_lightMapOrthoWidth / 2,
                    _lightMapOrthoWidth / 2,
                    -_lightMapOrthoHeight / 2,
                    _lightMapOrthoHeight / 2,
                    _lightMapOrthoNear,
                    _lightMapOrthoFar);
                _gpuEnvironment.DirectionalLightProjectionMatrix = directionalLightViewToClipMatrix;
                _gpuEnvironment.DirectionalLightViewMatrix = directionalLightWorldToViewMatrix;
                _gpuEnvironment.DirectionalLightDirection = new Vector4(Vector3.Subtract(directionalLightPosition, Vector3.Zero), 1.0f);
                _gpuEnvironment.DirectionalLightColor = new Vector4(0.1f, 1.0f, 0.1f, 1.0f);
                _gpuEnvironmentBuffer!.Update(_gpuEnvironment, 0);

                _graphicsContext.BindGraphicsPipeline(_globalLightsAndShadowPipeline!);
                _globalLightsAndShadowPipeline!.BindSampledTexture(_gBufferSampler!, _deferredGBufferBaseColorTexture!, 0u);
                _globalLightsAndShadowPipeline.BindSampledTexture(_gBufferSampler!, _deferredGBufferNormalTexture!, 1u);
                _globalLightsAndShadowPipeline.BindSampledTexture(_gBufferSampler!, _deferredGBufferDepthTexture!, 2u);
                _globalLightsAndShadowPipeline.BindSampledTexture(_gBufferSampler!, _deferredGBufferMaterialIdTexture!, 3u);
                _globalLightsAndShadowPipeline.BindSampledTexture(_shadowSampler!, _directionalLightShadowTexture!, 4u);
                _globalLightsAndShadowPipeline.BindSampledTexture(_blueNoiseSampler!, _blueNoise16Texture!, 5u);
                _globalLightsAndShadowPipeline.BindSampledTexture(_blueNoiseSampler!, _blueNoise32Texture!, 6u);

                _globalLightsAndShadowPipeline.BindUniformBuffer(_gpuBaseInformationBuffer!, 1);
                _globalLightsAndShadowPipeline.BindUniformBuffer(_gpuEnvironmentBuffer, 3);
                _globalLightsAndShadowPipeline.BindUniformBuffer(_gpuDirectionalShadowSettingsBuffer!, 8);

                if (_gpuMaterialBuffer != null)
                {
                    _deferredPipeline!.BindShaderStorageBuffer(_gpuMaterialBuffer!, 4);
                    var bindingIndex = 8u;
                    if (_textureArrays != null)
                    {
                        foreach (var textureArray in _textureArrays)
                        {
                            _deferredPipeline.BindSampledTexture(_textureSampler!, textureArray, bindingIndex++);
                        }
                    }
                }

                _globalLightsAndShadowPipeline.DrawArrays(3, 0);
                _graphicsContext.EndRender();
                GL.PopDebugGroup();
            }

            /*
            GL.PushDebugGroup("LocalLights");
            {
                _graphicsContext.BindGraphicsPipeline(_localLightsAndShadowPipeline);
                _localLightsAndShadowPipeline.BindSampledTexture(_gBufferSampler, _deferredGBufferBaseColorTexture, 0u);
                _localLightsAndShadowPipeline.BindSampledTexture(_gBufferSampler, _deferredGBufferNormalTexture, 1u);
                _localLightsAndShadowPipeline.BindSampledTexture(_gBufferSampler, _deferredGBufferDepthTexture, 2u);
                _localLightsAndShadowPipeline.BindSampledTexture(_gBufferSampler, _deferredGBufferMaterialIdTexture, 3u);

                if (_sceneBuffers.MaterialBuffer != null)
                {
                    _deferredPipeline.BindShaderStorageBuffer(_sceneBuffers.MaterialBuffer!, 4);
                    var bindingIndex = 4u;
                    foreach (var textureArray in _sceneBuffers.TextureArrays)
                    {
                        _deferredPipeline.BindSampledTexture(_textureSampler, textureArray, bindingIndex++);
                    }
                }

                _localLightsAndShadowPipeline.BindShaderStorageBuffer(_gpuLightBuffer, 5u);

                var pointLightCount = _gpuLights.Count(l => l.PositionAndType.W == 0.0f);
                if (pointLightCount > 0)
                {
                    _localLightsAndShadowPipeline.BindVertexBuffer(_lightSphereVertexBuffer, 0, 0);
                    _localLightsAndShadowPipeline.BindIndexBuffer(_lightSphereIndexBuffer);
                    _localLightsAndShadowPipeline.DrawElementsInstanced(_lightSphereIndexBuffer.Count, 0,
                        pointLightCount);
                }

                var spotLightCount = _gpuLights.Count(l => l.PositionAndType.W == 1.0f);
                if (spotLightCount > 0)
                {
                    _localLightsAndShadowPipeline.BindVertexBuffer(_lightConeVertexBuffer, 0, 0);
                    _localLightsAndShadowPipeline.BindIndexBuffer(_lightConeIndexBuffer);
                    _localLightsAndShadowPipeline.DrawElementsInstanced(_lightConeIndexBuffer.Count, 0,
                        spotLightCount);
                }

                _graphicsContext.EndRender();
                GL.PopDebugGroup();
            }
            */

            GL.PopDebugGroup();
        }
    }

    private void ResolveGAndLBufferDeferred()
    {
        GL.PushDebugGroup("Resolve GBuffer+LBuffer");
        {
            if (_rendererContext.DrawMode == DrawMode.Default)
            {
                _graphicsContext.BindGraphicsPipeline(_resolveDeferredPipeline!);
                _graphicsContext.BeginRenderToFramebuffer(_deferredFinalFramebufferRenderDescriptor);
                _localLightsAndShadowPipeline!.BindSampledTexture(_gBufferSampler!, _deferredGBufferBaseColorTexture!, 0u);
                _localLightsAndShadowPipeline.BindSampledTexture(_gBufferSampler!, _deferredLBufferLightTexture!, 1u);
                _localLightsAndShadowPipeline.BindSampledTexture(_gBufferSampler!, _deferredGBufferDepthTexture!, 2u);
                _localLightsAndShadowPipeline.BindSampledTexture(_gBufferSampler!, _deferredGBufferNormalTexture!, 3u);
                _localLightsAndShadowPipeline.BindSampledTexture(_textureSampler!, _skyboxTexture!, 5u);
                _localLightsAndShadowPipeline.BindSampledTexture(_textureSampler!, _skyboxConvolvedTexture!, 6u);
                _localLightsAndShadowPipeline.DrawArrays(3, 0);
                _graphicsContext.EndRender();
            }
            else
            {
                _graphicsContext.BindGraphicsPipeline(_debugGraphicsPipeline!);
                _debugGraphicsPipeline!.BindUniformBuffer(_gpuBaseInformationBuffer!, 1u);
                _graphicsContext.BeginRenderToFramebuffer(_deferredFinalFramebufferRenderDescriptor);
                if (_rendererContext.DrawMode == DrawMode.Normal)
                {
                    _localLightsAndShadowPipeline!.BindSampledTexture(_textureSampler!, _deferredGBufferNormalTexture!, 0u);
                }
                else if (_rendererContext.DrawMode == DrawMode.Depth)
                {
                    _localLightsAndShadowPipeline!.BindSampledTexture(_gBufferSampler!, _deferredGBufferDepthTexture!, 0u);
                }
                else if (_rendererContext.DrawMode == DrawMode.LightBuffer)
                {
                    _localLightsAndShadowPipeline!.BindSampledTexture(_textureSampler!, _deferredLBufferLightTexture!, 0u);
                }
                else if (_rendererContext.DrawMode == DrawMode.BaseColor)
                {
                    _localLightsAndShadowPipeline!.BindSampledTexture(_textureSampler!, _deferredGBufferBaseColorTexture!, 0u);
                }
                else if (_rendererContext.DrawMode == DrawMode.DirectionalShadowmap)
                {
                    _localLightsAndShadowPipeline!.BindSampledTexture(_textureSampler!, _directionalLightShadowTexture!, 0u);
                }

                _localLightsAndShadowPipeline!.DrawArrays(3, 0);
                _graphicsContext.EndRender();
            }

            GL.PopDebugGroup();
        }

        GL.PopDebugGroup();
    }

    private bool LoadGraphicsPipelines()
    {
        var debugPipelineResult = _graphicsContext.CreateGraphicsPipelineBuilder()
            .WithTopology(PrimitiveTopology.Triangles)
            .WithVertexInput(new VertexInputDescriptorBuilder()
                .AddAttribute(0, DataType.Float, 3, 0)
                .AddAttribute(0, DataType.Float, 2, 12)
                .Build("FST"))
            .WithShadersFromFiles("Shaders/FST.vs.glsl", "Shaders/FST.fs.glsl")
            .Build("Debug");
        if (debugPipelineResult.IsFailure)
        {
            _logger.Error("Unable to create 'Debug' graphics pipeline {Details}", debugPipelineResult.Error);
            return false;
        }
        _debugGraphicsPipeline = debugPipelineResult.Value;

        var deferredPipelineResult = _graphicsContext.CreateGraphicsPipelineBuilder()
            .EnableDepthTest()
            .WithTopology(PrimitiveTopology.Triangles)
            /*
            .WithVertexInput(VertexType.PositionNormalUvTangent)
            .WithVertexInput<VertexPositionNormalUvTangent>()
            .WithVertexInput<VertexPositionNormalUvTangent>(vertexType =>
            {
                vertexType.Position.BindToBuffer(0, 0);
                vertexType.Normal.BindToBuffer(1, 0);
                vertexType.Uv.BindToBuffer(2, 0);
            })
            */
            .WithVertexInput(new VertexInputDescriptorBuilder()
                .AddAttribute(0, DataType.Float, 3, 0)
                .AddAttribute(0, DataType.Float, 3, 12)
                .AddAttribute(0, DataType.Float, 2, 24)
                .AddAttribute(0, DataType.Float, 4, 32)
                .Build(nameof(VertexPositionNormalUvTangent)))
            .WithFaceWinding(FaceWinding.Clockwise)
            .WithShadersFromFiles("Shaders/Forward.vs.glsl", "Shaders/GBuffer.fs.glsl")
            .Build("GBuffer");
        if (deferredPipelineResult.IsFailure)
        {
            _logger.Error("Unable to create 'Deferred_PositionNormalUvTangent' graphics pipeline {Details}", deferredPipelineResult.Error);
            return false;
        }

        _deferredPipeline = deferredPipelineResult.Value;

        var globalLightAndShadowPipeline = _graphicsContext.CreateGraphicsPipelineBuilder()
            .WithVertexInput(new VertexInputDescriptorBuilder()
                .AddAttribute(0, DataType.Float, 3, 0)
                .AddAttribute(0, DataType.Float, 2, 12)
                .Build("FST"))
            .WithShadersFromFiles("Shaders/FST.vs.glsl", "Shaders/LightsGlobal.fs.glsl")
            //.EnableCulling(CullMode.Front)
            .DisableDepthTest()
            .EnableBlending(ColorBlendAttachmentDescriptor.PreMultiplied)
            .Build("GlobalLightAndShadowPipeline");
        if (globalLightAndShadowPipeline.IsFailure)
        {
            _logger.Error("Unable to create 'graphics pipeline {Details}", globalLightAndShadowPipeline.Error);
            return false;
        }

        _globalLightsAndShadowPipeline = globalLightAndShadowPipeline.Value;

        var localLightAndShadowPipeline = _graphicsContext.CreateGraphicsPipelineBuilder()
            .WithVertexInput(new VertexInputDescriptorBuilder()
                .AddAttribute(0, DataType.Float, 3, 0)
                .Build("LocalLightAndShadowPipeline"))
            .WithShadersFromFiles("Shaders/LightsLocal.vs.glsl", "Shaders/LightsLocal.fs.glsl")
            .EnableCulling(CullMode.Front)
            .DisableDepthTest()
            .EnableBlending(ColorBlendAttachmentDescriptor.PreMultiplied)
            .Build("LocalLightAndShadowPipeline");
        if (localLightAndShadowPipeline.IsFailure)
        {
            _logger.Error("Unable to create 'LocalLightAndShadowPipeline' graphics pipeline {Details}", localLightAndShadowPipeline.Error);
            return false;
        }

        _localLightsAndShadowPipeline = localLightAndShadowPipeline.Value;

        var resolveDeferredPipelineResult = _graphicsContext.CreateGraphicsPipelineBuilder()
            .WithShadersFromFiles("Shaders/FST2.vs.glsl", "Shaders/Resolve.fs.glsl")
            .WithVertexInput(new VertexInputDescriptorBuilder()
                .AddAttribute(0, DataType.Float, 3, 0)
                .AddAttribute(0, DataType.Float, 2, 12)
                .Build("Deferred_Resolve_PositionUv"))
            .DisableCulling()
            .DisableDepthTest()
            .Build("ResolveDeferredPipeline");
        if (resolveDeferredPipelineResult.IsFailure)
        {
            _logger.Error("Unable to create 'Deferred_Resolve_PositionUv' graphics pipeline {Details}", resolveDeferredPipelineResult.Error);
            return false;
        }

        _resolveDeferredPipeline = resolveDeferredPipelineResult.Value;

        var directionalLightShadowGraphicsPipelineResult = _graphicsContext.CreateGraphicsPipelineBuilder()
            .WithShadersFromFiles("Shaders/Shadow.vs.glsl", "Shaders/Shadow.fs.glsl")
            .WithVertexInput(new VertexInputDescriptorBuilder()
                .AddAttribute(0, DataType.Float, 3, 0)
                .Build("DirectionalLightShadow"))
            .Build("DirectionalLightShadow");
        if (directionalLightShadowGraphicsPipelineResult.IsFailure)
        {
            _logger.Error("Unable to create 'DirectionalLightShadow' graphics pipeline {Details}", resolveDeferredPipelineResult.Error);
            return false;
        }

        _directionalLightShadowGraphicsPipeline = directionalLightShadowGraphicsPipelineResult.Value;

        return true;
    }

    private bool LoadSamplers()
    {
        var gBufferSamplerDescriptor = new SamplerDescriptor
        {
            MagFilter = Filter.Nearest,
            MinFilter = Filter.Nearest,
            AddressModeU = AddressMode.ClampToBorder,
            AddressModeV = AddressMode.ClampToBorder
        };
        _gBufferSampler = _graphicsContext.CreateSampler(gBufferSamplerDescriptor);

        var textureSamplerDescriptor = new SamplerDescriptor
        {
            MagFilter = Filter.Linear,
            MinFilter = Filter.Linear,
            MipmapFilter = Filter.Linear,
            MaxLod = 12,
            AddressModeU = AddressMode.ClampToEdge,
            AddressModeV = AddressMode.ClampToEdge,
        };
        _textureSampler = _graphicsContext.CreateSampler(textureSamplerDescriptor);

        var shadowSamplerDescriptor = new SamplerDescriptor
        {
            MagFilter = Filter.Nearest,
            MinFilter = Filter.Nearest,
            MipmapFilter = Filter.None,
            AddressModeU = AddressMode.ClampToBorder,
            AddressModeV = AddressMode.ClampToBorder
        };
        _shadowSampler = _graphicsContext.CreateSampler(shadowSamplerDescriptor);

        var blueNoiseSamplerDescriptor = new SamplerDescriptor
        {
            MagFilter = Filter.Nearest,
            MinFilter = Filter.Nearest,
            AddressModeU = AddressMode.Repeat,
            AddressModeV = AddressMode.Repeat
        };
        _blueNoiseSampler = _graphicsContext.CreateSampler(blueNoiseSamplerDescriptor);

        _blueNoise16Texture = _graphicsContext.CreateTextureFromFile("Data/Default/T_Bluenoise16.png", false);
        _blueNoise32Texture = _graphicsContext.CreateTextureFromFile("Data/Default/T_Bluenoise32.png", false);

        return true;
    }

    private bool LoadSkybox(string skyboxName)
    {
        var skyboxFileNames = new[]
        {
            $"Data/Sky/TC_{skyboxName}_Xp.png",
            $"Data/Sky/TC_{skyboxName}_Xn.png",
            $"Data/Sky/TC_{skyboxName}_Yp.png",
            $"Data/Sky/TC_{skyboxName}_Yn.png",
            $"Data/Sky/TC_{skyboxName}_Zp.png",
            $"Data/Sky/TC_{skyboxName}_Zn.png"
        };
        _skyboxTexture = _graphicsContext.CreateTextureCubeFromFile(skyboxFileNames);
        return _skyboxTexture != null;
    }

    private bool CreateConvolveSkybox()
    {
        var skyboxTextureCreateDescriptor = new TextureCreateDescriptor
        {
            ImageType = ImageType.TextureCube,
            Format = Format.R16G16B16A16Float,
            Label = "ConvolvedSkybox",
            Size = new Vector3i(1024, 1024, 1),
            MipLevels = (uint)(1 + MathF.Ceiling(MathF.Log2(1024))),
            SampleCount = SampleCount.OneSample
        };
        _skyboxConvolvedTexture = _graphicsContext.CreateTexture(skyboxTextureCreateDescriptor);

        var convolutionComputePipelineResult = _graphicsContext.CreateComputePipelineBuilder()
            .WithShaderFromFile("Shaders/ConvolveSkybox.cs.glsl")
            .Build("ComputeConvolution");
        if (convolutionComputePipelineResult.IsFailure)
        {
            _logger.Error("{Category}: Unable to compile compute pipeline. {Details}", "App",
                convolutionComputePipelineResult.Error);
            return false;
        }

        const int xSize = 16;
        const int ySize = 16;
        const int xGroups = (1024 + xSize - 1) / xSize;
        const int yGroups = (1024 + ySize - 1) / ySize;

        using var convolutionComputePipeline = convolutionComputePipelineResult.Value;
        _graphicsContext.BindComputePipeline(convolutionComputePipeline);

        convolutionComputePipeline.BindImage(
            _skyboxTexture!,
            0,
            0,
            0,
            MemoryAccess.ReadOnly,
            _skyboxTexture!.Format);

        convolutionComputePipeline.BindImage(
            _skyboxConvolvedTexture,
            1,
            0,
            0,
            MemoryAccess.WriteOnly,
            _skyboxConvolvedTexture.Format);
        convolutionComputePipeline.Dispatch(xGroups, yGroups, 6);
        _graphicsContext.InsertMemoryBarrier(BarrierMask.ShaderImageAccess);

        _skyboxConvolvedTexture.GenerateMipmaps();

        return true;
    }

    private void DestroySizeDependentResources()
    {
        _deferredGBufferBaseColorTexture?.Dispose();
        _deferredGBufferNormalTexture?.Dispose();
        _deferredGBufferMaterialIdTexture?.Dispose();
        _deferredGBufferDepthTexture?.Dispose();
        _deferredLBufferLightTexture?.Dispose();
        _deferredFinalFramebufferTexture?.Dispose();

        _graphicsContext.RemoveFramebuffer(_deferredGeometryFramebufferRenderDescriptor);
        _graphicsContext.RemoveFramebuffer(_deferredLightFramebufferRenderDescriptor);
        _graphicsContext.RemoveFramebuffer(_deferredFinalFramebufferRenderDescriptor);
    }

    private void CreateSizeDependentResources()
    {
        _deferredGBufferBaseColorTexture = _graphicsContext.CreateTexture2D(_applicationContext.ScaledFramebufferSize.X, _applicationContext.ScaledFramebufferSize.Y, Format.R8G8B8A8UNorm);
        _deferredGBufferNormalTexture = _graphicsContext.CreateTexture2D(_applicationContext.ScaledFramebufferSize.X, _applicationContext.ScaledFramebufferSize.Y, Format.R16G16B16A16SNorm);
        _deferredGBufferMaterialIdTexture = _graphicsContext.CreateTexture2D(_applicationContext.ScaledFramebufferSize.X, _applicationContext.ScaledFramebufferSize.Y, Format.R32UInt);

        _deferredGBufferDepthTexture = _graphicsContext.CreateTexture2D(_applicationContext.ScaledFramebufferSize.X, _applicationContext.ScaledFramebufferSize.Y, Format.D32Float);

        _deferredGeometryFramebufferRenderDescriptor = new FramebufferRenderDescriptorBuilder()
            .WithViewport(_applicationContext.ScaledFramebufferSize.X, _applicationContext.ScaledFramebufferSize.Y)
            .WithColorAttachment(_deferredGBufferBaseColorTexture, true, new Vector4(0.2f, 0.2f, 0.3f, 1.0f))
            .WithColorAttachment(_deferredGBufferNormalTexture, true, Vector4.Zero)
            .WithColorAttachment(_deferredGBufferMaterialIdTexture, true, Vector4.Zero)
            .WithDepthAttachment(_deferredGBufferDepthTexture, true)
            .Build("GBuffer");

        _deferredLBufferLightTexture = _graphicsContext.CreateTexture2D(_applicationContext.ScaledFramebufferSize.X, _applicationContext.ScaledFramebufferSize.Y, Format.R16G16B16A16Float);

        _deferredLightFramebufferRenderDescriptor = new FramebufferRenderDescriptorBuilder()
            .WithViewport(_applicationContext.ScaledFramebufferSize.X, _applicationContext.ScaledFramebufferSize.Y)
            .WithColorAttachment(_deferredLBufferLightTexture, true, Vector4.Zero)
            .Build("LBuffer");

        _deferredFinalFramebufferTexture = _graphicsContext.CreateTexture2D(_applicationContext.ScaledFramebufferSize.X, _applicationContext.ScaledFramebufferSize.Y, Format.R8G8B8UNorm);
        _deferredFinalFramebufferRenderDescriptor = new FramebufferRenderDescriptorBuilder()
            .WithViewport(_applicationContext.ScaledFramebufferSize.X, _applicationContext.ScaledFramebufferSize.Y)
            .WithColorAttachment(_deferredFinalFramebufferTexture, true, new Vector4(0.2f, 0.2f, 0.2f, 1.0f))
            .Build("Forward");
    }

    private bool IsStructuralBufferRenewalRequired()
    {
        var requireStructuralBufferRenewal = false; // reconstruct vb/ib/material/textures
        if (_meshDatesToBeRemoved.Count > 0)
        {
            foreach (var meshDataNameToBeRemoved in _meshDatesToBeRemoved)
            {
                _knownMeshDatesRefCounter[meshDataNameToBeRemoved]--;
                if (_knownMeshDatesRefCounter[meshDataNameToBeRemoved] == 0)
                {
                    _knownMeshDates.Remove(meshDataNameToBeRemoved);
                }
            }

            _meshDatesToBeRemoved.Clear();
            requireStructuralBufferRenewal = true;
        }

        if (_meshDatesToBeAdded.Count > 0)
        {
            foreach (var meshDataToBeAdded in _meshDatesToBeAdded)
            {
                if (!_knownMeshDates.ContainsKey(meshDataToBeAdded.Key))
                {
                    _knownMeshDates.Add(meshDataToBeAdded.Key, meshDataToBeAdded.Value);
                }

                if (_knownMeshDatesRefCounter.ContainsKey(meshDataToBeAdded.Key))
                {
                    _knownMeshDatesRefCounter[meshDataToBeAdded.Key]++;
                }
                else
                {
                    _knownMeshDatesRefCounter.Add(meshDataToBeAdded.Key, 1);
                }
            }

            _meshDatesToBeAdded.Clear();
            requireStructuralBufferRenewal = true;
        }

        if (_materialsToBeRemoved.Count > 0)
        {
            foreach (var materialNameToBeRemoved in _materialsToBeRemoved)
            {
                _knownMaterialsRefCounter[materialNameToBeRemoved]--;
                if (_knownMaterialsRefCounter[materialNameToBeRemoved] == 0)
                {
                    _knownMaterials.Remove(materialNameToBeRemoved);
                }
            }

            requireStructuralBufferRenewal = true;
        }

        if (_materialsToBeAdded.Count > 0)
        {
            foreach (var materialToBeAdded in _materialsToBeAdded)
            {
                if (!_knownMaterials.ContainsKey(materialToBeAdded.Key))
                {
                    _knownMaterials.Add(materialToBeAdded.Key, materialToBeAdded.Value);
                }

                if (_knownMaterialsRefCounter.ContainsKey(materialToBeAdded.Key))
                {
                    _knownMaterialsRefCounter[materialToBeAdded.Key]++;
                }
                else
                {
                    _knownMaterialsRefCounter.Add(materialToBeAdded.Key, 1);
                }
            }

            _materialsToBeAdded.Clear();
            requireStructuralBufferRenewal = true;
        }

        return requireStructuralBufferRenewal;
    }

    private void RebuildStructuralBuffers()
    {
        var knownMeshDates = _knownMeshDates.Values.ToArray();

        var indexOffset = 0;
        var vertexOffset = 0;
        foreach (var knownMeshData in knownMeshDates)
        {
            knownMeshData.VertexOffset = vertexOffset;
            knownMeshData.IndexOffset = indexOffset;
            vertexOffset += knownMeshData.VertexCount;
            indexOffset += knownMeshData.IndexCount;
        }

        _vertexBuffer?.Dispose();
        _vertexBuffer = _graphicsContext.CreateVertexBuffer("SceneVertices", knownMeshDates, VertexType.PositionNormalUvTangent);

        _indexBuffer?.Dispose();
        _indexBuffer = _graphicsContext.CreateIndexBuffer("SceneIndices", knownMeshDates);

        var imageDataPerMaterial = _imageLibrary.GetImageDataPerMaterial(_knownMaterials.Values.ToList());
        if (imageDataPerMaterial.Any())
        {
            var beforeAvailableVM = GL.GetInteger((uint)GL.GpuMemoryInfo.CurrentAvailableVideoMemory);

            _textureArrays = _textureLibrary.PrepareTextureArrays(imageDataPerMaterial, out var textureArrayIndices);

            var afterAvailableVM = GL.GetInteger((uint)GL.GpuMemoryInfo.CurrentAvailableVideoMemory);

            var deltaAvailableVM = beforeAvailableVM - afterAvailableVM;

            var materialBufferData = _materialLibrary.GetMaterialBufferData(_knownMaterials.Keys.ToArray(), textureArrayIndices, out _materialNameIndexMap);
            _gpuMaterialBuffer.Update(materialBufferData.ToArray(), 0);
        }
    }

    private Task HandleAddModelToScene(AddModelToScene message)
    {
        foreach (var modelMesh in message.Model.Meshes)
        {
            if (!_meshDatesToBeAdded.ContainsKey(modelMesh.MeshData.MeshName))
            {
                _meshDatesToBeAdded.Add(modelMesh.MeshData.MeshName, modelMesh.MeshData);
            }

            if (!_materialsToBeAdded.ContainsKey(modelMesh.Material.Name))
            {
                _materialsToBeAdded.Add(modelMesh.Material.Name, modelMesh.Material);
            }
        }
        return Task.CompletedTask;
    }

    private Task HandleAddModelMeshToScene(AddModelMeshToScene message)
    {
        if (!_meshDatesToBeAdded.ContainsKey(message.ModelMesh.MeshData.MeshName))
        {
            _meshDatesToBeAdded.Add(message.ModelMesh.MeshData.MeshName, message.ModelMesh.MeshData);
        }

        if (!_materialsToBeAdded.ContainsKey(message.ModelMesh.Material.Name))
        {
            _materialsToBeAdded.Add(message.ModelMesh.Material.Name, message.ModelMesh.Material);
        }

        return Task.CompletedTask;
    }

    private Task HandleRemoveModelFromScene(RemoveModelFromScene message)
    {
        foreach (var modelMesh in message.Model.Meshes)
        {
            if (!_materialsToBeRemoved.Contains(modelMesh.Material.Name))
            {
                _materialsToBeRemoved.Add(modelMesh.Material.Name);
            }

            if (!_meshDatesToBeRemoved.Contains(modelMesh.MeshData.MeshName))
            {
                _meshDatesToBeRemoved.Add(modelMesh.MeshData.MeshName);
            }
        }

        return Task.CompletedTask;
    }
}