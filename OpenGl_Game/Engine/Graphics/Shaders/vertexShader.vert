#version 420 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec3 aNormal;

out vec2 vTexCoord;
out vec3 vNormal;
out vec3 vFragPos;

uniform mat4 world;
uniform mat4 model;
uniform mat4 inverseModel;

void main(){
    gl_Position = vec4(aPosition, 1.0) * model * world;
    
    vTexCoord = aTexCoord * vec2(length(vec3(model[0])), length(vec3(model[1])));
    vNormal = aNormal * mat3(inverseModel);
    vFragPos = vec3(vec4(aPosition, 1.0) * model);
}