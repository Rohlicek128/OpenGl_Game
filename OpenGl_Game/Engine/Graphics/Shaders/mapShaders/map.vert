#version 460 core

layout (location = 0) in vec3 aPosition;

out vec3 vFragPos;

uniform mat4 world;
uniform mat4 model;

void main(){
    vec4 viewPos = vec4(aPosition, 1.0) * model;
    vFragPos = viewPos.xyz;
    
    gl_Position = viewPos * world;
}