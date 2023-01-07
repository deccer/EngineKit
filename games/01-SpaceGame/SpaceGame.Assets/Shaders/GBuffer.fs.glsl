#version 460 core

struct VsOutput
{
    vec4 fragmentPositionInWorldSpace;
    vec4 fragmentPositionInViewSpace;
    mat3 tbn;
    vec2 uv;
};

layout (location = 1) flat in int v_material_index;
layout (location = 2) in VsOutput v_input;

layout (location = 0) out vec4 out_base_color;
layout (location = 1) out vec4 out_normal;
layout (location = 2) out uint out_material_id;

layout (binding = 0) uniform sampler2DArray t_textures[10];

#include <EngineKit.Graphics.GpuMaterial.virtual.glsl>

layout (std430, binding = 4) readonly buffer MaterialBuffer
{
    GpuMaterial Materials[];
} materialBuffer;

float ClampedDot(vec3 v1, vec3 v2)
{
    return clamp(dot(normalize(v1), normalize(v2)), 0.0, 1.0);
}

vec2 float32x3_to_hemioct(in vec3 normal)
{
    vec2 p = normal.xy * (1.0 / (abs(normal.x) + abs(normal.y) + normal.z));
    return vec2(p.x + p.y, p.x - p.y);
}

vec3 hemioct_to_float32x3(vec2 encodedNormal)
{
    vec2 temp = vec2(encodedNormal.x + encodedNormal.y, encodedNormal.x - encodedNormal.y) * 0.5;
    vec3 v = vec3(temp, 1.0 - abs(temp.x) - abs(temp.y));
    return normalize(v);
}

void main()
{
    GpuMaterial material = materialBuffer.Materials[v_material_index];

    vec3 baseColor;
    if (material.BaseColorTextureId.x == -1)
    {
        baseColor = material.BaseColor.rgb;
    }
    else
    {
        baseColor = texture(t_textures[material.BaseColorTextureId.x], vec3(v_input.uv, material.BaseColorTextureId.y)).rgb;
    }

    vec3 normal;
    if (material.NormalTextureId.x == -1)
    {
        normal = normalize(v_input.tbn[2]);
    }
    else
    {
        normal = normalize(texture(t_textures[material.NormalTextureId.x], vec3(v_input.uv, material.NormalTextureId.y)).rgb * 2.0 - 1.0);
        normal = normalize(v_input.tbn[0] * normal.x + v_input.tbn[1] * normal.y + v_input.tbn[2] * normal.z);
    }

    out_base_color = vec4(baseColor, 0.0);
    out_normal = vec4(normal, 0.0);
    out_material_id = v_material_index;
}