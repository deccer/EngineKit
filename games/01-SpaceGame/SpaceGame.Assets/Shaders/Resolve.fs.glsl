#version 460 core

layout (location = 5) in vec3 v_sky_ray;

layout (location = 0) out vec4 out_color;

layout (binding = 0) uniform sampler2D t_base_color;
layout (binding = 1) uniform sampler2D t_light;

layout (binding = 2) uniform sampler2D t_depth;
layout (binding = 3) uniform sampler2D t_normal;
layout (binding = 5) uniform samplerCube t_sky;
layout (binding = 6) uniform samplerCube t_sky_convolved;

layout(std140, binding = 1) uniform BaseInformation
{
    mat4 ViewToClipMatrix;
    mat4 ClipToViewMatrix;
    mat4 WorldToViewMatrix;
    mat4 ViewToWorldMatrix;
    vec4 CameraPosition;
    vec4 CameraDirection;
};

void main()
{
    vec2 texture_size = textureSize(t_base_color, 0);
    vec2 uv = gl_FragCoord.xy / texture_size;

    vec3 base_color = textureLod(t_base_color, uv, 0).rgb;
    vec3 light = textureLod(t_light, uv, 0).rgb;
    vec3 normal = textureLod(t_normal, uv, 0).rgb;
    float depth = textureLod(t_depth, uv, 0).r;
    if (depth == 1.0)
    {
        out_color = vec4(textureLod(t_sky, v_sky_ray, 0).rgb, 1.0);
        return;
    }

    vec3 irradiance = textureLod(t_sky_convolved, normalize(normal), 0).rgb;
    vec3 diffuse = irradiance * base_color;

    //vec3 prefiltered_color = textureLod(t_sky, ...
    //vec3 environment_brdf = texture(t_environment_lut, ...
    //vec3 specular = prefiltered_color * (fresnel * environment_brdf.x * environment_brdf.y);
    //vec3 ambient =

    out_color = vec4((1.0 * diffuse) + (0.6 * light), 1.0);
}