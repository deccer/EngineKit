#version 460 core

layout(location = 0) in vec3 i_position;
layout(location = 1) in vec3 i_color;
layout(location = 0) out gl_PerVertex
{
    vec4 gl_Position;
};
layout(location = 1) out vec3 v_color;

#include "Common.glsl"

void main()
{
    gl_Position = ViewProj * vec4(i_position.xyz, 1.0);
    v_color = i_color;
}