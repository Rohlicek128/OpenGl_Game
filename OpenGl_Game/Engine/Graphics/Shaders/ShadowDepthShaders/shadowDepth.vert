#version 420 core

layout (location = 0) in vec3 aPosition;

uniform mat4 lightSpace;
uniform mat4 model;

void main(){
    gl_Position = vec4(aPosition, 1.0) * model * lightSpace;
    //gl_Position.z *= 0.5;
}