#version 460 core

layout(location = 0) in vec3 i_position;
layout(location = 2) in vec3 i_normal;
layout(location = 3) in vec2 i_uv;
layout(location = 4) in vec4 i_tangent;

layout (location = 0) out gl_PerVertex
{
    vec4 gl_Position;
};
layout(location = 0) out vec3 v_position;
layout(location = 1) out vec3 v_normal;
layout(location = 2) out vec2 v_uv;

layout(location = 0) uniform mat4 u_world_matrix;

layout(binding = 0) uniform CameraInformationBuffer
{
    mat4 projection_matrix;
    mat4 view_matrix;
};

struct InstanceInformation
{
    mat4 world_matrix;
};

layout(binding = 1) readonly buffer InstanceInformationBuffer
{
    InstanceInformation Instances[];
};

void main()
{
    InstanceInformation instance = Instances[gl_DrawID];
    v_position = (instance.world_matrix * vec4(i_position, 1.0)).xyz;
    v_normal = normalize(inverse(transpose(mat3(instance.world_matrix))) * i_normal);
    v_uv = i_uv;
    gl_Position = projection_matrix * view_matrix * vec4(v_position, 1.0);
}