#version 460 core

layout(location = 0) in vec3 i_position;
layout(location = 1) in vec3 i_normal;
layout(location = 2) in vec2 i_uv;
layout(location = 3) in vec4 i_tangent;

layout (location = 0) out gl_PerVertex
{
    vec4 gl_Position;
};
layout(location = 0) out vec3 v_position;
layout(location = 1) out vec3 v_normal;
layout(location = 2) out vec2 v_uv;
layout(location = 3) out flat int v_model_mesh_material_id;

#include "Common.glsl"

void main()
{
    GpuModelMeshInstance modelMeshInstance = modelMeshInstanceBuffer.Instances[gl_BaseInstance];
    v_position = (modelMeshInstance.World * vec4(i_position, 1.0)).xyz;
    v_normal = normalize(inverse(transpose(mat3(modelMeshInstance.World))) * i_normal);
    v_uv = i_uv;
    v_model_mesh_material_id = modelMeshInstance.MaterialId.x;
    gl_Position = ViewProj * vec4(v_position, 1.0);
}