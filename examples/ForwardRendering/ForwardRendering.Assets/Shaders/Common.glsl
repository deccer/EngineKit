layout(binding = 0, std140) uniform GpuConstants
{
    mat4 ViewProj;
};

struct GpuModelMeshInstance
{
    mat4 World;
};

layout(binding = 1, std430) readonly buffer ModelMeshInstanceBuffer
{
    GpuModelMeshInstance Instances[];
} modelMeshInstanceBuffer;