using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EngineKit;
using EngineKit.Graphics;
using EngineKit.Input;
using EngineKit.Native.Glfw;
using EngineKit.Native.OpenGL;
using ImGuiNET;
using JoltPhysicsSharp;
using Microsoft.Extensions.Options;
using OpenTK.Mathematics;
using Serilog;
using SpaceGame.Game;
using SpaceGame.Game.Ecs;
using SpaceGame.Game.Messages;
using SpaceGame.Game.Physics;
using Num = System.Numerics;
using Entity = SpaceGame.Game.Ecs.Entity;
using IApplicationContext = EngineKit.IApplicationContext;
using MathHelper = EngineKit.MathHelper;
using Quaternion = OpenTK.Mathematics.Quaternion;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace SpaceGame;

internal class ConflictGameApplication : GraphicsApplication
{
    private readonly ILogger _logger;
    private readonly IApplicationContext _applicationContext;
    private readonly IMetrics _metrics;
    private readonly IInputProvider _inputProvider;
    private readonly IMessageBus _messageBus;

    private SwapchainRenderDescriptor _swapchainRenderDescriptor;
    private readonly IRenderer _deferredRenderer;
    private readonly IRendererContext _rendererContext;
    private readonly IModelLibrary _modelLibrary;
    private readonly IMaterialLibrary _materialLibrary;

    private readonly ICamera _camera;
    private readonly IImageLibrary _imageLibrary;

    private Num.Vector3 _directionalLightPosition = new Num.Vector3(-170f, 180f, -170f);

    private readonly EntityWorld _entityWorld;
    private readonly IList<int> _shadowMapResolutions;
    private int _shadowMapResolutionSelected;

    private bool _mouseIsActive = false;

    private readonly IPhysicsWorld? _physicsWorld;

    private int _playerShipEntity;
    private Body _playerShipBody;

    public ConflictGameApplication(
        ILogger logger,
        IApplicationContext applicationContext,
        IOptions<WindowSettings> windowSettings,
        IMetrics metrics,
        IOptions<ContextSettings> contextSettings,
        IGraphicsContext graphicsContext,
        IInputProvider inputProvider,
        IMessageBus messageBus,
        IUIRenderer uiRenderer,
        ICamera camera,
        IImageLibrary imageLibrary,
        IRenderer deferredRenderer,
        IRendererContext rendererContext,
        IModelLibrary modelLibrary,
        IMaterialLibrary materialLibrary,
        IPhysicsWorld? physicsWorld)
        : base(logger, windowSettings, contextSettings, applicationContext, metrics, inputProvider, graphicsContext, uiRenderer)
    {
        _logger = logger.ForContext<ConflictGameApplication>();
        _applicationContext = applicationContext;
        _metrics = metrics;
        _inputProvider = inputProvider;
        _messageBus = messageBus;
        _camera = camera;
        _imageLibrary = imageLibrary;
        _deferredRenderer = deferredRenderer;
        _rendererContext = rendererContext;
        _modelLibrary = modelLibrary;
        _materialLibrary = materialLibrary;
        _physicsWorld = physicsWorld;
        _entityWorld = new EntityWorld(physicsWorld, camera);
        _shadowMapResolutions = new List<int> { 16, 32, 64, 128, 256, 512, 1024, 2048 };
        _shadowMapResolutionSelected = 1024;
        _selectedMaterialName = string.Empty;
    }

    protected override void Unload()
    {
        _deferredRenderer.Dispose();

        _physicsWorld.Dispose();

        base.Unload();
    }

    protected override void Update()
    {
        base.Update();
        _physicsWorld.Update(_metrics.DeltaTime);
        _entityWorld.Update(_metrics.DeltaTime);

        if (_inputProvider.KeyboardState.IsKeyDown(Glfw.Key.KeyEscape))
        {
            Close();
        }

        if (_inputProvider.MouseState.IsButtonDown(Glfw.MouseButton.ButtonRight))
        {
            _camera.ProcessMouseMovement();
        }

        /*
        _mouseIsActive = _inputProvider.KeyboardState.IsKeyPressed(Glfw.Key.KeyLeftCtrl);

        if (_mouseIsActive)
        {
            ShowCursor();
        }
        else
        {
            HideCursor();
        }
        */

        var movement = Vector3.Zero;
        var speedFactor = 50f;

        if (_inputProvider.KeyboardState.IsKeyDown(Glfw.Key.KeyW))
        {
            movement += _camera.Direction;
        }

        if (_inputProvider.KeyboardState.IsKeyDown(Glfw.Key.KeyS))
        {
            movement -= _camera.Direction;
        }

        if (_inputProvider.KeyboardState.IsKeyDown(Glfw.Key.KeyA))
        {
            movement -= _camera.Right;
        }

        if (_inputProvider.KeyboardState.IsKeyDown(Glfw.Key.KeyD))
        {
            movement += _camera.Right;
        }

        movement = Vector3.Normalize(movement);
        if (_inputProvider.KeyboardState.IsKeyDown(Glfw.Key.KeyLeftShift))
        {
            movement *= speedFactor;
        }

        if (movement.Length > 0.0f)
        {
            //movement *= 0.999f;
            //_camera.ProcessKeyboard(movement, 1 / 60.0f);
            _playerShipBody.AddImpulseAtPosition(new Num.Vector3(movement.X, movement.Y, movement.Z) * 100, Num.Vector3.Zero);
        }

        if (_inputProvider.KeyboardState.IsKeyDown(Glfw.Key.KeyF1))
        {
            _rendererContext.DrawMode = DrawMode.Default;
        }

        if (_inputProvider.KeyboardState.IsKeyDown(Glfw.Key.KeyF2))
        {
            _rendererContext.DrawMode = DrawMode.Normal;
        }

        if (_inputProvider.KeyboardState.IsKeyDown(Glfw.Key.KeyF3))
        {
            _rendererContext.DrawMode = DrawMode.Depth;
        }

        if (_inputProvider.KeyboardState.IsKeyDown(Glfw.Key.KeyF4))
        {
            _rendererContext.DrawMode = DrawMode.LightBuffer;
        }

        if (_inputProvider.KeyboardState.IsKeyDown(Glfw.Key.KeyF5))
        {
            _rendererContext.DrawMode = DrawMode.BaseColor;
        }

        if (_inputProvider.KeyboardState.IsKeyDown(Glfw.Key.KeyF6))
        {
            _rendererContext.DrawMode = DrawMode.DirectionalShadowmap;
        }
    }

    protected override bool Load()
    {
        if (!base.Load())
        {
            return false;
        }

        if (!_physicsWorld.Load())
        {
            return false;
        }

        _imageLibrary.FlipHorizontal = false;
        _imageLibrary.FlipVertical = false;

        _swapchainRenderDescriptor = new SwapchainRenderDescriptorBuilder()
            .WithViewport(
                _applicationContext.FramebufferSize.X,
                _applicationContext.FramebufferSize.Y)
            .WithScissorRectangle(
                0,
                0,
                _applicationContext.FramebufferSize.X,
                _applicationContext.FramebufferSize.Y)
            .ClearColor(Color4.Black)
            .ClearDepth()
            .Build();

        if (!_deferredRenderer.Load())
        {
            return false;
        }

        SetupDefaultScene();

        _camera.ProcessMouseMovement();

        var playerShipPosition = new Vector3(0, 0, 10);
        var physicsModelMeshShapeComponent = new PhysicsModelMeshShapeComponent(_modelLibrary.GetMeshDataByName("Cube.003"));
        _playerShipBody = _physicsWorld.CreateAndAddBody(physicsModelMeshShapeComponent.SphereShapeSettings, playerShipPosition);

        _playerShipEntity = _entityWorld.CreateEntity("Player");
        _entityWorld.AddComponent(_playerShipEntity, new TransformComponent());
        _entityWorld.AddComponent(_playerShipEntity, physicsModelMeshShapeComponent);
        _entityWorld.AddComponent(_playerShipEntity, new PhysicsBodyComponent(_playerShipBody));
        _entityWorld.AddComponent(_playerShipEntity, new UpdateCameraPositionComponent());

        return true;
    }

    protected override void Render()
    {
        _deferredRenderer.PrepareScene(_entityWorld);

        _deferredRenderer.RenderScene(
            _camera,
            new Vector3(_directionalLightPosition.X, _directionalLightPosition.Y, _directionalLightPosition.Z));

        GL.PushDebugGroup("UI");
        {
            RenderUi();
            GL.PopDebugGroup();
        }
    }

    protected override void HandleDebugger(out bool breakOnError)
    {
        breakOnError = true;
    }

    protected override void FramebufferResized()
    {
        base.FramebufferResized();
        _swapchainRenderDescriptor = new SwapchainRenderDescriptorBuilder()
            .WithViewport(
                _applicationContext.FramebufferSize.X,
                _applicationContext.FramebufferSize.Y)
            .WithScissorRectangle(
                0,
                0,
                _applicationContext.FramebufferSize.X,
                _applicationContext.FramebufferSize.Y)
            .ClearColor(Color4.Black)
            .ClearDepth()
            .Build();

        _deferredRenderer.Resize();
        _camera.UpdateCameraVectors();
    }

    private void SetupDefaultScene()
    {
        _modelLibrary.AddModel("Deccer-Cubes", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/Default/SM_Deccer_Cubes_Textured.gltf"));
        _modelLibrary.AddModel("Asteroid-Pack", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/Props/asteroids_pack_rocky_version/scene.gltf"));
    }

    private void RenderUi()
    {
        GraphicsContext.BeginRenderToSwapchain(_swapchainRenderDescriptor);
        GraphicsContext.BlitFramebufferToSwapchain(
            _applicationContext.ScaledFramebufferSize.X,
            _applicationContext.ScaledFramebufferSize.Y,
            _applicationContext.FramebufferSize.X,
            _applicationContext.FramebufferSize.Y);

        UIRenderer.BeginLayout();
        ImGui.DockSpaceOverViewport(null, ImGuiDockNodeFlags.PassthruCentralNode);
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Quit"))
                    {
                        Close();
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("View"))
                {
                    if (ImGui.MenuItem("Wireframe"))
                    {
                        _rendererContext.UseWireframe = !_rendererContext.UseWireframe;
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }

            ImGui.EndMainMenuBar();
        }

        ImGui.ShowDemoWindow();

        if (ImGui.Begin("Debug"))
        {
            if (ImGui.Button("Set Light Pos To Cam Pos"))
            {
                _directionalLightPosition = new Num.Vector3(_camera.Position.X, _camera.Position.Y, _camera.Position.Z);
            }

            if (ImGui.BeginCombo("ShadowMap Resolution", _shadowMapResolutions.First(smr => smr == _shadowMapResolutionSelected).ToString()))
            {
                for (var i = 0; i < _shadowMapResolutions.Count; i++)
                {
                    var isSelected = _shadowMapResolutionSelected == _shadowMapResolutions[i];
                    if (ImGui.Selectable(_shadowMapResolutions[i].ToString(), isSelected))
                    {
                        _shadowMapResolutionSelected = _shadowMapResolutions[i];
                        _deferredRenderer.CreateShadowMaps(_shadowMapResolutionSelected, _shadowMapResolutionSelected);
                    }

                    if (isSelected)
                    {
                        ImGui.SetItemDefaultFocus();
                    }
                }

                ImGui.EndCombo();
            }

            _deferredRenderer.RenderShadowDebugUi();

            ImGui.End();
        }

        if (ImGui.Begin("Scene"))
        {
            RenderUiSceneTree();
            ImGui.End();
        }

        if (ImGui.Begin("Materials"))
        {
            RenderUiMaterials();
            ImGui.End();
        }

        if (ImGui.Begin("Models"))
        {
            if (ImGui.Button("Create Asteroid field"))
            {
                CreateAsteroidField();
                _messageBus.PublishWait(new AddModelToScene(_modelLibrary.Models.OfType<Model>().First(m => m.Name == "Asteroid-Pack")));
            }
            RenderUiAssets();

            ImGui.End();
        }

        UIRenderer.EndLayout();
        GraphicsContext.EndRender();
    }

    private void CreateAsteroidField(int? parentEntity = null)
    {
        var asteroidPackModel = _modelLibrary.Models.OfType<Model>().FirstOrDefault(a => a.Name == "Asteroid-Pack");
        if (asteroidPackModel == null)
        {
            return;
        }

        var asteroidMeshes = asteroidPackModel.Meshes.OrderBy(m => new Guid()).ToArray();

        var asteroidField = _entityWorld.CreateEntity("asteroid-field", parentEntity);
        _entityWorld.AddComponent(asteroidField, TransformComponent.CreateFromMatrix(Matrix4.Identity));

        var dimension = 192;
        var random = new Random();
        for (var i = 0; i < 384; i++)
        {
            var asteroidMesh = asteroidMeshes.ElementAt(random.Next(0, asteroidMeshes.Length));
            var childEntity = _entityWorld.CreateEntity(asteroidMesh.MeshData.MeshName + i, asteroidField);

            var position = new Vector3(-(dimension / 2) + dimension * random.NextSingle(), -(dimension / 2) + dimension * random.NextSingle(), -(dimension / 2) + dimension * random.NextSingle());
            var scale = new Vector3(random.NextSingle());

            var rotation = Quaternion.Identity;
            var randomAxis = random.Next(0, 3);
            if (randomAxis == 1)
            {
                rotation = Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(-180 + 360 * random.NextSingle()));
            }
            else if (randomAxis == 2)
            {
                rotation = Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(-180 + 360 * random.NextSingle()));
            }
            else if (randomAxis == 3)
            {
                rotation = Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(-180 + 360 * random.NextSingle()));
            }

            var transform = Matrix4.CreateTranslation(position) *
                            Matrix4.CreateFromQuaternion(rotation) *
                            Matrix4.CreateScale(scale);
            var physicsModelMeshShapeComponent = new PhysicsModelMeshShapeComponent(asteroidMesh);
            var body = _physicsWorld.CreateAndAddBody(physicsModelMeshShapeComponent.SphereShapeSettings, position);

            _entityWorld.AddComponent(childEntity, TransformComponent.CreateFromMatrix(transform));
            _entityWorld.AddComponent(childEntity, new ModelMeshComponent { MeshData = asteroidMesh.MeshData });
            _entityWorld.AddComponent(childEntity, physicsModelMeshShapeComponent);
            _entityWorld.AddComponent(childEntity, new PhysicsBodyComponent(body));
        }
    }

    private void CreateEntityFromModel(Model model, int? parentEntity = null)
    {
        var entity = _entityWorld.CreateEntity(model.Meshes.First().MeshData.MeshName, parentEntity);
        _entityWorld.AddComponent(entity, TransformComponent.CreateFromMatrix(Matrix4.Identity));

        foreach (var modelMesh in model.Meshes)
        {
            var childEntity = _entityWorld.CreateEntity(modelMesh.MeshData.MeshName, entity);

            _entityWorld.AddComponent(childEntity, TransformComponent.CreateFromMatrix(modelMesh.MeshData.Transform));
            _entityWorld.AddComponent(childEntity, new ModelMeshComponent { MeshData = modelMesh.MeshData });
        }
    }

    private void CreateEntityFromModelMesh(ModelMesh modelMesh, int? parentEntity = null)
    {
        var entity = _entityWorld.CreateEntity(modelMesh.MeshData.MeshName, parentEntity);

        _entityWorld.AddComponent(entity, TransformComponent.CreateFromMatrix(modelMesh.MeshData.Transform));
        _entityWorld.AddComponent(entity, new ModelMeshComponent { MeshData = modelMesh.MeshData });
    }

    private void RenderUiAssets()
    {
        foreach (var model in _modelLibrary.Models)
        {
            RenderUiModelAsset(model);
        }
    }

    private void RenderUiModelAsset(Model model)
    {
        if (ImGui.TreeNode($"{model.Name}"))
        {
            if (ImGui.Button("Create Instance"))
            {
                CreateEntityFromModel(model);
                _messageBus.PublishWait(new AddModelToScene(model));
            }

            foreach (var modelMesh in model.Meshes)
            {
                if (ImGui.TreeNode($"{modelMesh.MeshData.MeshName}"))
                {
                    if (ImGui.Button("Create Instance"))
                    {
                        CreateEntityFromModelMesh(modelMesh);
                        _messageBus.PublishWait(new AddModelMeshToScene(modelMesh));
                    }

                    ImGui.TreePop();
                }
            }
            ImGui.TreePop();
        }
    }

    private void RenderUiSceneTree()
    {
        var entities = _entityWorld.GetEntitiesWithComponents<TransformComponent>();
        foreach (var entity in entities)
        {
            RenderUiSceneEntity(entity);
        }
    }

    private void RenderUiSceneEntity(Entity entity)
    {
        if (ImGui.TreeNode(entity.Name))
        {
            var transformComponent = entity.GetComponent<TransformComponent>();
            var position = new Num.Vector3(transformComponent.LocalPosition.X, transformComponent.LocalPosition.Y, transformComponent.LocalPosition.Z);

            if (ImGui.DragFloat3("Position", ref position))
            {
                transformComponent.LocalPosition = new Vector3(position.X, position.Y, position.Z);
            }

            foreach (var child in entity.Children)
            {
                RenderUiSceneEntity(entity);
            }
            ImGui.TreePop();
        }
    }

    private string _selectedMaterialName;

    private void RenderUiMaterials()
    {
        var materialNames = _materialLibrary.GetMaterialNames();
        foreach (var materialName in materialNames)
        {
            var isMaterialSelected = _selectedMaterialName == materialName;
            if (ImGui.CollapsingHeader(materialName))
            {
                _selectedMaterialName = materialName;

                var material = _materialLibrary.GetMaterialByName(materialName);
                var baseColor = new Num.Vector4(material.BaseColor.R, material.BaseColor.G, material.BaseColor.G, material.BaseColor.A);
                if (ImGui.ColorPicker4("Base Color", ref baseColor))
                {
                    material.BaseColor = new Color4(baseColor.X, baseColor.Y, baseColor.Z, baseColor.W);
                }

                var emission = new Num.Vector4(material.Emissive.R, material.Emissive.G, material.Emissive.G, material.Emissive.A);
                if (ImGui.ColorPicker4("Emission", ref emission))
                {
                    material.Emissive = new Color4(emission.X, emission.Y, emission.Z, emission.W);
                }

                if (!string.IsNullOrEmpty(material.BaseColorTextureDataName))
                {
                    if (ImGui.CollapsingHeader(material.BaseColorTextureDataName))
                    {
                        if (!string.IsNullOrEmpty(material.BaseColorTextureFilePath))
                        {
                            ImGui.TextUnformatted(material.BaseColorTextureFilePath);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(material.NormalTextureDataName))
                {
                    if (ImGui.CollapsingHeader(material.NormalTextureDataName))
                    {
                        if (!string.IsNullOrEmpty(material.NormalTextureFilePath))
                        {
                            ImGui.TextUnformatted(material.NormalTextureFilePath);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(material.SpecularTextureDataName))
                {
                    if (ImGui.CollapsingHeader(material.SpecularTextureDataName))
                    {
                        if (!string.IsNullOrEmpty(material.SpecularTextureFilePath))
                        {
                            ImGui.TextUnformatted(material.SpecularTextureFilePath);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(material.MetalnessRoughnessTextureDataName))
                {
                    if (ImGui.CollapsingHeader(material.MetalnessRoughnessTextureDataName))
                    {
                        if (!string.IsNullOrEmpty(material.MetalnessRoughnessTextureFilePath))
                        {
                            ImGui.TextUnformatted(material.MetalnessRoughnessTextureFilePath);
                        }
                    }
                }
            }
        }
    }
}