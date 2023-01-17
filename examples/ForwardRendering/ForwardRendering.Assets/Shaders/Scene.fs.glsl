#version 460 core

#extension GL_NV_bindless_texture : require
#extension GL_NV_gpu_shader5 : require // required for uint64_t type

layout(location = 0) in vec3 v_position;
layout(location = 1) in vec3 v_normal;
layout(location = 2) in vec2 v_uv;
layout(location = 3) in flat int v_model_mesh_instance_id;

layout(location = 0) out vec4 o_color;

layout(binding = 0) uniform sampler2D s_base_color;

#include <ForwardRendering.GpuMaterial.virtual.glsl>

layout(binding = 2, std140) buffer MaterialBuffer
{
    GpuMaterial[] Materials;
} materialBuffer;

void main()
{
    GpuMaterial material = materialBuffer.Materials[v_model_mesh_instance_id];
    vec4 color = material.BaseColor.rgba;
    color *= texture(s_base_color, v_uv).rgba;

    o_color = vec4(color.rgb, v_model_mesh_instance_id);
}