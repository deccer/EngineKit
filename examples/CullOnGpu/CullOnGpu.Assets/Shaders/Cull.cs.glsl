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
    uint _padding3;    
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

/*
layout(std430, binding = 16) restrict buffer DebugAabbBuffer
{
    DrawIndirectCommand DrawCommand;
    DebugAabb Aabbs[];
} debugAabbBuffer;
*/

bool IsAABBInsidePlane(in vec3 center, in vec3 extent, in vec4 plane)
{
    const vec3 normal = plane.xyz;
    const float radius = dot(extent, abs(normal));
    return (dot(normal, center) - plane.w) >= -radius;
}

/*
bool TryPushDebugAabb(DebugAabb box)
{
    uint index = atomicAdd(debugAabbBuffer.DrawCommand.instance_count, 1);
    if (index >= debugAabbBuffer.Aabbs.length())
    {
        atomicAdd(debugAabbBuffer.DrawCommand.instance_count, -1);
        return false;
    }

    debugAabbBuffer.Aabbs[index] = box;
    return true;
}
*/

void main()
{
    const uint object_id = gl_GlobalInvocationID.x;
    SceneObject scene_object = SceneObjects[object_id];

    uint commandIndex = atomicAdd(DrawCount, 1);
    DrawCommands[commandIndex].index_count = scene_object.index_count;
    DrawCommands[commandIndex].instance_count = 1;
    DrawCommands[commandIndex].first_index = scene_object.first_index;
    DrawCommands[commandIndex].vertex_offset = scene_object.vertex_offset;
    DrawCommands[commandIndex].first_instance = 0;

    /*
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

    const float GOLDEN_CONJ = 0.6180339887498948482045868343656;
    vec4 color = vec4(2.0 * HsvToRgb(vec3(float(object_id) * GOLDEN_CONJ, 0.875, 0.85)), 1.0);
    
    //TryPushDebugAabb(DebugAabb(Vec3ToPacked(world_aabb_center), Vec3ToPacked(world_extent), Vec4ToPacked(color)));
    
    bool is_in_frustum = true;
    /*
    for (uint i = 0; i < 6; ++i)
    {
        if (!IsAABBInsidePlane(world_aabb_center, world_extent, FrustumPlanes[i]))
        {
            is_in_frustum = false;
        }
    }
    */

    //if (is_in_frustum)
    //{
    /*
    uint commandIndex = atomicAdd(DrawCount, 1);
    DrawCommands[commandIndex].index_count = scene_object.index_count;
    DrawCommands[commandIndex].instance_count = 1;
    DrawCommands[commandIndex].first_index = scene_object.first_index;
    DrawCommands[commandIndex].vertex_offset = scene_object.vertex_offset;
    DrawCommands[commandIndex].first_instance = 0;
    */ 
    //}
}