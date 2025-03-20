#version 420 core

layout (location = 0) in vec3 aPosition;

uniform mat4 world;
uniform mat4 model;

uniform float laser;

void main(){
    gl_Position = vec4(aPosition.x, aPosition.y * laser, aPosition.z * laser, 1.0) * model * world;
}