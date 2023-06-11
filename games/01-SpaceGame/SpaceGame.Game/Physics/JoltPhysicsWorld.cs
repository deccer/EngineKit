using System.Diagnostics;
using JoltPhysicsSharp;
using Quaternion = System.Numerics.Quaternion;
using Num = System.Numerics;
using Vector3 = EngineKit.Mathematics.Vector3;

namespace SpaceGame.Game.Physics;

public sealed class JoltPhysicsWorld : IPhysicsWorld
{
    private const uint MaxBodies = 1024;
    private const uint NumBodyMutexes = 0;
    private const uint MaxBodyPairs = 1024;
    private const uint MaxContactConstraints = 1024;

    private const int MaxPhysicsJobs = 2048;
    private const int MaxPhysicsBarriers = 8;

    private PhysicsSystem? _physicsSystem;
    private TempAllocator _tempAllocator;
    private JobSystemThreadPool _jobThreadPool;

    private BroadPhaseLayerInterface? _broadPhaseLayerImplementation;

    public JoltPhysicsWorld()
    {
        _physicsSystem = null;
    }

    public bool Load()
    {
        if (!Foundation.Init())
        {
            return false;
        }

        _tempAllocator = new TempAllocator(10 * 1024 * 1024);
        _jobThreadPool = new JobSystemThreadPool(MaxPhysicsJobs, MaxPhysicsBarriers);
        _broadPhaseLayerImplementation = new BroadPhaseLayerImplementation();

        _physicsSystem = new PhysicsSystem();
        _physicsSystem.Init(
            MaxBodies,
            NumBodyMutexes,
            MaxBodyPairs,
            MaxContactConstraints,
            _broadPhaseLayerImplementation, BroadPhaseCanCollide, ObjectCanCollide);
        _physicsSystem.Gravity = Num.Vector3.Zero;
        _physicsSystem.OnBodyActivated += PhysicsSystemOnOnBodyActivated;
        _physicsSystem.OnBodyDeactivated += PhysicsSystemOnOnBodyDeactivated;
        _physicsSystem.OnContactAdded += PhysicsSystemOnOnContactAdded;
        _physicsSystem.OnContactPersisted += PhysicsSystemOnOnContactPersisted;
        _physicsSystem.OnContactRemoved += PhysicsSystemOnOnContactRemoved;
        _physicsSystem.OnContactValidate += PhysicsSystemOnOnContactValidate;

        return true;
    }

    public Body CreateAndAddBody(MeshShapeSettings meshShapeSettings, Vector3 position)
    {
        var p = new Num.Vector3(position.X, position.Y, position.Z);
        var bodyCreationSettings = new BodyCreationSettings(meshShapeSettings, p, Quaternion.Identity, MotionType.Dynamic, Layers.Moving);
        var body = _physicsSystem.BodyInterface.CreateBody(bodyCreationSettings);
        body.SetLinearVelocity(Num.Vector3.Zero);
        body.SetAngularVelocity(Num.Vector3.Zero);

        _physicsSystem.BodyInterface.AddBody(body, ActivationMode.Activate);
        return body;
    }

    public Body CreateAndAddBody(SphereShapeSettings meshShapeSettings, Vector3 position)
    {
        var p = new Num.Vector3(position.X, position.Y, position.Z);
        var bodyCreationSettings = new BodyCreationSettings(meshShapeSettings, p, Quaternion.Identity, MotionType.Dynamic, Layers.Moving);
        var body = _physicsSystem.BodyInterface.CreateBody(bodyCreationSettings);
        body.SetLinearVelocity(Num.Vector3.Zero);
        body.SetAngularVelocity(new Num.Vector3(1000.1f, 0.2f, 0.3f));
        body.Restitution = 1.01f;

        _physicsSystem.BodyInterface.AddBody(body, ActivationMode.Activate);
        return body;
    }

    public Body CreateAndAddBody(BoxShapeSettings boxShapeSettings, Vector3 position)
    {
        var p = new Num.Vector3(position.X, position.Y, position.Z);
        var bodyCreationSettings = new BodyCreationSettings(boxShapeSettings, p, Quaternion.Identity, MotionType.Dynamic, Layers.Moving);
        var body = _physicsSystem.BodyInterface.CreateBody(bodyCreationSettings);
        body.SetLinearVelocity(Num.Vector3.Zero);
        body.SetAngularVelocity(Num.Vector3.Zero);

        _physicsSystem.BodyInterface.AddBody(body, ActivationMode.Activate);
        return body;
    }

    public void RemoveBody(BodyID bodyId)
    {
        _physicsSystem.BodyInterface.RemoveBody(bodyId);
    }

    public void Dispose()
    {
        _physicsSystem?.Dispose();
        Foundation.Shutdown();
    }

    public Vector3 GetPosition(BodyID bodyId)
    {
        var position = _physicsSystem.BodyInterface.GetCenterOfMassPosition(bodyId);

        return new Vector3(position.X, position.Y, position.Z);
    }

    public void Update(float deltaTime)
    {
        _physicsSystem.Update(1.0f / 60.0f, 1, 1, _tempAllocator, _jobThreadPool);
    }

    private static bool BroadPhaseCanCollide(ObjectLayer layer1, BroadPhaseLayer layer2)
    {
        switch (layer1)
        {
            case Layers.NonMoving:
                return layer2 == BroadPhaseLayers.Moving;
            case Layers.Moving:
                return true;
            default:
                Debug.Assert(false);
                return false;
        }
    }

    private static bool ObjectCanCollide(ObjectLayer layer1, ObjectLayer layer2)
    {
        switch (layer1)
        {
            case Layers.NonMoving:
                return layer2 == Layers.Moving;
            case Layers.Moving:
                return true;
            default:
                Debug.Assert(false);
                return false;
        }
    }

    private ValidateResult PhysicsSystemOnOnContactValidate(PhysicsSystem system, in Body body1, in Body body2, Num.Vector3 baseoffset, nint collisionresult)
    {
        return ValidateResult.AcceptContact;
    }

    private void PhysicsSystemOnOnContactRemoved(PhysicsSystem system, ref SubShapeIDPair subshapepair)
    {
    }

    private void PhysicsSystemOnOnContactPersisted(PhysicsSystem system, in Body body1, in Body body2)
    {
    }

    private void PhysicsSystemOnOnContactAdded(PhysicsSystem system, in Body body1, in Body body2)
    {
    }

    private void PhysicsSystemOnOnBodyDeactivated(PhysicsSystem system, in BodyID bodyid, ulong bodyuserdata)
    {
    }

    private void PhysicsSystemOnOnBodyActivated(PhysicsSystem system, in BodyID bodyid, ulong bodyuserdata)
    {
    }
}