#version 460 core

#extension GL_ARB_bindless_texture : enable
#extension GL_ARB_gpu_shader_int64 : enable

layout(location = 0) in vec3 v_position;
layout(location = 1) in vec3 v_normal;
layout(location = 2) in vec2 v_uv;
layout(location = 3) flat in int v_material_index;

layout(location = 0) out vec4 o_color;

struct GpuMaterial
{
    vec4 BaseColorFactor;
    vec4 EmissiveFactor;
    
    float MetallicFactor;
    float RoughnessFactor;
    float AlphaCutOff;
    int AlphaMode;
    
    uvec2 BaseColorTexture;
    uvec2 NormalTexture;
    
    uvec2 MetalnessRoughnessTexture;
    uvec2 SpecularTexture;
    
    uvec2 OcclusionTexture;
    uvec2 EmissiveTexture;
};

layout(std430, binding = 2) readonly buffer MaterialBuffer
{
    GpuMaterial Materials[];
};

void main()
{
    GpuMaterial material = Materials[v_material_index];
    
    vec3 albedo;
    if (material.BaseColorTexture.x != 0)
    {
        albedo = pow(texture(sampler2D(material.BaseColorTexture), v_uv).rgb, vec3(1.0f / 2.2f));
    }
    else
    {
        albedo = v_normal * 0.5 + 0.5;
    }
    
    o_color = vec4(albedo,1.0f);
}