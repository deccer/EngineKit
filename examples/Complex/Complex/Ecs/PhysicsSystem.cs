using System;
using Arch.Core;
using Arch.System;
using BepuPhysics;
using BepuUtilities;
using BepuUtilities.Memory;

namespace Complex.Ecs;

public partial class PreRenderSystem : BaseSystem<World, float>
{
    public PreRenderSystem(World world) : base(world)
    {
    }

    [Query]
    public void UpdateTransforms([Data] float deltaTime, ref ParentOf parentOf, ref TransformComponent transformComponent)
    {
        
    }
}

internal class PhysicsSystem : BaseSystem<World, float>
{
    private readonly BufferPool _bufferPool;
    private readonly ThreadDispatcher _threadDispatcher;
    private readonly Simulation _simulation;

    public PhysicsSystem(World world) : base(world)
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

    public override void Dispose()
    {
        _bufferPool.Clear();
        base.Dispose();
    }
}