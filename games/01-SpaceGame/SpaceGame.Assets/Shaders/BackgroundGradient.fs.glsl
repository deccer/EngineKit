#version 460 core

layout (location = 0) in vec4 fs_position;
layout (location = 3) in vec2 fs_uv;

layout (location = 0) out vec4 out_color;

layout(std140, binding = 1) uniform BackgroundUniforms
{
    vec4 top_color;
    vec4 bottom_color;
};

void main()
{
    out_color = vec4(mix(bottom_color.rgb, top_color.rgb, fs_uv.y), 1.0);
}