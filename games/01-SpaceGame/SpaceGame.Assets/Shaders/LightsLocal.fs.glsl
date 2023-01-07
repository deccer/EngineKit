#version 460 core

struct GpuLight
{
    vec4 PositionAndType; // xyz, w = type
    vec4 Color; // xyz = color, w = radius
    vec4 Attenuation; // x = att.quadratic, y = att.linear, z = att.constant
    vec4 DirectionAndCutOff; // xyz = direction, w = cut off
};

layout (location = 1) in flat int v_light_index;
layout (location = 0) out vec4 out_light;

layout (binding = 1) uniform sampler2D t_base_color;
layout (binding = 2) uniform sampler2D t_normal;
layout (binding = 3) uniform sampler2D t_depth;
layout (binding = 4) uniform usampler2D t_material_id;
layout (binding = 5) uniform sampler2DArray t_textures[10];

layout(std140, binding = 1) uniform BaseInformation
{
    mat4 ViewToClipMatrix;
    mat4 ClipToViewMatrix;
    mat4 WorldToViewMatrix;
    mat4 ViewToWorldMatrix;
    vec4 CameraPosition;
    vec4 CameraDirection;
//    vec4 _biPadding;
};

layout(std140, binding = 3) uniform LightInformation
{
    vec4 DirectionalLightDirection;
    vec4 DirectionalLightColor;
    ivec4 LightCount;
};

struct GpuMaterial
{
    vec4 DiffuseColor;
    vec4 EmissiveColor;
    ivec4 BaseColorTextureId; // x = texturearray index, y = texturearray slice
    ivec4 NormalTextureId;
};

layout (std430, binding = 4) readonly buffer MaterialBuffer
{
    GpuMaterial Materials[];
} materialBuffer;

layout (std430, binding = 5) readonly buffer LightBuffer
{
    GpuLight Lights[];
} lightBuffer;

float CalculateDiffuseLambert(vec3 fragmentPosition, vec3 normal, vec3 lightPosition)
{
    vec3 lightDir = normalize(fragmentPosition - lightPosition);
    float diffuse = max(dot(lightDir, normal), 0.0);
    return diffuse;
}

float CalculateSpecularBlinnPhong(vec3 fragmentPosition, vec3 normal, vec3 lightPosition, vec3 cameraPosition, float attenuationExponent)
{
    vec3 lightDir = normalize(lightPosition - fragmentPosition);
    vec3 viewDir = fragmentPosition - cameraPosition;
    vec3 halfwayDir = normalize(lightDir + viewDir);

    float spec = pow(max(dot(normal, halfwayDir), 0.0), attenuationExponent);
    return spec;
}

float CalculateAttenuation(vec3 fragmentPosition, vec3 lightPosition, vec3 attenuation, float radius, float fallOff)
{
    float distance = length(lightPosition - fragmentPosition) / radius;
    //float a = clamp(1 - distance / attenuation.z, 0.0, 1.0f);

    //float a = 1.0 / (attenuation.z + attenuation.y * distance + attenuation.x * (distance * distance));
    //float a = pow(smoothstep(radius, 0, distance), fallOff);

    return distance;
}

vec3 CalculateSpotLight(GpuLight spotLight, vec3 fragment_position, vec3 fragment_normal)
{
    return vec3(0.5, 0.5, 0.0);
}

vec3 CalculatePointLight(GpuLight pointLight, vec3 fragment_position, vec3 fragment_normal)
{
    vec4 v_point_light_position_vs = ViewToClipMatrix * vec4(pointLight.PositionAndType.xyz, 1.0);
    vec3 v_point_light_position = v_point_light_position_vs.xyz / v_point_light_position_vs.w;

    vec3 diffuseLight = vec3(0.0f);
    vec3 specularLight = vec3(0.0f);

    //float attenuation = CalculateAttenuation(fragment_position, pointLight.PositionAndType.xyz, pointLight.Attenuation.xyz, pointLight.Color.w, pointLight.DirectionAndCutOff.w);
    float attenuation = CalculateAttenuation(fragment_position, v_point_light_position, pointLight.Attenuation.xyz, pointLight.Color.w, pointLight.DirectionAndCutOff.w);
    vec3 diffuse = CalculateDiffuseLambert(fragment_position, fragment_normal, v_point_light_position) * pointLight.Color.rgb;
    float specular = CalculateSpecularBlinnPhong(fragment_position, fragment_normal, v_point_light_position, CameraPosition.xyz, 8);

    diffuseLight += attenuation * diffuse;
    specularLight += attenuation * specular;

    return v_point_light_position;
}

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

void main()
{
    mat4 inverseViewProjection = inverse(WorldToViewMatrix * ViewToClipMatrix);
    vec2 textureSize = textureSize(t_normal, 0);
    vec2 uv = gl_FragCoord.xy / textureSize;
    float depth = texture(t_depth, uv).r;
    if (depth == 1.0)
    {
        discard;
    }
    vec3 fragmentPositionInWorldSpace = ReconstructFragmentWorldPositionFromDepth(depth, textureSize, inverseViewProjection);
    vec3 normal = normalize(textureLod(t_normal, uv, 0).rgb);
    uint materialIndex = textureLod(t_material_id, uv, 0).r;
    //GpuMaterial v_material = materialBuffer.Materials[materialIndex];
    vec3 baseColor = textureLod(t_base_color, uv, 0).rgb;

    GpuLight light = lightBuffer.Lights[v_light_index];

    vec3 lightValue = vec3(0.0, 0.0, 0.0);
    if (light.PositionAndType.w <= 0.0)
    {
        lightValue = CalculatePointLight(light, fragmentPositionInWorldSpace, normal);
    }
    else if (light.PositionAndType.w > 0.0)
    {
        lightValue = CalculateSpotLight(light, fragmentPositionInWorldSpace, normal);
    }

    out_light = vec4(lightValue, 1.0);
}