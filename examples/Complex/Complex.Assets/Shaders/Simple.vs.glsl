#version 460 core

layout(location = 0) in vec3 i_position;
layout(location = 1) in vec3 i_normal;
layout(location = 2) in vec2 i_uv;
layout(location = 3) in vec4 i_tangent;

layout (location = 0) out gl_PerVertex
{
    vec4 gl_Position;
};
layout(location = 0) out vec3 v_position;
layout(location = 1) out vec3 v_normal;
layout(location = 2) out vec2 v_uv;
layout(location = 3) out flat int v_material_index;

layout(std140, binding = 0) uniform CameraInformationBuffer
{
    mat4 projection_matrix;
    mat4 view_matrix;
};

struct InstanceInformation
{
    mat4 world_matrix;
    ivec4 material_index;
};

layout(std430, binding = 1) readonly buffer InstanceInformationBuffer
{
    InstanceInformation Instances[];
};

vec3 hsv_to_rgb(in vec3 hsv)
{
    vec3 rgb = clamp(abs(mod(hsv.x * 6.0 + vec3(0.0, 4.0, 2.0), 6.0) - 3.0) - 1.0, 0.0, 1.0);
    return hsv.z * mix(vec3(1.0), rgb, hsv.y);
}

layout(location = 0) uniform vec3 u_color;

void main()
{
    InstanceInformation instance = Instances[gl_DrawID];
    v_position = (instance.world_matrix * vec4(i_position, 1.0)).xyz;

    const float GOLDEN_CONJ = 0.6180339887498948482045868343656;
    vec3 color = vec4(2.0 * hsv_to_rgb(vec3(float(gl_DrawID) * u_color.x, u_color.y, u_color.z)), 1.0).xyz;
    
    v_normal = normalize(inverse(transpose(mat3(instance.world_matrix))) * i_normal);
    //v_normal = color;
    v_uv = i_uv;
    v_material_index = instance.material_index.x;
    gl_Position = projection_matrix * view_matrix * vec4(v_position, 1.0);
}