using System.Runtime.InteropServices;
using Complex.Ecs.Components;
using EngineKit;
using EngineKit.Graphics;

namespace Complex.Ecs.Systems;

internal class PreRenderSystem : IPreRenderSystem
{
    private readonly ICamera _camera;

    private readonly IEntityRegistry _entityRegistry;

    private readonly IMaterialLibrary _materialLibrary;

    private readonly IRenderer _renderer;

    public PreRenderSystem(IEntityRegistry entityRegistry,
                           IRenderer renderer,
                           IMaterialLibrary materialLibrary,
                           ICamera camera)
    {
        _entityRegistry = entityRegistry;
        _renderer = renderer;
        _materialLibrary = materialLibrary;
        _camera = camera;
    }

    public void Update()
    {
        var globalLightEntities = _entityRegistry.GetEntitiesWithComponents<GlobalLightComponent>();
        var globalLightEntitiesSpan = CollectionsMarshal.AsSpan(globalLightEntities);
        for (var i = 0; i < globalLightEntities.Count; i++)
        {
            ref var globalLightEntity = ref globalLightEntitiesSpan[i];
        }

        var meshEntities = _entityRegistry.GetEntitiesWithComponents<ModelMeshComponent>();
        var meshEntitiesSpan = CollectionsMarshal.AsSpan(meshEntities);

        if (meshEntities.Count > 0) _renderer.Clear();

        var cameraFrustum = _camera.GetViewFrustum();

        for (var i = 0; i < meshEntities.Count; i++)
        {
            ref var meshEntity = ref meshEntitiesSpan[i];

            var meshComponent = _entityRegistry.GetComponent<ModelMeshComponent>(meshEntity.Id);
            var materialComponent = _entityRegistry.GetComponent<MaterialComponent>(meshEntity.Id);
            var material = materialComponent == null
                    ? _materialLibrary.GetMaterialByName("M_Default") //TODO(deccer): add a GetDefaultMaterial()
                    : materialComponent.Material;

            var meshGlobalMatrix = meshEntity.GetGlobalMatrix();
            var transformedMeshAabb = meshComponent!.MeshPrimitive.BoundingBox.Transform(meshGlobalMatrix);

            if (cameraFrustum.Intersects(transformedMeshAabb))
            {
                _renderer.AddMeshInstance(meshComponent.MeshPrimitive,
                        material,
                        meshGlobalMatrix,
                        transformedMeshAabb);
            }
        }
    }
}
