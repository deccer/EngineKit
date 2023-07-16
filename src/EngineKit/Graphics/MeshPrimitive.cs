using System;
using System.Collections.Generic;
using System.Linq;
using EngineKit.Mathematics;

namespace EngineKit.Graphics;

public sealed class MeshPrimitive
{
    private readonly List<Vector3> _positions;

    private readonly List<Vector3> _normals;

    private readonly List<Vector3> _colors;

    private readonly List<Vector2> _uvs;

    private readonly List<Vector3> _tangents;

    private readonly List<Vector3> _biTangents;

    private readonly List<Vector4> _realTangents;

    private readonly List<uint> _indices;

    public string MeshName { get; set; }

    public Matrix Transform { get; set; }

    public List<uint> Indices => _indices;

    public int IndexCount => _indices.Count;

    public VertexType VertexType { get; private set; }

    public int VertexCount => _positions.Count;

    public int IndexOffset { get; set; }

    public int VertexOffset { get; set; }

    public string? MaterialName { get; set; }

    public BoundingBox BoundingBox { get; set; }

    public List<Vector3> Positions => _positions;

    public List<Vector3> Normals => _normals;

    public List<Vector2> Uvs => _uvs;

    public List<Vector3> Colors => _colors;

    public List<Vector3> Tangents => _tangents;

    public List<Vector3> Bitangents => _biTangents;

    public List<Vector4> RealTangents => _realTangents;

    public bool HasIndices { get; private set; }

    public int Stride
    {
        get
        {
            return VertexType switch
            {
                VertexType.Default => 0,
                VertexType.Unknown => 0,
                VertexType.Position => 12,
                VertexType.PositionColor => 24,
                VertexType.PositionColorNormal => 36,
                VertexType.PositionColorNormalUv => 44,
                VertexType.PositionColorUv => 32,
                VertexType.PositionNormal => 24,
                VertexType.PositionNormalUv => 32,
                VertexType.PositionNormalUvTangent => 48,
                VertexType.PositionUv => 20,
                VertexType.ImGui => 20,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public MeshPrimitive(string meshName)
    {
        MeshName = meshName;
        MaterialName = "Default";
        VertexType = VertexType.Unknown;
        _positions = new List<Vector3>(512);
        _normals = new List<Vector3>(512);
        _colors = new List<Vector3>(512);
        _uvs = new List<Vector2>(512);
        _tangents = new List<Vector3>(512);
        _biTangents = new List<Vector3>(512);
        _realTangents = new List<Vector4>(512);
        _indices = new List<uint>(1024);
        HasIndices = false;
    }

    public static MeshPrimitive Combine(params MeshPrimitive[] meshPrimitives)
    {
        var combinedMeshPrimitive = new MeshPrimitive(Guid.NewGuid().ToString());
        combinedMeshPrimitive.VertexType = meshPrimitives.First().VertexType;

        foreach (var meshPrimitive in meshPrimitives)
        {
            combinedMeshPrimitive._positions.AddRange(meshPrimitive._positions);
            combinedMeshPrimitive._normals.AddRange(meshPrimitive._normals);
            combinedMeshPrimitive._colors.AddRange(meshPrimitive._colors);
            combinedMeshPrimitive._uvs.AddRange(meshPrimitive._uvs);
            combinedMeshPrimitive._tangents.AddRange(meshPrimitive._tangents);
            combinedMeshPrimitive._biTangents.AddRange(meshPrimitive._biTangents);
            combinedMeshPrimitive._realTangents.AddRange(meshPrimitive._realTangents);
            combinedMeshPrimitive._indices.AddRange(meshPrimitive._indices.Select(i => i));
        }

        return combinedMeshPrimitive;
    }

    public VertexPositionNormalUvTangent[] GetVertices()
    {
        if (!RealTangents.Any())
        {
            CalculateTangents();
        }

        var bufferData = new List<VertexPositionNormalUvTangent>(1_024_000);
        for (var i = 0; i < Positions.Count; ++i)
        {
            bufferData.Add(new VertexPositionNormalUvTangent(
                Positions[i],
                Normals[i],
                Uvs[i],
                RealTangents[i]));
        }

        return bufferData.ToArray();
    }

    public void CalculateTangents()
    {
        if (!_positions.Any())
        {
            return;
        }

        if (!_normals.Any())
        {
            return;
        }

        if (!_uvs.Any())
        {
            return;
        }

        for (var i = 0; i < _normals.Count; i++)
        {
            _normals[i] = Vector3.Normalize(_normals[i]);
        }

        for (var i = 0; i < _positions.Count; i += 3)
        {
            if (i >= _positions.Count || i + 1 >= _positions.Count || i + 2 >= _positions.Count ||
                i >= _uvs.Count || i + 1 >= _uvs.Count || i + 3 >= _uvs.Count)
            {
                break;
            }

            var triangle = Matrix3x3.Identity;
            triangle.Row1 = _positions[i + 0];
            triangle.Row2 = _positions[i + 1];
            triangle.Row3 = _positions[i + 2];

            var uv0 = _uvs[i + 1] - _uvs[i + 0];
            var uv1 = _uvs[i + 2] - _uvs[i + 0];

            var q1 = triangle.Row2 - triangle.Row1;
            var q2 = triangle.Row3 - triangle.Row1;

            var det = uv0.X * uv1.Y - uv1.X * uv0.Y;
            if (MathF.Abs(det) <= 0.000001f)
            {
                det = 0.000001f;
            }

            var inverseDet = 1.0f / det;

            var tangent = new Vector3(
                inverseDet * (uv1.Y * q1.X - uv0.Y * q2.X),
                inverseDet * (uv1.Y * q1.Y - uv0.Y * q2.Y),
                inverseDet * (uv1.Y * q1.Z - uv0.Y * q2.Z));
            var biTangent = new Vector3(
                inverseDet * (-uv1.X * q1.X * uv0.X * q2.X),
                inverseDet * (-uv1.X * q1.Y * uv0.X * q2.Y),
                inverseDet * (-uv1.X * q1.Z * uv0.X * q2.Z));

            _tangents[i + 0] += tangent;
            _tangents[i + 1] += tangent;
            _tangents[i + 2] += tangent;
            _biTangents[i + 0] += biTangent;
            _biTangents[i + 1] += biTangent;
            _biTangents[i + 2] += biTangent;
        }

        for (var i = 0; i < _positions.Count; i++)
        {
            if (i >= _tangents.Count)
            {
                break;
            }

            var normal = _normals[i];
            var tangent = _tangents[i];
            var biTangent = _biTangents[i];

            var realTangent = Vector3.Normalize(Vector3.Subtract(tangent, normal * Vector3.Dot(normal, tangent)));
            var realBiTangent = Vector3.Dot(Vector3.Cross( Vector3.Normalize(normal), Vector3.Normalize(tangent)), Vector3.Normalize(biTangent)) < 0.0f
                ? -1.0f
                : 1.0f;

            _realTangents[i] = new Vector4(realTangent, realBiTangent);
        }
    }

    public void AddIndices(params uint[] indices)
    {
        _indices.AddRange(indices);
        HasIndices = true;
    }

    public void AddPosition(Vector3 position)
    {
        AddVertex(position, color: null, normal: null, uv: null, tangent: null);
        VertexType = VertexType.Position;
    }

    public void AddPositionColor(
        Vector3 position,
        Vector3 color)
    {
        AddVertex(position, color, uv: null, tangent: null);
        VertexType = VertexType.PositionColor;
    }

    public void AddPositionColorNormal(
        Vector3 position,
        Vector3 color,
        Vector3 normal)
    {
        AddVertex(position, color, normal, tangent: null);
        VertexType = VertexType.PositionColorNormal;
    }

    public void AddPositionColorNormalUv(
        Vector3 position,
        Vector3 color,
        Vector3 normal,
        Vector2 uv)
    {
        AddVertex(position, color, normal, uv, tangent: null);
        VertexType = VertexType.PositionColorNormalUv;
    }

    public void AddPositionNormal(
        Vector3 position,
        Vector3 normal)
    {
        AddVertex(position, normal: normal, uv: null, tangent: null);
        VertexType = VertexType.PositionNormal;
    }

    public void AddPositionNormalUv(
        Vector3 position,
        Vector3 normal,
        Vector2 uv)
    {
        AddVertex(position, normal: normal, uv: uv, tangent: null);
        VertexType = VertexType.PositionNormalUv;
    }

    public void AddPositionNormalUvTangent(
        Vector3 position,
        Vector3 normal,
        Vector2 uv,
        Vector3 tangent)
    {
        AddVertex(position, normal: normal, uv: uv, tangent: tangent);
        VertexType = VertexType.PositionNormalUvTangent;
    }

    public void AddPositionNormalUvRealTangent(
        Vector3 position,
        Vector3 normal,
        Vector2 uv,
        Vector4 realTangent)
    {
        AddVertex(position, normal: normal, uv: uv, realTangent: realTangent);
        VertexType = VertexType.PositionNormalUvTangent;
    }

    public void AddPositionUv(
        Vector3 position,
        Vector2 uv)
    {
        AddVertex(position, uv: uv, tangent: null);
        VertexType = VertexType.PositionUv;
    }

    private void AddVertex(
        Vector3 position,
        Vector3? color = null,
        Vector3? normal = null,
        Vector2? uv = null,
        Vector3? tangent = null)
    {
        _positions.Add(position);
        _colors.Add(color ?? Vector3.Zero);
        _normals.Add(normal ?? Vector3.Zero);
        _uvs.Add(uv ?? Vector2.Zero);
        _tangents.Add(tangent ?? Vector3.Zero);
        _biTangents.Add(Vector3.Zero);
        _realTangents.Add(Vector4.Zero);
    }

    private void AddVertex(
        Vector3 position,
        Vector3? color = null,
        Vector3? normal = null,
        Vector2? uv = null,
        Vector4? realTangent = null)
    {
        _positions.Add(position);
        _colors.Add(color ?? Vector3.Zero);
        _normals.Add(normal ?? Vector3.Zero);
        _uvs.Add(uv ?? Vector2.Zero);
        _realTangents.Add(realTangent ?? Vector4.Zero);
        _biTangents.Add(Vector3.Zero);
        _tangents.Add(realTangent != null
            ? new Vector3(realTangent.Value.X, realTangent.Value.Y, realTangent.Value.Z)
            : Vector3.Zero);
    }
}