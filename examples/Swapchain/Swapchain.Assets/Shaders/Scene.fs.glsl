#version 460 core

layout(location = 0) out vec4 o_color;
layout(location = 1) out vec3 o_normal;

layout(location = 0) in vec3 v_position;
layout(location = 1) in vec3 v_normal;
layout(location = 2) in vec2 v_uv;
layout(location = 3) in flat int v_object_id;

layout(binding = 0) uniform sampler2D s_base_color;

#include <Swapchain.GpuMaterial.virtual.glsl>
/*
struct GpuMaterial
{
    ivec4 flagsInt;
    vec4 flagsFloat;
    vec4 baseColor;
};
*/

layout(binding = 2, std140) buffer MaterialBuffer
{
    GpuMaterial[] Materials;
} materialBuffer;

void main()
{
    GpuMaterial material = materialBuffer.Materials[v_object_id];
    vec4 color = material.BaseColor.rgba;
    color *= texture(s_base_color, v_uv).rgba;

    if (color.a < material.FlagsFloat.x)
    {
        discard;
    }

    o_color = vec4(color.rgb, v_object_id);
    o_normal = normalize(v_normal);
}