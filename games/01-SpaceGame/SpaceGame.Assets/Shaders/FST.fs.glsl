#version 460 core

layout (location = 3) in vec2 v_uv;

layout (location = 0) out vec4 out_color;

layout (binding = 0) uniform sampler2D t_image;

void main()
{
    out_color = vec4(texture(t_image, v_uv).rgb, 1.0);
}