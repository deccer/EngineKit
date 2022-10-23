#version 460 core
#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_explicit_uniform_location : enable

layout(location = 1) in vec4 fs_color;
layout(location = 2) in vec2 fs_uv;

layout(location = 0) out vec4 out_color;

layout(binding = 0) uniform sampler2D t_font;

void main()
{
    out_color = fs_color * texture(t_font, fs_uv);
}