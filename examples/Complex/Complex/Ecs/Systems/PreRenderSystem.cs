using System.Runtime.InteropServices;
using Complex.Ecs.Components;
using EngineKit;
using EngineKit.Graphics;

namespace Complex.Ecs.Systems;

internal class PreRenderSystem : IPreRenderSystem
{
    private readonly IEntityWorld _entityWorld;
    private readonly IRenderer _renderer;
    private readonly IMaterialLibrary _materialLibrary;
    private readonly ICamera _camera;

    public PreRenderSystem(
        IEntityWorld entityWorld,
        IRenderer renderer,
        IMaterialLibrary materialLibrary,
        ICamera camera)
    {
        _entityWorld = entityWorld;
        _renderer = renderer;
        _materialLibrary = materialLibrary;
        _camera = camera;
    }

    public void Update()
    {
        var globalLightEntities = _entityWorld.GetEntitiesWithComponents<GlobalLightComponent>();
        var globalLightEntitiesSpan = CollectionsMarshal.AsSpan(globalLightEntities);
        for (var i = 0; i < globalLightEntities.Count; i++)
        {
            ref var globalLightEntity = ref globalLightEntitiesSpan[i];
        }
        
        var meshEntities = _entityWorld.GetEntitiesWithComponents<ModelMeshComponent>();
        var meshEntitiesSpan = CollectionsMarshal.AsSpan(meshEntities);

        if (meshEntities.Count > 0)
        {
            _renderer.Clear();
        }

        var cameraFrustum = _camera.GetViewFrustum();

        for (var i = 0; i < meshEntities.Count; i++)
        {
            ref var meshEntity = ref meshEntitiesSpan[i];
           
            var meshComponent = _entityWorld.GetComponent<ModelMeshComponent>(meshEntity.Id);
            var materialComponent = _entityWorld.GetComponent<MaterialComponent>(meshEntity.Id);
            var material = materialComponent == null
                ? _materialLibrary.GetMaterialByName("M_Default") //TODO(deccer): add a GetDefaultMaterial()
                : materialComponent.Material;

            var meshGlobalMatrix = meshEntity.GetGlobalMatrix();
            var transformedMeshAabb = meshComponent!.MeshPrimitive.BoundingBox.Transform(meshGlobalMatrix);
            
            if (cameraFrustum.Intersects(transformedMeshAabb))
            {
                _renderer.AddMeshInstance(meshComponent.MeshPrimitive, material, meshGlobalMatrix, transformedMeshAabb);
            }
        }
    }
}