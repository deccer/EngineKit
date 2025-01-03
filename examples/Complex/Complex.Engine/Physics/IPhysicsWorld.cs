using System;
using System.Numerics;
using BepuPhysics;

namespace Complex.Engine.Physics;

public interface IPhysicsWorld : IDisposable
{
    void Update();

    Matrix4x4 GetBodyPoseByBodyHandle(BodyHandle handle);
}
