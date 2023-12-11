#version 460 core

#include "BaseTypes.glsl"
#include "Common.glsl"

layout(location = 0) out gl_PerVertex
{
    vec4 gl_Position;
};
layout(location = 0) out vec4 v_color;

layout(std430, binding = 16) restrict buffer DebugAabbBuffer
{
    DrawIndirectCommand DrawCommand;
    DebugAabb Aabbs[];
} debugAabbBuffer;

vec3 CreateCube(in uint vertexID)
{
    uint b = 1 << vertexID;
    return vec3((0x287a & b) != 0, (0x02af & b) != 0, (0x31e3 & b) != 0);
}

void main()
{
    DebugAabb box = debugAabbBuffer.Aabbs[gl_InstanceID + gl_BaseInstance];
    v_color = PackedToVec4(box.color);
    vec3 vertex_position = CreateCube(gl_VertexID) - 0.5;
    vec3 world_position = vertex_position * PackedToVec3(box.extent) + PackedToVec3(box.center);
    gl_Position = ViewProj * vec4(world_position, 1.0);
}