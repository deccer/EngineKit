layout(binding = 0, std140) uniform GpuConstants
{
    mat4 ViewProj;
};

struct GpuObject
{
    mat4 World;
};

layout(binding = 1, std430) readonly buffer ObjectBuffer
{
    GpuObject Objects[];
} objectBuffer;
