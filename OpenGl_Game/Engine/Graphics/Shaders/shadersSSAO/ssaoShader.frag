#version 420 core

in vec2 vTexCoord;

out float pixelColor;

uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D noiseTexture;

uniform vec3 samples[128];
uniform int kernelSize;
uniform float radius;
uniform float bias;

uniform mat4 projection;
uniform mat4 view;

uniform vec2 noiseScale;

void main(){
    //vec3 fragPos = vec3(vec4(texture(gPosition, vTexCoord).xyz, 1.0) * view);
    vec3 fragPos = texture(gPosition, vTexCoord).xyz;
    vec3 normal = normalize(texture(gNormal, vTexCoord)).xyz;
    vec3 randomVec = normalize(texture(noiseTexture, vTexCoord * noiseScale).xyz);
    
    vec3 tangent = normalize(randomVec - normal * dot(randomVec, normal));
    mat3 TBN = mat3(tangent, cross(normal, tangent), normal);
    
    float occlusion = 0.0;
    for (int i = 0; i < kernelSize; i++)
    {
        vec3 samplePos = TBN * samples[i];
        samplePos = fragPos + samplePos * radius;

        vec4 offset = vec4(samplePos, 1.0);
        offset = offset * projection;
        offset.xy /= offset.w;
        offset.xy = offset.xy * 0.5 + 0.5;

        float occlusionPos = texture(gPosition, offset.xy).z;
        float rangeCheck = smoothstep(0.0, 1.0, (radius * 2.0) / abs(fragPos.z - occlusionPos));
        
        occlusion += (occlusionPos >= samplePos.z + bias ? 1.0 : 0.0) * rangeCheck;
    }
    occlusion = 1.0 - (occlusion / kernelSize);
    pixelColor = pow(occlusion, 100.0);
    //pixelColor = vec3(normal);
}