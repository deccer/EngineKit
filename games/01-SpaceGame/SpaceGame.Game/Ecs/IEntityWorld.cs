using System.Collections.Generic;

namespace SpaceGame.Game.Ecs;

public interface IEntityWorld
{
    int CreateEntity(string name, int? parent = null);
    Entity GetEntity(int entityId);
    void AddComponent<T>(int entityId, T component) where T : Component;
    void RemoveComponent<T>(int entityId) where T : Component;

    List<Entity> GetEntitiesWithComponents<TComponent>()
        where TComponent : Component;

    List<Entity> GetEntitiesWithComponents<TComponent1, TComponent2>()
        where TComponent1 : Component
        where TComponent2 : Component;

    void Update(float deltaTime);
    T? GetComponent<T>(int entityId) where T : Component;
    IList<T> GetComponents<T>() where T : Component;
}