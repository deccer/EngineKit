#version 460 core

layout(location = 0) in vec3 i_position;
layout(location = 2) in vec3 i_normal;
layout(location = 3) in vec2 i_uv;
layout(location = 4) in vec4 i_tangent;

layout (location = 0) out gl_PerVertex
{
    vec4 gl_Position;
};
layout(location = 0) out vec3 v_position;
layout(location = 1) out vec2 v_uv;
layout(location = 2) out flat int v_mesh_material_id;
layout(location = 3) out mat3 v_tbn;

layout(binding = 0, std140) uniform CameraInformation
{
    mat4 ProjectionMatrix;
    mat4 ViewMatrix;
    vec4 Viewport;
    vec4 CameraPosition; // xyz = position, w = fieldOfView
    vec4 CameraDirection; // xyz = direction, w = aspectRatio
} cameraInformation;

struct GpuModelMeshInstance
{
    mat4 WorldMatrix;
    ivec4 MaterialId;
};

layout(binding = 1, std430) readonly buffer InstanceBuffer
{
    GpuModelMeshInstance Instances[];
} instanceBuffer;

mat3 CreateTbnMatrix(in mat3 transform)
{
    const vec3 bitangent = cross(normalize(i_normal), normalize(i_tangent.xyz)) * i_tangent.w;
    const vec3 T = normalize(transform * i_tangent.xyz);
    const vec3 B = normalize(transform * bitangent);
    const vec3 N = normalize(transform * i_normal);
    return mat3(T, B, N);
}

void main()
{
    GpuModelMeshInstance modelMeshInstance = instanceBuffer.Instances[gl_BaseInstance + gl_DrawID];

    v_mesh_material_id = modelMeshInstance.MaterialId.x;
    v_position = (modelMeshInstance.WorldMatrix * vec4(i_position, 1.0)).xyz;
    v_uv = i_uv;
    v_tbn = CreateTbnMatrix(mat3(transpose(inverse(mat3(modelMeshInstance.WorldMatrix)))));
    
    gl_Position = cameraInformation.ProjectionMatrix * cameraInformation.ViewMatrix * vec4(v_position, 1.0);
}