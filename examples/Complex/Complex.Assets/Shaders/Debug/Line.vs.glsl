#version 460 core

layout(location = 0) in vec3 i_position;
layout(location = 1) in vec3 i_color;

layout(location = 0) out gl_PerVertex
{
    vec4 gl_Position;
};
layout(location = 1) out vec3 v_color;

layout(std140, binding = 0) uniform CameraInformationBuffer
{
    mat4 projection_matrix;
    mat4 view_matrix;
};

void main()
{
    gl_Position = projection_matrix * view_matrix * vec4(i_position.xyz, 1.0);
    v_color = i_color;
}