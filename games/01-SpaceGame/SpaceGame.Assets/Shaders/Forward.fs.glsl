#version 460 core

layout (location = 0) in vec4 fs_position;
layout (location = 2) in vec2 fs_uv;
layout (location = 3) in mat3 fs_tbn;
layout (location = 7) in vec4 fs_view_position;
layout (location = 8) flat in int fs_material_index;

layout (location = 0) out vec3 out_color;

layout (binding = 0) uniform sampler2DArray t_textures[10];

layout(std140, binding = 3) uniform LightInformation
{
    vec4 DirectionalLightPosition;
    vec4 DirectionalLightColor;
};

#include <EngineKit.Graphics.GpuMaterial.virtual.glsl>

layout (std430, binding = 4) readonly buffer Materials
{
    GpuMaterial b_materials[];
};

float ClampedDot(vec3 v1, vec3 v2)
{
    return clamp(dot(normalize(v1), normalize(v2)), 0.0, 1.0);
}

void main()
{
    GpuMaterial v_material = b_materials[fs_material_index];
    vec3 v_view_position = normalize(fs_view_position.xyz - fs_position.xyz);
    vec3 v_normal;
    if (v_material.NormalTextureId.x == -1)
    {
        v_normal = normalize(fs_tbn[2]);
    }
    else
    {
        v_normal = normalize(texture(t_textures[v_material.NormalTextureId.x], vec3(fs_uv, v_material.NormalTextureId.y)).rgb * 2 - 1);
    }
    v_normal = normalize(fs_tbn[0] * v_normal.x + fs_tbn[1] * v_normal.y + fs_tbn[2] * v_normal.z);
    vec3 v_diffuse = ClampedDot(DirectionalLightPosition.xyz, v_normal) * DirectionalLightColor.xyz;

    vec3 v_base_color;
    if (v_material.BaseColorTextureId.x == -1)
    {
        v_base_color = v_material.DiffuseColor.rgb;
    }
    else
    {
        v_base_color = texture(t_textures[v_material.BaseColorTextureId.x], vec3(fs_uv, v_material.BaseColorTextureId.y)).rgb;
    }

    out_color = v_diffuse * v_normal + (0.0001 * (v_diffuse * v_base_color));
}