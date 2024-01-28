#version 460 core

//layout(binding = 0) uniform samplerCube u_environment;
layout(binding = 0, rgba8) uniform readonly imageCube u_environment;
layout(binding = 1, rgba16f) uniform writeonly imageCube u_irradiance;

const float PI = 3.14159265359;

vec3 cubeCoordToWorld(ivec3 cubeCoord, vec2 cubemapSize)
{
    vec2 uv = vec2(cubeCoord.xy) / cubemapSize;
    uv = uv  * 2.0 - 1.0;// -1..1
    switch (cubeCoord.z)
    {
        case 0: return vec3(1.0, -uv.yx);// posx
        case 1: return vec3(-1.0, -uv.y, uv.x);//negx
        case 2: return vec3(uv.x, 1.0, uv.y);// posy
        case 3: return vec3(uv.x, -1.0, -uv.y);//negy
        case 4: return vec3(uv.x, -uv.y, 1.0);// posz
        case 5: return vec3(-uv.xy, -1.0);// negz
    }
    return vec3(0.0);
}

float max3(vec3 v)
{
    return max(max(v.x, v.y), v.z);
}

ivec3 texCoordToCube(vec3 texCoord, vec2 cubemapSize)
{
    vec3 abst = abs(texCoord);
    texCoord /= max3(abst);

    float cubeFace;
    vec2 uvCoord;
    if (abst.x > abst.y && abst.x > abst.z)
    {
        // x major
        float negx = step(texCoord.x, 0.0);
        uvCoord = mix(-texCoord.zy, vec2(texCoord.z, -texCoord.y), negx);
        cubeFace = negx;
    }
    else if (abst.y > abst.z)
    {
        // y major
        float negy = step(texCoord.y, 0.0);
        uvCoord = mix(texCoord.xz, vec2(texCoord.x, -texCoord.z), negy);
        cubeFace = 2.0 + negy;
    }
    else
    {
        // z major
        float negz = step(texCoord.z, 0.0);
        uvCoord = mix(vec2(texCoord.x, -texCoord.y), -texCoord.xy, negz);
        cubeFace = 4.0 + negz;
    }
    uvCoord = (uvCoord + 1.0) * 0.5;// 0..1
    uvCoord = uvCoord * cubemapSize;
    uvCoord = clamp(uvCoord, vec2(0.0), cubemapSize - vec2(1.0));

    return ivec3(ivec2(uvCoord), int(cubeFace));
}

layout (local_size_x = 16, local_size_y = 16, local_size_z = 1) in;
void main()
{
    vec2 cubeSize = imageSize(u_irradiance);
    ivec3 cubeCoord = ivec3(gl_GlobalInvocationID);
    if (any(lessThan(cubeCoord, ivec3(0))) || any(greaterThanEqual(cubeCoord.xy, cubeSize)))
    {
        return;
    }

    vec3 worldPos = cubeCoordToWorld(cubeCoord, cubeSize);
    // tangent space from origin point
    vec3 normal = normalize(worldPos);
    vec3 up = vec3(0.0, 1.0, 0.0);
    vec3 right = normalize(cross(up, normal));
    up = cross(normal, right);

    vec3 irradiance = vec3(0.0);

    //float sampleDelta = 0.025;
    float sampleDelta = 0.09;
    float nrSamples = 0.0;

    for (float phi = 0.0; phi < 2.0 * PI; phi += sampleDelta)
    {
        for (float theta = 0.0; theta < 0.5 * PI; theta += sampleDelta)
        {
            vec3 tangentSample = vec3(sin(theta) * cos(phi), sin(theta) * sin(phi), cos(theta));
            vec3 sampleVec = tangentSample.x * right + tangentSample.y * up + tangentSample.z * normal;
            ivec3 sampleCoord = texCoordToCube(sampleVec, cubeSize);
            irradiance += imageLoad(u_environment, sampleCoord).rgb * cos(theta) * sin(theta);

            //irradiance += textureLod(u_environment, sampleVec, 0).rgb * cos(theta) * sin(theta);
            nrSamples++;
        }
    }

    irradiance = PI * irradiance * (1.0 / float(nrSamples));
    imageStore(u_irradiance, cubeCoord, vec4(irradiance, 1.0));
}