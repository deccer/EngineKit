using System;
using System.Numerics;
using BepuPhysics;
using BepuUtilities;
using BepuUtilities.Memory;

namespace Complex.Physics;

internal class PhysicsWorld : IPhysicsWorld
{
    private readonly BufferPool _bufferPool;

    private readonly Simulation _simulation;

    private readonly ThreadDispatcher _threadDispatcher;

    public PhysicsWorld()
    {
        _bufferPool = new BufferPool();
        _threadDispatcher = new ThreadDispatcher(Environment.ProcessorCount - 2);
        _simulation = Simulation.Create(
            _bufferPool,
            new NarrowPhaseCallbacks(),
            new PoseIntegratorCallbacks(),
            new SolveDescription(1, 60));
    }

    public void Update()
    {
        _simulation.Timestep(1.0f / 60.0f, _threadDispatcher);
    }

    public Matrix4x4 GetBodyPoseByBodyHandle(BodyHandle handle)
    {
        var bodyReference = _simulation.Bodies.GetBodyReference(handle);
        return Matrix4x4.CreateScale(1.0f) *
               Matrix4x4.CreateFromQuaternion(bodyReference.Pose.Orientation) *
               Matrix4x4.CreateTranslation(bodyReference.Pose.Position);
    }

    public void Dispose()
    {
        _bufferPool.Clear();
        _threadDispatcher.Dispose();
    }
}
