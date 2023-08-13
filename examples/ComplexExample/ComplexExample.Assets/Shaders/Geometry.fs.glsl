#version 460 core

#extension GL_ARB_bindless_texture : enable
#extension GL_ARB_gpu_shader_int64 : enable

layout(location = 0) in vec3 v_position;
layout(location = 1) in vec2 v_uv;
layout(location = 2) in flat int v_mesh_material_id;
layout(location = 3) in mat3 v_tbn;

layout(location = 0) out vec4 o_albedo;
layout(location = 1) out vec4 o_normal;
layout(location = 2) out vec4 o_material;
layout(location = 3) out vec4 o_emissive;

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

layout(binding = 2, std430) buffer MaterialBuffer
{
    GpuMaterial[] Materials;
} materialBuffer;

const float BayerMatrixDim = 8.0;
float Bayer_matrix[8][8] = {
    {0.0/65.0, 32.0/65.0, 8.0/65.0, 40.0/65.0, 2.0/65.0, 34.0/65.0, 10.0/65.0, 42.0/65.0},
    {48.0/65.0, 16.0/65.0, 56.0/65.0, 24.0/65.0, 50.0/65.0, 18.0/65.0, 58.0/65.0, 26.0/65.0},
    {12.0/65.0, 44.0/65.0, 4.0/65.0, 36.0/65.0, 14.0/65.0, 46.0/65.0, 6.0/65.0, 38.0/65.0},
    {60.0/65.0, 28.0/65.0, 52.0/65.0, 20.0/65.0, 62.0/65.0, 30.0/65.0, 54.0/65.0, 22.0/65.0},
    {3.0/65.0, 35.0/65.0, 11.0/65.0, 43.0/65.0, 1.0/65.0, 33.0/65.0, 9.0/65.0, 41.0/65.0},
    {51.0/65.0, 19.0/65.0, 59.0/65.0, 27.0/65.0, 49.0/65.0, 17.0/65.0, 57.0/65.0, 25.0/65.0},
    {15.0/65.0, 47.0/65.0, 7.0/65.0, 39.0/65.0, 13.0/65.0, 45.0/65.0, 5.0/65.0, 37.0/65.0},
    {63.0/65.0, 31.0/65.0, 55.0/65.0, 23.0/65.0, 61.0/65.0, 29.0/65.0, 53.0/65.0, 21.0/65.0}
};

void main()
{
    GpuMaterial material = materialBuffer.Materials[v_mesh_material_id];

    vec4 albedo = material.BaseColorFactor;
    if (material.BaseColorTexture.x != 0)
    {
        albedo = texture(sampler2D(material.BaseColorTexture), v_uv);
    }

    uvec2 viewPortPosition = uvec2(gl_FragCoord.xy);
    if(albedo.a <= (Bayer_matrix[viewPortPosition.x % 8][viewPortPosition.y % 8]))
    {
        discard;
    }

    vec3 normal = normalize(v_tbn[2]);
    if (material.NormalTexture.x != 0)
    {
        normal = (v_tbn * (texture(sampler2D(material.NormalTexture), v_uv).rgb * 2.0 - 1.0));
    }

    float occlusion = 1.0f;
    float roughness = material.RoughnessFactor;    
    float metalness = material.MetallicFactor;
        
    if (material.MetalnessRoughnessTexture.x != 0)
    {
        vec3 metalnessRoughness = texture(sampler2D(material.MetalnessRoughnessTexture), v_uv).rgb;
        occlusion = metalnessRoughness.r;
        roughness = metalnessRoughness.g * roughness;                
        metalness = metalnessRoughness.b * metalness;
    }
    
    roughness = clamp(roughness, 0.04, 1.0);
    metalness = clamp(metalness, 0.0, 1.0); 

    if (material.OcclusionTexture.x != 0)
    {
        occlusion = texture(sampler2D(material.OcclusionTexture), v_uv).r;
    }
    
    vec3 emissive = material.EmissiveFactor.rgb;
    if (material.EmissiveTexture.x != 0)
    {
        emissive = texture(sampler2D(material.EmissiveTexture), v_uv).rgb * emissive;
    }
    
    o_albedo = albedo;
    o_normal = vec4(normal, 1.0);
    o_material = vec4(occlusion, roughness, metalness, 1.0);
    o_emissive = vec4(emissive, 1.0);
}