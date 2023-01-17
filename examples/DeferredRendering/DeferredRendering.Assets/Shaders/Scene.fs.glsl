#version 460 core

#extension GL_ARB_bindless_texture : enable

layout(location = 0) in vec3 v_position;
layout(location = 1) in vec3 v_normal;
layout(location = 2) in vec2 v_uv;
layout(location = 3) in flat int v_model_mesh_material_id;

layout(location = 0) out vec4 o_color;
layout(location = 1) out vec3 o_normal;

//layout(binding = 0) uniform sampler2D s_base_color;

#include <DeferredRendering.GpuMaterial.virtual.glsl>

layout(binding = 2, std430) buffer MaterialBuffer
{
    GpuMaterial[] Materials;
} materialBuffer;

void main()
{
    GpuMaterial material = materialBuffer.Materials[v_model_mesh_material_id];
    vec4 color = vec4(material.BaseColor.rgb, 1.0);
    //color *= vec4(texture(sampler2D(material.BaseColorTexture), v_uv).rgb, 1.0);

    o_color = vec4(20 + (20 * v_model_mesh_material_id), 0, 0, 1) + 0.00001 * vec4(vec3(0.1) + color.rgb, v_model_mesh_material_id);
    o_normal = normalize(v_normal);
}