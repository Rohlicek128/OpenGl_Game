#version 420 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec3 aNormal;
layout (location = 3) in vec3 aTangent;

out vec2 vTexCoord;
out vec3 vNormal;
out vec3 vFragPos;
out mat3 vTBN;

uniform mat4 world;
uniform mat4 model;
uniform mat4 inverseModel;
uniform float textureScaling;

void main(){
    gl_Position = vec4(aPosition, 1.0) * model * world;
    
    vTexCoord = aTexCoord * (vec2(length(vec3(model[0])), length(vec3(model[1]))) * textureScaling + (1 - textureScaling));
    vNormal = aNormal * mat3(inverseModel);
    vFragPos = vec3(vec4(aPosition, 1.0) * model);
    
    vec3 T = normalize(vec3(vec4(aTangent, 0.0) * model));
    vec3 N = normalize(vec3(vec4(aNormal, 0.0) * model));
    vec3 B = cross(N, T);
    vTBN = mat3(T, B, N);
}