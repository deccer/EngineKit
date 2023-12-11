#version 460 core

#include "BaseTypes.glsl"

struct SceneObject
{
    mat4 world_matrix;
    
    vec3 aabb_min;
    float _padding1;
    
    vec3 aabb_max;      
    float _padding2;

    uint index_count;
    uint first_index;
    int vertex_offset;
    uint material_id;    
};

layout(local_size_x = 32, local_size_y = 1, local_size_z = 1) in;

layout(std430, binding = 0) readonly buffer SceneObjectsBuffer
{
    SceneObject SceneObjects[];
};

layout(std430, binding = 1) writeonly buffer DrawIndirectBuffer
{
    DrawIndexedIndirectCommand DrawCommands[];
};

layout(std430, binding = 2) readonly buffer FrustumBuffer
{
    vec4 FrustumPlanes[];
};

layout(std430, binding = 3) writeonly restrict buffer DrawCountBuffer
{
    uint DrawCount;
};

struct GpuModelMeshInstance
{
    mat4 World;
    ivec4 MaterialId;
};

layout(std430, binding = 4) writeonly restrict buffer ModelMeshInstanceBuffer
{
    GpuModelMeshInstance ModelMeshInstances[];
};

bool IsAABBInsidePlane(in vec3 center, in vec3 extent, in vec4 plane)
{
    const vec3 normal = plane.xyz;
    const float radius = dot(extent, abs(normal));
    return (dot(normal, center) - plane.w) >= -radius;
}

int PlaneVsAABBIntersect(vec4 plane, vec3 bbMin, vec3 bbMax)
{
    vec3 min;
    vec3 max;

    max.x = (plane.x >= 0.0f) ? bbMin.x : bbMax.x;
    max.y = (plane.y >= 0.0f) ? bbMin.y : bbMax.y;
    max.z = (plane.z >= 0.0f) ? bbMin.z : bbMax.z;
    min.x = (plane.x >= 0.0f) ? bbMin.x : bbMax.x;
    min.y = (plane.y >= 0.0f) ? bbMin.y : bbMax.y;
    min.z = (plane.z >= 0.0f) ? bbMin.z : bbMax.z;

    float distance = dot(plane.xyz, max);

    if (distance + plane.w > 0.0f)
    {
        return 0; // front
    }

    distance = dot(plane.xyz, min);

    if (distance + plane.w < 0.0f)
    {
        return 1; // back
    }

    return 2; // intersecting
}

void main()
{
    const uint object_id = gl_GlobalInvocationID.x;
    if (object_id >= 200)
    {
        return;
    }

    SceneObject scene_object = SceneObjects[object_id];

    const mat4 transform = scene_object.world_matrix;
    const vec3 aabb_min = scene_object.aabb_min;
    const vec3 aabb_max = scene_object.aabb_max;
    const vec3 aabb_center = (aabb_min + aabb_max) / 2.0;
    const vec3 aabb_extent = aabb_max - aabb_center;
    const vec3 world_aabb_center = vec3(transform * vec4(aabb_center, 1.0));
    const vec3 right = vec3(transform[0]) * aabb_extent.x;
    const vec3 up = vec3(transform[1]) * aabb_extent.y;
    const vec3 forward = vec3(-transform[2]) * aabb_extent.z;

    const vec3 world_extent = vec3(
        abs(dot(vec3(1.0, 0.0, 0.0), right)) +
        abs(dot(vec3(1.0, 0.0, 0.0), up)) +
        abs(dot(vec3(1.0, 0.0, 0.0), forward)),

        abs(dot(vec3(0.0, 1.0, 0.0), right)) +
        abs(dot(vec3(0.0, 1.0, 0.0), up)) +
        abs(dot(vec3(0.0, 1.0, 0.0), forward)),

        abs(dot(vec3(0.0, 0.0, 1.0), right)) +
        abs(dot(vec3(0.0, 0.0, 1.0), up)) +
        abs(dot(vec3(0.0, 0.0, 1.0), forward)));
    
    bool is_in_frustum = false;
    
    const vec3 aabb_min2 = vec3(transform * vec4(scene_object.aabb_min, 1.0));
    const vec3 aabb_max2 = vec3(transform * vec4(scene_object.aabb_max, 1.0));
    
    for (uint i = 0; i < 6; ++i)
    {
        //if (!IsAABBInsidePlane(world_aabb_center, world_extent, FrustumPlanes[i]))
        if (PlaneVsAABBIntersect(FrustumPlanes[i], aabb_min2, aabb_max2) == 2) 
        {
            is_in_frustum = true;
        }
    }

    if (is_in_frustum)
    {
        uint commandIndex = atomicAdd(DrawCount, 1);
        DrawCommands[commandIndex].index_count = scene_object.index_count;
        DrawCommands[commandIndex].instance_count = 1;
        DrawCommands[commandIndex].first_index = scene_object.first_index;
        DrawCommands[commandIndex].vertex_offset = scene_object.vertex_offset;
        DrawCommands[commandIndex].first_instance = 0;

        ModelMeshInstances[commandIndex].World = scene_object.world_matrix;
        ModelMeshInstances[commandIndex].MaterialId = ivec4(scene_object.material_id, 0, 0, 0);
    }
}