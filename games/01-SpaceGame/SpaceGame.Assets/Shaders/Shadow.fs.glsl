#version 460 core

layout(location = 0) out float out_depth;

void main()
{
    out_depth = gl_FragCoord.z;
}