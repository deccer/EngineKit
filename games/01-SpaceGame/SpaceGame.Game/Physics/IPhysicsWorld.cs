using System;
using JoltPhysicsSharp;
using EngineKit.Mathematics;

namespace SpaceGame.Game.Physics;

public interface IPhysicsWorld : IDisposable
{
    bool Load();

    void Update(float deltaTime);

    Vector3 GetPosition(BodyID bodyId);

    Body CreateAndAddBody(MeshShapeSettings meshShapeSettings, Vector3 position);

    Body CreateAndAddBody(SphereShapeSettings meshShapeSettings, Vector3 position);

    Body CreateAndAddBody(BoxShapeSettings boxShapeSettings, Vector3 position);

    void RemoveBody(BodyID bodyId);
}