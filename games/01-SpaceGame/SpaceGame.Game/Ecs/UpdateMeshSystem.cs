using System.Collections.Generic;
using EngineKit.Graphics;

namespace SpaceGame.Game.Ecs;

public class UpdateMeshSystem : ISystem
{
    private readonly IEntityWorld _entityWorld;
    private readonly IMessageBus _messageBus;

    public UpdateMeshSystem(IEntityWorld entityWorld, IMessageBus messageBus)
    {
        _entityWorld = entityWorld;
        _messageBus = messageBus;
    }

    public void Update(float deltaTime)
    {
        var entities = _entityWorld.GetEntitiesWithComponents<UpdateMeshSystemComponent>();
        var meshDates = new List<MeshData>();
        foreach (var entity in entities)
        {
            var modelMeshComponent = _entityWorld.GetComponent<ModelMeshComponent>(entity.Id);
            if (!meshDates.Contains(modelMeshComponent.MeshData))
            {
                meshDates.Add(modelMeshComponent.MeshData);
            }

            _entityWorld.RemoveComponent<UpdateMeshSystemComponent>(entity.Id);
        }

        //_messageBus.PublishWait(new UpdateMeshSystemMessage(meshDates));
    }
}