#version 460 core

layout(location = 0) in vec3 i_position;
layout(location = 1) in vec3 i_normal;
layout(location = 2) in vec2 i_uv;

layout(location = 0) out vec3 v_position;
layout(location = 1) out vec3 v_normal;
layout(location = 2) out vec2 v_uv;

layout(binding = 0, std140) uniform GpuConstants
{
    mat4 ViewProj;
};

struct GpuObject
{
    mat4 Model;
};

layout(binding = 1, std430) readonly buffer ObjectBuffer
{
    GpuObject Objects[];
} objectBuffer;

void main()
{
    int i = gl_InstanceID + gl_BaseInstance;
    GpuObject object = objectBuffer.Objects[i];
    v_position = (object.Model * vec4(i_position, 1.0)).xyz;
    v_normal = normalize(inverse(transpose(mat3(object.Model))) * i_normal);
    v_uv = i_uv;
    gl_Position = ViewProj * vec4(v_position, 1.0);
}