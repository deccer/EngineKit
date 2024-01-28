#version 460 core

#include "BaseTypes.glsl"

layout (location = 0) out gl_PerVertex
{
    vec4 gl_Position;
};
layout(location = 0) out vec3 v_position;
layout(location = 1) out vec3 v_normal;
layout(location = 2) out vec2 v_uv;
layout(location = 3) out flat int v_model_mesh_material_id;

layout(std430, binding = 3) restrict readonly buffer VertexBuffer
{
    Vertex Vertices[];
};

#include "Common.glsl"

void main()
{
    Vertex vertex = Vertices[gl_VertexID];

    GpuModelMeshInstance modelMeshInstance = ModelMeshInstances[gl_BaseInstance];
    v_position = (modelMeshInstance.World * vec4(PackedToVec3(vertex.position), 1.0)).xyz;
    v_normal = normalize(inverse(transpose(mat3(modelMeshInstance.World))) * PackedToVec3(vertex.normal));
    v_uv = PackedToVec2(vertex.uv);
    v_model_mesh_material_id = modelMeshInstance.MaterialId.x;
    gl_Position = ViewProj * vec4(v_position, 1.0);
}