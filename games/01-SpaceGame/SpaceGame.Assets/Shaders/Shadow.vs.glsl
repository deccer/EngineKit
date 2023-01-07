#version 460 core

layout (location = 0) in vec3 i_position;

layout (location = 0) out gl_PerVertex
{
    vec4 gl_Position;
};

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

struct GpuInstance
{
    mat4 Transform;
    ivec4 MaterialIndex;
};

layout(std430, binding = 2) readonly buffer InstanceBuffer
{
    GpuInstance Instances[];
} instanceBuffer;

void main()
{
    gl_Position = ViewToClipMatrix * WorldToViewMatrix * instanceBuffer.Instances[gl_DrawID].Transform * vec4(i_position, 1.0);
}