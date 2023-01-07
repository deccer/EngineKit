using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using JoltPhysicsSharp;

namespace SpaceGame.Game.Ecs;

public class PhysicsModelMeshShapeComponent : Component
{
    public PhysicsModelMeshShapeComponent(ModelMesh modelMesh)
    {
        var vertices = modelMesh.MeshData.Positions.Select(p => new System.Numerics.Vector3(p.X, p.Y, p.Z)).ToArray();
        var indexedTriangles = new List<IndexedTriangle>();
        for (var i = 0; i < modelMesh.MeshData.Indices.Count; i += 3)
        {
            var index0 = modelMesh.MeshData.Indices[i + 0];
            var index1 = modelMesh.MeshData.Indices[i + 1];
            var index2 = modelMesh.MeshData.Indices[i + 2];
            indexedTriangles.Add(new IndexedTriangle(index0, index1, index2));
        }
        MeshShapeSettings = new MeshShapeSettings(vertices, indexedTriangles.ToArray());

        var halfSize = new Vector3(5, 5, 5);// modelMesh.MeshData.BoundingBox.HalfSize;
        BoxShapeSettings = new BoxShapeSettings(new Vector3(halfSize.X, halfSize.Y, halfSize.Z), 0.0f);

        SphereShapeSettings = new SphereShapeSettings(2);
    }

    public MeshShapeSettings MeshShapeSettings { get; }

    public BoxShapeSettings BoxShapeSettings { get; }

    public SphereShapeSettings SphereShapeSettings { get; }
}