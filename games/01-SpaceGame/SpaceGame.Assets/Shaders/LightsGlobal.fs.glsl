#version 460 core

#define TWO_PI 3.14159

struct GpuLight
{
    vec4 PositionAndType; // xyz, w = type
    vec4 Color; // xyz = color, w = radius
    vec4 Attenuation; // x = att.quadratic, y = att.linear, z = att.constant
    vec4 DirectionAndCutOff; // xyz = direction, w = cut off
};

layout (location = 0) in vec2 v_uv;

layout (location = 0) out vec4 out_light;

layout (binding = 0) uniform sampler2D t_base_color;
layout (binding = 1) uniform sampler2D t_normal;
layout (binding = 2) uniform sampler2D t_depth;
layout (binding = 3) uniform usampler2D t_material_id;
layout (binding = 4) uniform sampler2D t_shadow;
layout (binding = 5) uniform sampler2D t_bluenoise_16;
layout (binding = 6) uniform sampler2D t_bluenoise_32;
layout (binding = 7) uniform sampler2DArray t_textures[10];

layout(std140, binding = 1) uniform BaseInformation
{
    mat4 ViewToClipMatrix;
    mat4 ClipToViewMatrix;
    mat4 WorldToViewMatrix;
    mat4 ViewToWorldMatrix;
    vec4 CameraPosition;
    vec4 CameraDirection;
};

layout(std140, binding = 3) uniform LightInformation
{
    mat4 DirectionalLightViewMatrix;
    mat4 DirectionalLightProjectionMatrix;
    vec4 DirectionalLightColor;
    vec4 DirectionalLightDirection;
    ivec4 LightCount;
};

layout(std140, binding = 8) uniform GpuDirectionalShadowSettings
{
    float Bias1; // 0.02
    float Bias2; // 0.0015
    float rMax; // 0.005;
    float AccumFactor; // 1.0
    int Samples; // 4
    int RandomOffset; // 10000
    int _padding1;
    int _padding2;
};

vec3 ReconstructFragmentWorldPositionFromDepth(float depth, vec2 screenSize, mat4 invViewProj)
{
    float z = depth * 2.0 - 1.0; // [0, 1] -> [-1, 1]
    vec2 position_cs = gl_FragCoord.xy / screenSize; // [0.5, screenSize] -> [0, 1]
    vec4 position_ndc = vec4(position_cs * 2.0 - 1.0, z, 1.0); // [0, 1] -> [-1, 1]

    // undo view + projection
    vec4 position_ws = invViewProj * position_ndc;
    position_ws /= position_ws.w;

    return position_ws.xyz;
}

vec2 Hammersley(uint i, uint N)
{
    return vec2(
        float(i) / float(N),
        float(bitfieldReverse(i)) * 2.3283064365386963e-10
    );
}

void main()
{
    mat4 inverseViewProjection = inverse(ViewToClipMatrix * WorldToViewMatrix);
    vec2 sizeTexture = textureSize(t_normal, 0);
    vec2 uv = gl_FragCoord.xy / sizeTexture;

    float depth = texture(t_depth, uv).r;
    if (depth == 1.0)
    {
        out_light = vec4(0, 0, 0, 1);
        return;
    }

    vec3 fragmentPositionInWorldSpace = ReconstructFragmentWorldPositionFromDepth(depth, sizeTexture, inverseViewProjection);
    vec3 normal = normalize(textureLod(t_normal, uv, 0).rgb);
    uint materialIndex = textureLod(t_material_id, uv, 0).r;
    //GpuMaterial v_material = materialBuffer.Materials[materialIndex];
    vec3 baseColor = textureLod(t_base_color, uv, 0).rgb;

    vec4 shadowUv = DirectionalLightProjectionMatrix * DirectionalLightViewMatrix * vec4(fragmentPositionInWorldSpace, 1.0);
    shadowUv = vec4(shadowUv.xyz * 0.5 + 0.5, shadowUv.w);

    if (any(lessThan(shadowUv.xyz, vec3(0))) || any(greaterThan(shadowUv.xyz, vec3(1))))
    {
        out_light = vec4(0, 1, 0, 1.0);
        return;
    }

    float nDotL = max(0.0, dot(normalize(DirectionalLightDirection.xyz), normal));

    float bias = (1.0 - nDotL) * Bias1;
    bias += Bias2;

    ivec2 uvNoise = ivec2(gl_FragCoord.xy) % textureSize(t_bluenoise_32, 0);
    vec4 noiseSample = texelFetch(t_bluenoise_32, uvNoise, 0);

    float accumShadow = 0;
    for (uint i = 0; i < Samples; i++)
    {
        vec2 xi = mod(Hammersley(i, Samples) + noiseSample.xy, 1);
        float r = xi.x * rMax;
        float theta = xi.y * 2 * TWO_PI;
        float shadowDepth = textureLod(t_shadow, shadowUv.xy + vec2(r * cos(theta), r * sin(theta)), 0).r;
        if (shadowDepth >= shadowUv.z - bias)
        {
            accumShadow += AccumFactor;
        }
    }
    float notInShadowAmount = accumShadow / Samples;

    out_light = vec4(nDotL, nDotL, nDotL, 1.0) * notInShadowAmount;
}