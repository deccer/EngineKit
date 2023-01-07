#version 460 core

#include "Common.glsl"

layout (location = 0) in vec3 i_position;
layout (location = 1) in vec3 i_normal;
layout (location = 2) in vec2 i_uv;
layout (location = 3) in vec4 i_tangent;

struct VsOutput
{
    vec4 fragmentPositionInWorldSpace;
    vec4 fragmentPositionInViewSpace;
    mat3 tbn;
    vec2 uv;
};

layout (location = 0) out gl_PerVertex
{
    vec4 gl_Position;
};
layout (location = 1) out int v_material_index;
layout (location = 2) out VsOutput v_output;

struct GpuInstance
{
    mat4 Transform;
    ivec4 MaterialIndex;
};

layout(std430, binding = 2) readonly buffer InstanceBuffer
{
    GpuInstance Instances[];
} instanceBuffer;

void main()
{
    GpuInstance instance = instanceBuffer.Instances[gl_DrawID];

    vec4 worldPosition = instance.Transform * vec4(i_position, 1.0f);
    vec4 viewPosition = WorldToViewMatrix * worldPosition;

    mat3 normalMatrix = mat3(transpose(inverse(mat3(instance.Transform))));
    vec3 tangent = normalize(normalMatrix * i_tangent.xyz);
    vec3 normal = normalize(normalMatrix * i_normal);
    vec3 biTangent = normalize(cross(normal, tangent)) * i_tangent.w;

    gl_Position = ViewToClipMatrix * viewPosition;
    v_output.fragmentPositionInWorldSpace = worldPosition;
    v_output.fragmentPositionInViewSpace = viewPosition;
    v_output.uv = i_uv;
    v_output.tbn = mat3(tangent, biTangent, normal);
    v_material_index = instance.MaterialIndex.x;
}