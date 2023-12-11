struct PackedVec2
{
    float x;
    float y;
};

struct PackedVec3
{
    float x;
    float y;
    float z;
};

struct PackedVec4
{
    float x;
    float y;
    float z;
    float w;
};

vec2 PackedToVec2(in PackedVec2 v)
{
    return vec2(v.x, v.y);
}

PackedVec2 Vec2ToPacked(in vec2 v)
{
    return PackedVec2(v.x, v.y);
}

vec3 PackedToVec3(in PackedVec3 v)
{
    return vec3(v.x, v.y, v.z);
}

PackedVec3 Vec3ToPacked(in vec3 v)
{
    return PackedVec3(v.x, v.y, v.z);
}

vec4 PackedToVec4(in PackedVec4 v)
{
    return vec4(v.x, v.y, v.z, v.w);
}

PackedVec4 Vec4ToPacked(in vec4 v)
{
    return PackedVec4(v.x, v.y, v.z, v.w);
}

struct Vertex
{
    PackedVec3 position;
    PackedVec3 normal;
    PackedVec2 uv;
    PackedVec4 tangent;
};

struct DrawIndirectCommand
{
    uint vertex_count;
    uint instance_count;
    uint first_vertex;
    uint first_instance;
};

struct DrawIndexedIndirectCommand
{
    uint index_count;
    uint instance_count;
    uint first_index;
    int vertex_offset;
    uint first_instance;
};

struct DebugAabb
{
    PackedVec3 center;
    PackedVec3 extent;
    PackedVec4 color;
};

vec3 HsvToRgb(in vec3 hsv)
{
    vec3 rgb = clamp(abs(mod(hsv.x * 6.0 + vec3(0.0, 4.0, 2.0), 6.0) - 3.0) - 1.0, 0.0, 1.0);
    return hsv.z * mix(vec3(1.0), rgb, hsv.y);
}