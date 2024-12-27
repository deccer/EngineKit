using System.Runtime.InteropServices;
using Complex.Engine.Ecs.Components;
using EngineKit;
using EngineKit.Graphics;

namespace Complex.Engine.Ecs.Systems;

public class PreRenderSystem : IPreRenderSystem
{
    private readonly ICamera _camera;

    private readonly IEntityRegistry _entityRegistry;

    private readonly IMaterialLibrary _materialLibrary;

    private readonly IRenderer2 _renderer;

    public PreRenderSystem(IEntityRegistry entityRegistry,
                           IRenderer2 renderer,
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

        var entitiesWithMesh = _entityRegistry.GetEntitiesWithComponents<ModelMeshComponent>();
        var entitiesWithMeshSpan = CollectionsMarshal.AsSpan(entitiesWithMesh);

        if(entitiesWithMesh.Count > 0)
        {
            _renderer.Clear();
        }

        var cameraFrustum = _camera.GetViewFrustum();

        for (var i = 0; i < entitiesWithMesh.Count; i++)
        {
            ref var entityWithMesh = ref entitiesWithMeshSpan[i];

            var meshComponent = _entityRegistry.GetComponent<ModelMeshComponent>(entityWithMesh.Id);
            var materialComponent = _entityRegistry.GetComponent<MaterialComponent>(entityWithMesh.Id);
            var material = materialComponent == null
                    ? _materialLibrary.GetMaterialByName("M_Default") //TODO(deccer): add a GetDefaultMaterial()
                    : materialComponent.Material;

            var meshGlobalMatrix = entityWithMesh.GetGlobalMatrix();
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
