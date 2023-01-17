layout(binding = 0, std140) uniform GpuCameraConstants
{
    mat4 ViewProj;
};

struct GpuModelMeshInstance
{
    mat4 World;
    ivec4 MaterialId;
};

layout(binding = 1, std430) readonly buffer ModelMeshInstanceBuffer
{
    GpuModelMeshInstance Instances[];
} modelMeshInstanceBuffer;