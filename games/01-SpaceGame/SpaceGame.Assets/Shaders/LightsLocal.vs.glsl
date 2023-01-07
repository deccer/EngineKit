#version 460 core

layout (location = 0) out gl_PerVertex
{
    vec4 gl_Position;
};

struct GpuLight
{
    vec4 PositionAndType; // xyz, w = type
    vec4 Color; // xyz = color, w = radius
    vec4 Attenuation; // x = att.quadratic, y = att.linear, z = att.constant
    vec4 DirectionAndCutOff; // xyz = direction, w = cut off
};

layout (location = 0) in vec3 in_position;

layout (location = 1) out flat int v_light_index;

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

layout (std430, binding = 5) readonly buffer SSBO5
{
    GpuLight Lights[];
} LightBuffer;

void main()
{
    GpuLight light = LightBuffer.Lights[gl_InstanceID];
    vec3 position_ws = (in_position * light.Color.w) + light.PositionAndType.xyz;
    gl_Position = ViewToClipMatrix * WorldToViewMatrix * vec4(position_ws, 1.0);
    v_light_index = gl_InstanceID;
}