using System.Numerics;
using BepuPhysics;
using BepuUtilities;

namespace Complex;

public struct PoseIntegratorCallbacks : IPoseIntegratorCallbacks
{
    public AngularIntegrationMode AngularIntegrationMode { get; }
    
    public bool AllowSubstepsForUnconstrainedBodies { get; }
    
    public bool IntegrateVelocityForKinematics { get; }
    
    public void Initialize(Simulation simulation)
    {
    }

    public void PrepareForIntegration(float dt)
    {
    }

    public void IntegrateVelocity(
        Vector<int> bodyIndices,
        Vector3Wide position,
        QuaternionWide orientation,
        BodyInertiaWide localInertia,
        Vector<int> integrationMask,
        int workerIndex,
        Vector<float> dt,
        ref BodyVelocityWide velocity)
    {
    }
}