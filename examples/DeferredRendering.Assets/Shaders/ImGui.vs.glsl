#version 460 core
#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_explicit_uniform_location : enable

layout(location = 0) in vec2 in_position;
layout(location = 1) in vec2 in_uv;
layout(location = 2) in vec4 in_color;

out gl_PerVertex
{
    vec4 gl_Position;
};
layout(location = 1) out vec4 fs_color;
layout(location = 2) out vec2 fs_uv;

layout(std140, binding = 0) uniform GlobalMatrices
{
    mat4 ProjectionMatrix;
};

void main()
{
    gl_Position = ProjectionMatrix * vec4(in_position, 0, 1);
    fs_color = in_color;
    fs_uv = in_uv;
}